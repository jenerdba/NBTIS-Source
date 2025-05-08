using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class BridgeNoChange
{
    public byte StateCode { get; set; }

    public string SubmittedBy { get; set; } = null!;

    public string OldBridgeNo { get; set; } = null!;

    public string NewBridgeNo { get; set; } = null!;

    public DateTime ChangeDate { get; set; }
}
