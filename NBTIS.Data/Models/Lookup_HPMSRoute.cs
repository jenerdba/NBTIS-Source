using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Lookup_HPMSRoute
{
    public short YearRecord { get; set; }

    public byte StateCode { get; set; }

    public string RouteID { get; set; } = null!;
}
