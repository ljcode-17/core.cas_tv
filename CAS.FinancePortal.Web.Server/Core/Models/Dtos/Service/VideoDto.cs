using System;

namespace CAS.FinancePortal.Web.Server.Core.Models.Dtos.Service
{
    public class VideoDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public string Status { get; set; } = "Active";
        public int? UploadedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
