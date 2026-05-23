#nullable disable
using System;
using System.Collections.Generic;

public partial class CoreTvVideo
{
    public int Id { get; set; }

    public string FileName { get; set; }

    public string OriginalFileName { get; set; }

    public string ContentType { get; set; }

    public long FileSize { get; set; }

    public string StoragePath { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsActive { get; set; }

    public int? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
