using System;

namespace CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables
{
    public class Video
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
        public bool IsDeleted { get; set; } = false;

        public virtual UserAccess? UploadedByUserAccess { get; set; }
    }
}
