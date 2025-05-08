using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class BridgeSpanSet
{
    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string SpanConfigDesig_BSP01 { get; set; } = null!;

    public decimal? NumberOfSpans_BSP02 { get; set; }

    public decimal? NumberOfBeamLines_BSP03 { get; set; }

    public string? SpanMaterial_BSP04 { get; set; }

    public string? SpanContinuity_BSP05 { get; set; }

    public string? SpanType_BSP06 { get; set; }

    public string? SpanProtectSystem_BSP07 { get; set; }

    public string? DeckInteraction_BSP08 { get; set; }

    public string? DeckMaterial_BSP09 { get; set; }

    public string? WearingSurface_BSP10 { get; set; }

    public string? DeckProtectSystem_BSP11 { get; set; }

    public string? DeckReinforcSystem_BSP12 { get; set; }

    public string? DeckStayInPlaceForms_BSP13 { get; set; }

    public virtual BridgePrimary BridgePrimary { get; set; } = null!;
}
