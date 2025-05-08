using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class SubmittalError
{
    public long ErrorId { get; set; }

    public long SubmitId { get; set; }

    public string SubmittedBy { get; set; } = null!;

    public int? StateCode { get; set; }

    public string? BridgeNo { get; set; }

    public string? Owner { get; set; }

    public string? ItemId { get; set; }

    public string? ErrorType { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorDescription { get; set; }

    public string? SubmittedValue { get; set; }

    public DateTime SubmitDate { get; set; }

    public string Submitter { get; set; } = null!;

    public bool Ignore { get; set; }

    public bool Reviewed { get; set; }

    public string? Comments { get; set; }

    public string? ReviewedBy { get; set; }

    public string? IgnoredBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public DateTime? IgnoredDate { get; set; }

    public string? DataSet { get; set; }

    public bool IsCorrected { get; set; }

    public virtual SubmittalLog Submit { get; set; } = null!;
}
