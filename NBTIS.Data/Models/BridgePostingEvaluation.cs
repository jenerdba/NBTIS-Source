using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class BridgePostingEvaluation
{
    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string LegalLoadConfig_BEP01 { get; set; } = null!;

    public decimal? LegalLoadRatingFactor_BEP02 { get; set; }

    public string? PostingType_BEP03 { get; set; }

    public string? PostingValue_BEP04 { get; set; }

    public virtual BridgePrimary BridgePrimary { get; set; } = null!;
}
