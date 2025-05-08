using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class SubmittalComment
{
    public int Id { get; set; }

    public long SubmitId { get; set; }

    public string CommentType { get; set; } = null!;

    public string CommentText { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual SubmittalLog Submit { get; set; } = null!;
}
