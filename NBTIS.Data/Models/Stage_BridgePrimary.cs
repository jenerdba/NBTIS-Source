using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Stage_BridgePrimary
{
    public long SubmitId { get; set; }

    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string? BridgeName_BID02 { get; set; }

    public string? PrevBridgeNo_BID03 { get; set; }

    public int CountyCode_BL02 { get; set; }

    public int? PlaceCode_BL03 { get; set; }

    public string? HighwayDist_BL04 { get; set; }

    public decimal? Latitude_BL05 { get; set; }

    public decimal? Longitude_BL06 { get; set; }

    public string? BorderBridgeNo_BL07 { get; set; }

    public string? BorderBridgeStateCode_BL08 { get; set; }

    public string? BorderBridgeInspectResp_BL09 { get; set; }

    public string? BorderBridgeLeadState_BL10 { get; set; }

    public string? BridgeLocation_BL11 { get; set; }

    public string? MPO_BL12 { get; set; }

    public string Owner_BCL01 { get; set; } = null!;

    public string MaintResp_BCL02 { get; set; } = null!;

    public string? FedTribalAccess_BCL03 { get; set; }

    public string? HistSignificance_BCL04 { get; set; }

    public string? Toll_BCL05 { get; set; }

    public string? EmergencyEvacDesig_BCL06 { get; set; }

    public string? BridgeRailings_BRH01 { get; set; }

    public string? Transitions_BRH02 { get; set; }

    public decimal? NBISBridgeLength_BG01 { get; set; }

    public decimal? TotalBridgeLength_BG02 { get; set; }

    public decimal? MaxSpanLength_BG03 { get; set; }

    public decimal? MinSpanLength_BG04 { get; set; }

    public decimal? BridgeWidthOut_BG05 { get; set; }

    public decimal? BridgeWidthCurb_BG06 { get; set; }

    public decimal? LeftCurbWidth_BG07 { get; set; }

    public decimal? RightCurbWidth_BG08 { get; set; }

    public decimal? ApproachRoadwayWidth_BG09 { get; set; }

    public string? BridgeMedian_BG10 { get; set; }

    public byte? Skew_BG11 { get; set; }

    public string? CurvedBridge_BG12 { get; set; }

    public int? MaxBridgeHeight_BG13 { get; set; }

    public string? SidehillBridge_BG14 { get; set; }

    public decimal? IrregularDeckArea_BG15 { get; set; }

    public decimal? CalcDeckArea_BG16 { get; set; }

    public string? DesignLoad_BLR01 { get; set; }

    public string? DesignMethod_BLR02 { get; set; }

    public DateOnly? LoadRatingDate_BLR03 { get; set; }

    public string? LoadRatingMethod_BLR04 { get; set; }

    public decimal? InventoryLRFactor_BLR05 { get; set; }

    public decimal? OperatingLRFactor_BLR06 { get; set; }

    public decimal? ControllingLegalLRFactor_BLR07 { get; set; }

    public string? RoutinePermitLoads_BLR08 { get; set; }

    public string? NSTMInspectReq_BIR01 { get; set; }

    public string? FatigueDetails_BIR02 { get; set; }

    public string? UnderwaterInspectReq_BIR03 { get; set; }

    public string? ComplexFeature_BIR04 { get; set; }

    public string? DeckCondRate_BC01 { get; set; }

    public string? SuperstrCondRate_BC02 { get; set; }

    public string? SubstructCondRate_BC03 { get; set; }

    public string? CulvertCondRate_BC04 { get; set; }

    public string? BridgeRailCondRate_BC05 { get; set; }

    public string? BridgeRailTransitCondRate_BC06 { get; set; }

    public string? BridgeBearingCondRate_BC07 { get; set; }

    public string? BridgeJointCondRate_BC08 { get; set; }

    public string? ChannelCondRate_BC09 { get; set; }

    public string? ChannelProtectCondRate_BC10 { get; set; }

    public string? ScourCondRate_BC11 { get; set; }

    public string? BridgeCondClass_BC12 { get; set; }

    public string? LowestCondRateCode_BC13 { get; set; }

    public string? ApproachRoadwayAlign_BAP01 { get; set; }

    public string? OvertopLikelihood_BAP02 { get; set; }

    public string? ScourVulnerability_BAP03 { get; set; }

    public string? ScourPOA_BAP04 { get; set; }

    public string? SeismicVulnerability_BAP05 { get; set; }

    public short? YearBuilt_BW01 { get; set; }

    public string? NSTMInspectCond_BC14 { get; set; }

    public string? UnderwaterInspectCond_BC15 { get; set; }

    public virtual Lookup_County Lookup_County { get; set; } = null!;

    public virtual ICollection<Stage_BridgeElement> Stage_BridgeElements { get; set; } = new List<Stage_BridgeElement>();

    public virtual ICollection<Stage_BridgeFeature> Stage_BridgeFeatures { get; set; } = new List<Stage_BridgeFeature>();

    public virtual ICollection<Stage_BridgeInspection> Stage_BridgeInspections { get; set; } = new List<Stage_BridgeInspection>();

    public virtual ICollection<Stage_BridgePostingEvaluation> Stage_BridgePostingEvaluations { get; set; } = new List<Stage_BridgePostingEvaluation>();

    public virtual ICollection<Stage_BridgePostingStatus> Stage_BridgePostingStatuses { get; set; } = new List<Stage_BridgePostingStatus>();

    public virtual ICollection<Stage_BridgeSpanSet> Stage_BridgeSpanSets { get; set; } = new List<Stage_BridgeSpanSet>();

    public virtual ICollection<Stage_BridgeSubstructureSet> Stage_BridgeSubstructureSets { get; set; } = new List<Stage_BridgeSubstructureSet>();

    public virtual ICollection<Stage_BridgeWork> Stage_BridgeWorks { get; set; } = new List<Stage_BridgeWork>();

    public virtual Lookup_State StateCode_BL01Navigation { get; set; } = null!;

    public virtual SubmittalLog Submit { get; set; } = null!;
}
