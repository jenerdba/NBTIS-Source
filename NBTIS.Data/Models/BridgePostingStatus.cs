using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class BridgePostingStatus
{
    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public DateOnly PostingStatusChangeDate_BPS02 { get; set; }

    public string? LoadPostingStatus_BPS01 { get; set; }

    public virtual BridgePrimary BridgePrimary { get; set; } = null!;
}
