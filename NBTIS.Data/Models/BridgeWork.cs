using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class BridgeWork
{
    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public short YearWorkPerformed_BW02 { get; set; }

    public string? WorkPerformed_BW03 { get; set; }

    public virtual BridgePrimary BridgePrimary { get; set; } = null!;
}
