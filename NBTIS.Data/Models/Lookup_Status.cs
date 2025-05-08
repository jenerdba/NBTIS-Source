using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Lookup_Status
{
    public byte StatusCode { get; set; }

    public string StatusDescription { get; set; } = null!;
}
