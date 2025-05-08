using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Stage_BridgeRoute
{
    public long ID { get; set; }

    public long FeatureID { get; set; }

    public long SubmitId { get; set; }

    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string? RouteDesignation_BRT01 { get; set; }

    public string? RouteNumber_BRT02 { get; set; }

    public string? RouteDirection_BRT03 { get; set; }

    public string? RouteType_BRT04 { get; set; }

    public string? ServiceType_BRT05 { get; set; }

    public string RecordStatus { get; set; } = null!;

    public virtual Stage_BridgeFeature Feature { get; set; } = null!;
}
