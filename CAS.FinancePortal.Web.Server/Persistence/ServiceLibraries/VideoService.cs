using CAS.FinancePortal.Web.Server.Core.DbContext;
using CAS.FinancePortal.Web.Server.Core.Models.Dtos.Service;
using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables;
using CAS.FinancePortal.Web.Server.Core.ServiceLibraries;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CAS.FinancePortal.Web.Server.Persistence.ServiceLibraries
{
    public class VideoService(
        ICurrentApplicationDbContext db,
        IWebHostEnvironment env,
        ILogger<VideoService> logger
        ) : IVideoService
    {
        private readonly ICurrentApplicationDbContext _db = db;
        private readonly IWebHostEnvironment _env = env;
        private readonly ILogger<VideoService> _logger = logger;

        public async Task<(bool IsSuccess, int Status, string Message, VideoDto resultData)> UploadVideoAsync(IFormFile file, int? uploadedBy = null)
        {
            try
            {
                if (file == null || file.Length == 0) return (false, 400, "No file provided", null!);

                var uploadsFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "media", "videos");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var safeFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "-" + Path.GetFileName(file.FileName).Replace(' ', '_');
                var filePath = Path.Combine(uploadsFolder, safeFileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/media/videos/{safeFileName}";

                var entity = new Video
                {
                    FileName = file.FileName,
                    FilePath = relativePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    Status = "Active",
                    UploadedBy = uploadedBy,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await ((Microsoft.EntityFrameworkCore.DbContext)_db).Set<Video>().AddAsync(entity!);
                await ((Microsoft.EntityFrameworkCore.DbContext)_db).SaveChangesAsync();

                var dto = new VideoDto
                {
                    Id = entity.Id,
                    FileName = entity.FileName,
                    FilePath = entity.FilePath,
                    ContentType = entity.ContentType,
                    FileSize = entity.FileSize,
                    Status = entity.Status,
                    UploadedBy = entity.UploadedBy,
                    CreatedAt = entity.CreatedAt,
                    UpdatedAt = entity.UpdatedAt
                };

                return (true, 200, "Uploaded", dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error uploading video");
                return (false, 500, e.Message, null!);
            }
        }

        public async Task<(bool IsSuccess, int Status, string Message, List<VideoDto> resultData)> GetAllVideosAsync()
        {
            try
            {
                var list = await ((Microsoft.EntityFrameworkCore.DbContext)_db).Set<Video>().Where(v => !v.IsDeleted).OrderBy(v => v.CreatedAt).ToListAsync();
                var dto = list.Select(v => new VideoDto
                {
                    Id = v.Id,
                    FileName = v.FileName,
                    FilePath = v.FilePath,
                    ContentType = v.ContentType,
                    FileSize = v.FileSize,
                    Status = v.Status,
                    UploadedBy = v.UploadedBy,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                }).ToList();

                return (true, 200, "OK", dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting videos");
                return (false, 500, e.Message, new List<VideoDto>());
            }
        }

        public async Task<(bool IsSuccess, int Status, string Message, VideoDto resultData)> UpdateStatusAsync(int id, string status)
        {
            try
            {
                var set = ((Microsoft.EntityFrameworkCore.DbContext)_db).Set<Video>();
                var entity = await set.FindAsync(id);
                if (entity == null) return (false, 404, "Not found", null!);
                entity.Status = status;
                entity.UpdatedAt = DateTime.UtcNow;
                set.Update(entity);
                await ((Microsoft.EntityFrameworkCore.DbContext)_db).SaveChangesAsync();

                var dto = new VideoDto
                {
                    Id = entity.Id,
                    FileName = entity.FileName,
                    FilePath = entity.FilePath,
                    ContentType = entity.ContentType,
                    FileSize = entity.FileSize,
                    Status = entity.Status,
                    UploadedBy = entity.UploadedBy,
                    CreatedAt = entity.CreatedAt,
                    UpdatedAt = entity.UpdatedAt
                };

                return (true, 200, "Updated", dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating video status");
                return (false, 500, e.Message, null!);
            }
        }

        public async Task<(bool IsSuccess, int Status, string Message)> DeleteVideoAsync(int id)
        {
            try
            {
                var set = ((Microsoft.EntityFrameworkCore.DbContext)_db).Set<Video>();
                var entity = await set.FindAsync(id);
                if (entity == null) return (false, 404, "Not found");

                // Soft delete: hide from dashboard AND stop from playing
                entity.IsDeleted = true;
                entity.Status = "Inactive";
                entity.UpdatedAt = DateTime.UtcNow;
                set.Update(entity);
                await ((Microsoft.EntityFrameworkCore.DbContext)_db).SaveChangesAsync();

                return (true, 200, "Removed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error removing video");
                return (false, 500, e.Message);
            }
        }
    }
}
