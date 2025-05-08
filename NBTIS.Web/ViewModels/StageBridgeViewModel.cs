using NBTIS.Data.Models;
using NBTIS.Core.DTOs;

namespace NBTIS.Web.ViewModels
{
    public class StageBridgeItem
    {
        public long SubmitId { get; set; }

        public byte StateCodeBl01 { get; set; }

        public string BridgeNoBid01 { get; set; } = null!;

        public string SubmittedBy { get; set; } = null!;

        public string? BridgeNameBid02 { get; set; }

        public string? PrevBridgeNoBid03 { get; set; }

        public short CountyCodeBl02 { get; set; }

        public short? PlaceCodeBl03 { get; set; }

        public string? HighwayDistBl04 { get; set; }

        public decimal? LatitudeBl05 { get; set; }

        public decimal? LongitudeBl06 { get; set; }

        public string? BorderBridgeNoBl07 { get; set; }

        public byte? BorderBridgeStateCodeBl08 { get; set; }

        public string? BorderBridgeInspectRespBl09 { get; set; }

        public byte? BorderBridgeLeadStateBl10 { get; set; }

        public string? BridgeLocationBl11 { get; set; }

        public string? MpoBl12 { get; set; }

        public string OwnerBcl01 { get; set; } = null!;

        public string MaintRespBcl02 { get; set; } = null!;

        public string? FedTribalAccessBcl03 { get; set; }

        public string? HistSignificanceBcl04 { get; set; }

        public string? TollBcl05 { get; set; }

        public string? EmergencyEvacDesigBcl06 { get; set; }

        public string? BridgeRailingsBrh01 { get; set; }

        public string? TransitionsBrh02 { get; set; }

        public decimal? NbisbridgeLengthBg01 { get; set; }

        public decimal? TotalBridgeLengthBg02 { get; set; }

        public decimal? MaxSpanLengthBg03 { get; set; }

        public decimal? MinSpanLengthBg04 { get; set; }

        public decimal? BridgeWidthOutBg05 { get; set; }

        public decimal? BridgeWidthCurbBg06 { get; set; }

        public decimal? LeftCurbWidthBg07 { get; set; }

        public decimal? RightCurbWidthBg08 { get; set; }

        public decimal? ApproachRoadwayWidthBg09 { get; set; }

        public string? BridgeMedianBg10 { get; set; }

        public byte? SkewBg11 { get; set; }

        public string? CurvedBridgeBg12 { get; set; }

        public short? MaxBridgeHeightBg13 { get; set; }

        public string? SidehillBridgeBg14 { get; set; }

        public decimal? IrregularDeckAreaG15 { get; set; }

        public decimal? CalculatedDeckAreaG16 { get; set; }

        public string? DesignLoadBlr01 { get; set; }

        public string? DesignMethodBlr02 { get; set; }

        public DateOnly? LoadRatingDateBlr03 { get; set; }

        public string? LoadRatingMethodBlr04 { get; set; }

        public decimal? InventoryLrfactorBlr05 { get; set; }

        public decimal? OperatingLrfactorBlr06 { get; set; }

        public decimal? ControllingLegalLrfactorBlr07 { get; set; }

        public string? RoutinePermitLoadsBlr08 { get; set; }

        public string? NstminspectReqBir01 { get; set; }

        public string? FatigueDetailsBir02 { get; set; }

        public string? UnderwaterInspectReqBir03 { get; set; }

        public string? ComplexFeatureBir04 { get; set; }

        public string? DeckCondRateBc01 { get; set; }

        public string? SuperstrCondRateBc02 { get; set; }

        public string? SubstructCondRateBc03 { get; set; }

        public string? CulvertCondRateBc04 { get; set; }

        public string? BridgeRailCondRateBc05 { get; set; }

        public string? BridgeRailTransitCondRateBc06 { get; set; }

        public string? BridgeBearingCondRateBc07 { get; set; }

        public string? BridgeJointCondRateBc08 { get; set; }

        public string? ChannelCondRateBc09 { get; set; }

        public string? ChannelProtectCondRateBc10 { get; set; }

        public string? ScourCondRateBc11 { get; set; }

        public string? BridgeCondClassBc12 { get; set; }

        public string? LowestCondRateCodeBc13 { get; set; }

        public string? NstminspectCondBc14 { get; set; }

        public string? UnderwaterInspectCondBc15 { get; set; }

        public string? ApproachRoadwayAlignBap01 { get; set; }

        public string? OvertopLikelihoodBap02 { get; set; }

        public string? ScourVulnerabilityBap03 { get; set; }

        public string? ScourPlanOfActionBAp04 { get; set; }

        public string? SeismicVulnerabilityAp05 { get; set; }

        public short? YearBuiltW1 { get; set; }

        public virtual Lookup_County LookupCounty { get; set; } = null!;

        public virtual ICollection<Stage_BridgeElement> StageBridgeElements { get; set; } = new List<Stage_BridgeElement>();

        public virtual ICollection<Stage_BridgeFeature> StageBridgeFeatures { get; set; } = new List<Stage_BridgeFeature>();

        public virtual ICollection<Stage_BridgeInspection> StageBridgeInspections { get; set; } = new List<Stage_BridgeInspection>();

        public virtual ICollection<Stage_BridgePostingEvaluation> StageBridgePostingEvaluations { get; set; } = new List<Stage_BridgePostingEvaluation>();

        public virtual ICollection<Stage_BridgePostingStatus> StageBridgePostingStatuses { get; set; } = new List<Stage_BridgePostingStatus>();

        public virtual ICollection<Stage_BridgeSpanSet> StageBridgeSpanSets { get; set; } = new List<Stage_BridgeSpanSet>();

        public virtual ICollection<Stage_BridgeSubstructureSet> StageBridgeSubstructureSets { get; set; } = new List<Stage_BridgeSubstructureSet>();

        public virtual ICollection<Stage_BridgeWork> StageBridgeWorks { get; set; } = new List<Stage_BridgeWork>();

        public virtual Lookup_State StateCodeBl01Navigation { get; set; } = null!;

        public virtual SubmittalLog Submit { get; set; } = null!;
    }


}

