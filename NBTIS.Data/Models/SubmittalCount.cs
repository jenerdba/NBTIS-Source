using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class SubmittalCount
{
    public long CountId { get; set; }

    public long SubmitId { get; set; }

    public byte FileType { get; set; }

    public int? TotalCount { get; set; }

    public int? NoChangeCount { get; set; }

    public int? ModifiedCount { get; set; }

    public int? AddedCount { get; set; }

    public int? DeletedCount { get; set; }

    public int? DuplicateCount { get; set; }
}
