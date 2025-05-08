using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Lookup_ErrorMessage
{
    public string ErrorCode { get; set; } = null!;

    public string? ErrorType { get; set; }

    public string? ErrorDescription { get; set; }

    public string? ItemId { get; set; }
}
