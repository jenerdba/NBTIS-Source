using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public short DataYear { get; set; }

    public byte StateCode { get; set; }

    public string SubmittedBy { get; set; } = null!;

    public string ReportType { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public DateTime ReportDate { get; set; }

    public string LoginId { get; set; } = null!;

    public byte[] FileContent { get; set; } = null!;

    public int? FileSize { get; set; }
}
