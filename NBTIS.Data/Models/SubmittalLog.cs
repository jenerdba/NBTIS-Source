using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class SubmittalLog
{
    public long SubmitId { get; set; }

    public string SubmittedBy { get; set; } = null!;

    public bool IsPartial { get; set; }

    public byte StatusCode { get; set; }

    public byte[]? ReportContent { get; set; }

    public DateTime UploadDate { get; set; }

    public string UploadedBy { get; set; } = null!;

    public DateTime? SubmitDate { get; set; }

    public string? Submitter { get; set; }

    public DateTime? ReviewDate { get; set; }

    public string? Reviewer { get; set; }

    public DateTime? ApproveRejectDate { get; set; }

    public string? Approver { get; set; }

    public Guid UploadId { get; set; }

    public long? MergedIntoSubmitId { get; set; }

    public virtual ICollection<SubmittalLog> InverseMergedIntoSubmit { get; set; } = new List<SubmittalLog>();

    public virtual SubmittalLog? MergedIntoSubmit { get; set; }

    public virtual ICollection<Stage_BridgePrimary> Stage_BridgePrimaries { get; set; } = new List<Stage_BridgePrimary>();

    public virtual ICollection<SubmittalComment> SubmittalComments { get; set; } = new List<SubmittalComment>();

    public virtual ICollection<SubmittalError> SubmittalErrors { get; set; } = new List<SubmittalError>();

    public virtual ICollection<SubmittalFile> SubmittalFiles { get; set; } = new List<SubmittalFile>();
}
