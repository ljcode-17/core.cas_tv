using CAS.FinancePortal.Web.Server.Core.ServiceLibraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAS.FinancePortal.Web.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideosController(IVideoService videoService) : ControllerBase
    {
        private readonly IVideoService _videoService = videoService;

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _videoService.GetAllVideosAsync();
                return new JsonResult(new { result = result.resultData }) { StatusCode = result.Status };
            }
            catch (Exception e)
            {
                return new JsonResult(new { success = false, responseText = e.Message }) { StatusCode = 400 };
            }
        }

        [AllowAnonymous]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)] // 500 MB
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            try
            {
                if (file == null) return BadRequest(new { success = false, responseText = "No file provided" });

                var userIdStr = User.FindFirst("user_id")?.Value
                    ?? User.FindFirst("nameid")?.Value
                    ?? User.FindFirst("sub")?.Value
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int? userId = null;
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out var uid)) userId = uid;

                var result = await _videoService.UploadVideoAsync(file, userId);
                if (!result.IsSuccess) return StatusCode(result.Status, new { success = false, responseText = result.Message });

                return Ok(new { success = true, video = result.resultData });
            }
            catch (Exception e)
            {
                return new JsonResult(new { success = false, responseText = e.Message }) { StatusCode = 400 };
            }
        }

        public class UpdateStatusRequest { public string Status { get; set; } }

        [AllowAnonymous]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest body)
        {
            try
            {
                string status = body.Status;
                var result = await _videoService.UpdateStatusAsync(id, status);
                if (!result.IsSuccess) return StatusCode(result.Status, new { success = false, responseText = result.Message });
                return Ok(new { success = true, video = result.resultData });
            }
            catch (Exception e)
            {
                return new JsonResult(new { success = false, responseText = e.Message }) { StatusCode = 400 };
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _videoService.DeleteVideoAsync(id);
                if (!result.IsSuccess) return StatusCode(result.Status, new { success = false, responseText = result.Message });
                return Ok(new { success = true });
            }
            catch (Exception e)
            {
                return new JsonResult(new { success = false, responseText = e.Message }) { StatusCode = 400 };
            }
        }
    }
}
