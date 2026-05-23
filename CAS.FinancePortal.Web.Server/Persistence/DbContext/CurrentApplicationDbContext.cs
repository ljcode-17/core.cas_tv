using CAS.FinancePortal.Web.Server.Core.DbContext;
using CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables;
using CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Views;
using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables;
using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Views;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CAS.FinancePortal.Web.Server.Persistence.DbContext
{
    public class CurrentApplicationDbContext(DbContextOptions<CurrentApplicationDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options), ICurrentApplicationDbContext
    {
        public DbSet<VwCombinedUserInformation> VwCombinedUserInformations { get; set; }
        public DbSet<VwDepartment> VwDepartments { get; set; }
        public DbSet<VwEmployee> VwEmployees { get; set; }

        public DbSet<SystemMenu> SystemMenus { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }
        public DbSet<UserAccessType> UserAccessTypes { get; set; }
        public DbSet<Signature> Signatures { get; set; }
        public DbSet<CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables.Video> Videos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VwCombinedUserInformationConfig());
            modelBuilder.ApplyConfiguration(new VwDepartmentConfig());
            modelBuilder.ApplyConfiguration(new VwEmployeeConfig());

            modelBuilder.ApplyConfiguration(new SystemMenuConfig());
            modelBuilder.ApplyConfiguration(new UserAccessConfig());
            modelBuilder.ApplyConfiguration(new UserAccessTypeConfig());
            modelBuilder.ApplyConfiguration(new SignatureConfig());
            modelBuilder.ApplyConfiguration(new CAS.FinancePortal.Web.Server.Persistence.EntityConfigurations.CurrentApplication.Tables.VideoConfig());

            var allowedEntityTypes = new HashSet<Type>
            {
                typeof(VwCombinedUserInformation),
                typeof(VwDepartment),
                typeof(VwEmployee),
                typeof(SystemMenu),
                typeof(UserAccess),
                typeof(UserAccessType),
                typeof(Signature),
                typeof(CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables.Video)
            };

            foreach (var entityType in modelBuilder.Model.GetEntityTypes().Select(e => e.ClrType).ToList())
            {
                if (!allowedEntityTypes.Contains(entityType))
                {
                    modelBuilder.Ignore(entityType);
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                var prop = clrType.GetProperty("IsDeleted");
                if (prop != null && prop.PropertyType == typeof(bool))
                {
                    var parameter = Expression.Parameter(clrType, "e");
                    var propertyAccess = Expression.Property(parameter, prop);
                    var compare = Expression.Equal(propertyAccess, Expression.Constant(false));
                    var lambda = Expression.Lambda(compare, parameter);
                    modelBuilder.Entity(clrType).HasQueryFilter(lambda);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}