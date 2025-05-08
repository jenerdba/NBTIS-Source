using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class SubmittalFile
{
    public long FileId { get; set; }

    public long SubmitId { get; set; }

    public byte FileType { get; set; }

    public string FileName { get; set; } = null!;

    public byte[] FileContent { get; set; } = null!;

    public virtual SubmittalLog Submit { get; set; } = null!;
}
