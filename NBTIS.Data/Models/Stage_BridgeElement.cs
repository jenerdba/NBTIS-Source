using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Stage_BridgeElement
{
    public long ID { get; set; }

    public long SubmitId { get; set; }

    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string? ElementNo_BE01 { get; set; }

    public string? ElementParentNo_BE02 { get; set; }

    public int ElementTotalQuantity_BE03 { get; set; }

    public int? ElementCS1_BCS01 { get; set; }

    public int? ElementCS2_BCS02 { get; set; }

    public int? ElementCS3_BCS03 { get; set; }

    public int? ElementCS4_BCS04 { get; set; }

    public string RecordStatus { get; set; } = null!;

    public virtual Stage_BridgePrimary Stage_BridgePrimary { get; set; } = null!;
}
