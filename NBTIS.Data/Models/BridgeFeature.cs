using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class BridgeFeature
{
    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string FeatureType_BF01 { get; set; } = null!;

    public string? FeatureLocation_BF02 { get; set; }

    public string? FeatureName_BF03 { get; set; }

    public string? FuncClass_BH01 { get; set; }

    public string? UrbanCode_BH02 { get; set; }

    public string? NHSDesig_BH03 { get; set; }

    public string? NatHwyFreightNet_BH04 { get; set; }

    public string? STRAHNETDesig_BH05 { get; set; }

    public string? LRSRouteID_BH06 { get; set; }

    public decimal? LRSMilePoint_BH07 { get; set; }

    public byte? LanesOnHwy_BH08 { get; set; }

    public decimal? AADT_BH09 { get; set; }

    public decimal? AADTT_BH10 { get; set; }

    public short? YearAADT_BH11 { get; set; }

    public decimal? HwyMaxVertClearance_BH12 { get; set; }

    public decimal? HwyMinVertClearance_BH13 { get; set; }

    public decimal? HwyMinHorizClearanceLeft_BH14 { get; set; }

    public decimal? HwyMinHorizClearanceRight_BH15 { get; set; }

    public decimal? HwyMaxUsableSurfaceWidth_BH16 { get; set; }

    public decimal? BypassDetourLength_BH17 { get; set; }

    public string? CrossingBridgeNo_BH18 { get; set; }

    public string? RailroadServiceType_BRR01 { get; set; }

    public decimal? RailroadMinVertClearance_BRR02 { get; set; }

    public decimal? RailroadMinHorizOffset_BRR03 { get; set; }

    public string? NavWaterway_BN01 { get; set; }

    public decimal? NavMinVertClearance_BN02 { get; set; }

    public decimal? MovableMaxNavVertClearance_BN03 { get; set; }

    public decimal? NavChannelWidth_BN04 { get; set; }

    public decimal? NavChannelMinHorizClearance_BN05 { get; set; }

    public string? SubstructNavProtection_BN06 { get; set; }

    public virtual BridgePrimary BridgePrimary { get; set; } = null!;
}
