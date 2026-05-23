using CAS.FinancePortal.Web.Server.Core.Applications;
using CAS.FinancePortal.Web.Server.Core.DbContext;
using CAS.FinancePortal.Web.Server.Core.ServiceLibraries;
using CAS.FinancePortal.Web.Server.Core.UnitOfWork;
using CAS.FinancePortal.Web.Server.Persistence.Applications;
using CAS.FinancePortal.Web.Server.Persistence.DbContext;
using CAS.FinancePortal.Web.Server.Persistence.ServiceLibraries;
using CAS.FinancePortal.Web.Server.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Allow large request bodies for video uploads (500 MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000; // 500 MB
});

var fileRootFolder = builder.Environment.IsDevelopment() ? builder.Environment.ContentRootPath : builder.Configuration.GetSection("BaseFileLocation").Value;

// Add services to the container.
builder.Services.AddHttpContextAccessor();

// Allow large multipart form uploads (e.g. video files) up to 500 MB
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524_288_000; // 500 MB
});


#region SuperApplicationConfig
builder.Services.AddDbContext<SuperApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SuperApplicationConnection")));
builder.Services.AddScoped<ISuperApplicationDbContext, SuperApplicationDbContext>();
#endregion

builder.Services.AddDbContext<CurrentApplicationDbContext>(
	options => options.UseSqlServer(
			builder.Configuration.GetConnectionString("DefaultConnection"),
			sqlOptions => sqlOptions.EnableRetryOnFailure())
		.EnableSensitiveDataLogging()
);

builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
// builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
// builder.Services.AddScoped<IVendorService, VendorService>();
// builder.Services.AddScoped<IRequestFormService, RequestFormService>();
// builder.Services.AddScoped<IAccountabilityService, AccountabilityService>();

builder.Services.AddScoped<ICurrentApplicationDbContext, CurrentApplicationDbContext>();
builder.Services.AddScoped<ICurrentApplicationUnitOfWork, CurrentApplicationUnitOfWork>();


builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IVideoService, VideoService>();


#region GraphServiceRegistration

builder.Services.AddScoped<IGraphService>(_ =>
{
	var azureAdConfig = builder.Configuration.GetSection("AzureAd");
	var tenantId = azureAdConfig["TenantId"]
		?? throw new InvalidOperationException("AzureAd:TenantId is missing in configuration.");
	var clientId = azureAdConfig["ClientId"]
		?? throw new InvalidOperationException("AzureAd:ClientId is missing in configuration.");
	var clientSecret = azureAdConfig["ClientSecret"]
		?? throw new InvalidOperationException("AzureAd:ClientSecret is missing in configuration.");

	return new GraphService(tenantId, clientId, clientSecret);

	
});

#endregion


builder.Services.AddControllers(options =>
{
	// Prevents non-nullable strings from being treated as implicitly [Required]
	// This allows optional fields like Signature and Notes to be null in DTOs
	options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
}).AddJsonOptions(options =>
{
	options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
	options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options =>
	{
		options.Authority = "https://172.16.254.4/cas/services/identity_server";

		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateAudience = true,
			ValidAudience = "CoreAgileSystem",
			ValidateIssuer = false,
			ValidateIssuerSigningKey = true,
		};
		// Allow internal server with self-signed certificate
		options.RequireHttpsMetadata = false;
		// Preserve JWT claim names exactly as issued (no mapping to long .NET URIs)
		options.MapInboundClaims = false;
		// Trust self-signed cert on the identity server backchannel
		options.BackchannelHttpHandler = new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
		};
	});




builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.UseDefaultFiles();
var assetsDocsPath = Path.Combine(builder.Environment.ContentRootPath, "assets_docs");
if (!Directory.Exists(assetsDocsPath))
{
    Directory.CreateDirectory(assetsDocsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(assetsDocsPath),
    RequestPath = "/cas/financeportal/assets_docs"
});

// CRITICAL FIX: Allow serving dynamically uploaded files from wwwroot (like newly uploaded videos)
app.UseStaticFiles(); 

app.MapStaticAssets();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Only redirect to HTTPS in production – the Vite dev proxy reaches the
// server over plain HTTP, and redirection would abort the connection.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


// CORS must be BEFORE auth/authorization
//app.UseCors(CorsPolicy);
app.UseCors("AllowReactApp");

// Development helper: allow simulating an authenticated user by sending
// `X-Dev-UserId` (and optional `X-Dev-UserEmail`) headers. This is only
// enabled in Development and does not affect production.
if (app.Environment.IsDevelopment())
{
	app.Use(async (context, next) =>
	{
		if (context.Request.Headers.ContainsKey("X-Dev-UserId"))
		{
			var userId = context.Request.Headers["X-Dev-UserId"].FirstOrDefault();
			var email = context.Request.Headers["X-Dev-UserEmail"].FirstOrDefault() ?? string.Empty;
			var claims = new List<Claim>
			{
				new Claim("user_id", userId ?? string.Empty),
				new Claim("email", email)
			};
			var identity = new ClaimsIdentity(claims, "Dev");
			context.User = new ClaimsPrincipal(identity);
		}

		await next();
	});
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
