using CAS.FinancePortal.Web.Server.Core.Models.Dtos.Service;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CAS.FinancePortal.Web.Server.Core.ServiceLibraries
{
    public interface IVideoService
    {
        Task<(bool IsSuccess, int Status, string Message, VideoDto resultData)> UploadVideoAsync(IFormFile file, int? uploadedBy = null);
        Task<(bool IsSuccess, int Status, string Message, List<VideoDto> resultData)> GetAllVideosAsync();
        Task<(bool IsSuccess, int Status, string Message, VideoDto resultData)> UpdateStatusAsync(int id, string status);
        Task<(bool IsSuccess, int Status, string Message)> DeleteVideoAsync(int id);
    }
}
