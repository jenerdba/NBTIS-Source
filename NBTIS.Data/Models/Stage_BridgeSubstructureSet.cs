using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Stage_BridgeSubstructureSet
{
    public long ID { get; set; }

    public long SubmitId { get; set; }

    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string? SubstructConfigDesig_BSB01 { get; set; }

    public decimal? NoSubstructUnits_BSB02 { get; set; }

    public string? SubstructMaterial_BSB03 { get; set; }

    public string? SubstructType_BSB04 { get; set; }

    public string? SubstructProtectSystem_BSB05 { get; set; }

    public string? FoundationType_BSB06 { get; set; }

    public string? FoundationProtectSystem_BSB07 { get; set; }

    public string RecordStatus { get; set; } = null!;

    public virtual Stage_BridgePrimary Stage_BridgePrimary { get; set; } = null!;
}
