using System.Linq;
using AutoMapper;
using NBTIS.Core.DTOs;
using NBTIS.Data.Models;
using static NBTIS.Core.DTOs.SNBIRecord;

namespace NBTIS.Core.Mapping
{
    public class MapStageBridge : Profile
    {
        public MapStageBridge()
        {
            CreateMap<Stage_BridgePrimary, SNBIRecord>()
                .ForMember(dest => dest.SubmitId, opt => opt.MapFrom(src => src.SubmitId))
                .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
                .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
                .ForMember(dest => dest.SubmittedBy, opt => opt.MapFrom(src => src.SubmittedBy))
                .ForMember(dest => dest.BID02, opt => opt.MapFrom(src => src.BridgeName_BID02))
                .ForMember(dest => dest.BID03, opt => opt.MapFrom(src => src.PrevBridgeNo_BID03))
                .ForMember(dest => dest.BL02, opt => opt.MapFrom(src => src.CountyCode_BL02))
                .ForMember(dest => dest.BL03, opt => opt.MapFrom(src => src.PlaceCode_BL03))
                .ForMember(dest => dest.BL04, opt => opt.MapFrom(src => src.HighwayDist_BL04))
                .ForMember(d => d.BL05, o => o.MapFrom(s => (double?)s.Latitude_BL05))
                .ForMember(d => d.BL06, o => o.MapFrom(s => (double?)s.Longitude_BL06))
                .ForMember(d => d.BL07, o => o.MapFrom(s => s.BorderBridgeNo_BL07))
                .ForMember(d => d.BL08, o => o.MapFrom(s => s.BorderBridgeStateCode_BL08))
                .ForMember(d => d.BL09, o => o.MapFrom(s => s.BorderBridgeInspectResp_BL09))
                .ForMember(d => d.BL10, o => o.MapFrom(s => s.BorderBridgeLeadState_BL10))
                .ForMember(d => d.BL11, o => o.MapFrom(s => s.BridgeLocation_BL11))
                .ForMember(d => d.BL12, o => o.MapFrom(s => s.MPO_BL12))
                .ForMember(dest => dest.BCL01, opt => opt.MapFrom(src => src.Owner_BCL01))
                .ForMember(dest => dest.BCL02, opt => opt.MapFrom(src => src.MaintResp_BCL02))
                .ForMember(dest => dest.BCL03, opt => opt.MapFrom(src => src.FedTribalAccess_BCL03))
                .ForMember(dest => dest.BCL04, opt => opt.MapFrom(src => src.HistSignificance_BCL04))
                .ForMember(dest => dest.BCL05, opt => opt.MapFrom(src => src.Toll_BCL05))
                .ForMember(dest => dest.BCL06, opt => opt.MapFrom(src => src.EmergencyEvacDesig_BCL06))
                .ForMember(dest => dest.BRH01, opt => opt.MapFrom(src => src.BridgeRailings_BRH01))
                .ForMember(dest => dest.BRH02, opt => opt.MapFrom(src => src.Transitions_BRH02))
                .ForMember(dest => dest.BG01, opt => opt.MapFrom(src => (double?)src.NBISBridgeLength_BG01))
                .ForMember(dest => dest.BG02, opt => opt.MapFrom(src => (double?)src.TotalBridgeLength_BG02))
                .ForMember(dest => dest.BG03, opt => opt.MapFrom(src => (double?)src.MaxSpanLength_BG03))
                .ForMember(dest => dest.BG04, opt => opt.MapFrom(src => (double?)src.MinSpanLength_BG04))
                .ForMember(dest => dest.BG05, opt => opt.MapFrom(src => (double?)src.BridgeWidthOut_BG05))
                .ForMember(dest => dest.BG06, opt => opt.MapFrom(src => (double?)src.BridgeWidthCurb_BG06))
                .ForMember(dest => dest.BG07, opt => opt.MapFrom(src => (double?)src.LeftCurbWidth_BG07))
                .ForMember(dest => dest.BG08, opt => opt.MapFrom(src => (double?)src.RightCurbWidth_BG08))
                .ForMember(dest => dest.BG09, opt => opt.MapFrom(src => (double?)src.ApproachRoadwayWidth_BG09))
                .ForMember(dest => dest.BG10, opt => opt.MapFrom(src => src.BridgeMedian_BG10))
                .ForMember(dest => dest.BG11, opt => opt.MapFrom(src => (double?)src.Skew_BG11))
                .ForMember(dest => dest.BG12, opt => opt.MapFrom(src => src.CurvedBridge_BG12))
                .ForMember(dest => dest.BG13, opt => opt.MapFrom(src => (double?)src.MaxBridgeHeight_BG13))
                .ForMember(dest => dest.BG14, opt => opt.MapFrom(src => src.SidehillBridge_BG14))
                .ForMember(dest => dest.BG15, opt => opt.MapFrom(src => (double?)src.IrregularDeckArea_BG15))
                .ForMember(dest => dest.BG16, opt => opt.MapFrom(src => (double?)src.CalcDeckArea_BG16))
                .ForMember(dest => dest.BLR01, opt => opt.MapFrom(src => src.DesignLoad_BLR01))
                .ForMember(dest => dest.BLR02, opt => opt.MapFrom(src => src.DesignMethod_BLR02))
                .ForMember(dest => dest.BLR03, opt => opt.MapFrom(src => src.LoadRatingDate_BLR03.ToString()))
                .ForMember(dest => dest.BLR04, opt => opt.MapFrom(src => src.LoadRatingMethod_BLR04))
                .ForMember(dest => dest.BLR05, opt => opt.MapFrom(src => (double?)src.InventoryLRFactor_BLR05))
                .ForMember(dest => dest.BLR06, opt => opt.MapFrom(src => (double?)src.OperatingLRFactor_BLR06))
                .ForMember(dest => dest.BLR07, opt => opt.MapFrom(src => (double?)src.ControllingLegalLRFactor_BLR07))
                .ForMember(dest => dest.BLR08, opt => opt.MapFrom(src => src.RoutinePermitLoads_BLR08))
                .ForMember(dest => dest.BIR01, opt => opt.MapFrom(src => src.NSTMInspectReq_BIR01))
                .ForMember(dest => dest.BIR02, opt => opt.MapFrom(src => src.FatigueDetails_BIR02))
                .ForMember(dest => dest.BIR03, opt => opt.MapFrom(src => src.UnderwaterInspectReq_BIR03))
                .ForMember(dest => dest.BIR04, opt => opt.MapFrom(src => src.ComplexFeature_BIR04))
                .ForMember(dest => dest.BC01, opt => opt.MapFrom(src => src.DeckCondRate_BC01))
                .ForMember(dest => dest.BC02, opt => opt.MapFrom(src => src.SuperstrCondRate_BC02))
                .ForMember(dest => dest.BC03, opt => opt.MapFrom(src => src.SubstructCondRate_BC03))
                .ForMember(dest => dest.BC04, opt => opt.MapFrom(src => src.CulvertCondRate_BC04))
                .ForMember(dest => dest.BC05, opt => opt.MapFrom(src => src.BridgeRailCondRate_BC05))
                .ForMember(dest => dest.BC06, opt => opt.MapFrom(src => src.BridgeRailTransitCondRate_BC06))
                .ForMember(dest => dest.BC07, opt => opt.MapFrom(src => src.BridgeBearingCondRate_BC07))
                .ForMember(dest => dest.BC08, opt => opt.MapFrom(src => src.BridgeJointCondRate_BC08))
                .ForMember(dest => dest.BC09, opt => opt.MapFrom(src => src.ChannelCondRate_BC09))
                .ForMember(dest => dest.BC10, opt => opt.MapFrom(src => src.ChannelProtectCondRate_BC10))
                .ForMember(dest => dest.BC11, opt => opt.MapFrom(src => src.ScourCondRate_BC11))
                .ForMember(dest => dest.BC12, opt => opt.MapFrom(src => src.BridgeCondClass_BC12))
                .ForMember(dest => dest.BC13, opt => opt.MapFrom(src => src.LowestCondRateCode_BC13))
                .ForMember(dest => dest.BC14, opt => opt.MapFrom(src => src.NSTMInspectCond_BC14))
                .ForMember(dest => dest.BC15, opt => opt.MapFrom(src => src.UnderwaterInspectCond_BC15))
                .ForMember(dest => dest.BAP01, opt => opt.MapFrom(src => src.ApproachRoadwayAlign_BAP01))
                .ForMember(dest => dest.BAP02, opt => opt.MapFrom(src => src.OvertopLikelihood_BAP02))
                .ForMember(dest => dest.BAP03, opt => opt.MapFrom(src => src.ScourVulnerability_BAP03))
                .ForMember(dest => dest.BAP04, opt => opt.MapFrom(src => src.ScourPOA_BAP04))
                .ForMember(dest => dest.BAP05, opt => opt.MapFrom(src => src.SeismicVulnerability_BAP05))
                .ForMember(dest => dest.BW01, opt => opt.MapFrom(src => src.YearBuilt_BW01))

                // Collections (manual mapping assumed elsewhere)
                .ForMember(d => d.Elements, o => o.MapFrom(s => s.Stage_BridgeElements))
                .ForMember(d => d.Features, o => o.MapFrom(s => s.Stage_BridgeFeatures))
                .ForMember(d => d.Inspections, o => o.MapFrom(s => s.Stage_BridgeInspections))
                .ForMember(d => d.PostingEvaluations, o => o.MapFrom(s => s.Stage_BridgePostingEvaluations))
                .ForMember(d => d.PostingStatuses, o => o.MapFrom(s => s.Stage_BridgePostingStatuses))
                .ForMember(d => d.SpanSets, o => o.MapFrom(s => s.Stage_BridgeSpanSets))
                .ForMember(d => d.SubstructureSets, o => o.MapFrom(s => s.Stage_BridgeSubstructureSets))
                .ForMember(d => d.Works, o => o.MapFrom(s => s.Stage_BridgeWorks))
                .ForMember(d => d.Routes, o => o.MapFrom(s =>
                                                s.Stage_BridgeFeatures
                                                 .SelectMany(f => f.Stage_BridgeRoutes)));

            /////******* ELEMENTS ********/////

            CreateMap<Stage_BridgeElement, Element>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
                .ForMember(dest => dest.SubmitId, opt => opt.MapFrom(src => src.SubmitId))
                .ForMember(dest => dest.RecordStatus, opt => opt.MapFrom(src => src.RecordStatus))
               .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
                .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
                //.ForMember(dest => dest.BCL01, opt => opt.MapFrom(src => src.StageBridgePrimary.OwnerBcl01))
                .ForMember(dest => dest.BE01, opt => opt.MapFrom(src => src.ElementNo_BE01))
                .ForMember(dest => dest.BE02, opt => opt.MapFrom(src => src.ElementParentNo_BE02))
                .ForMember(dest => dest.BE03, opt => opt.MapFrom(src => (int?)src.ElementTotalQuantity_BE03))
                .ForMember(dest => dest.BCS01, opt => opt.MapFrom(src => (double?)src.ElementCS1_BCS01))
                .ForMember(dest => dest.BCS02, opt => opt.MapFrom(src => (double?)src.ElementCS2_BCS02))
                .ForMember(dest => dest.BCS03, opt => opt.MapFrom(src => (double?)src.ElementCS3_BCS03))
                .ForMember(dest => dest.BCS04, opt => opt.MapFrom(src => (double?)src.ElementCS4_BCS04));

            /////******* FEATURES ********/////

            CreateMap<Stage_BridgeFeature, Feature>()
             .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
             .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
             .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
             .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
             .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
             .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
             //.ForMember(d => d.BCL01, o => o.MapFrom(s => s.StageBridgePrimary.CountyCodeBl02))
             .ForMember(d => d.BF01, o => o.MapFrom(s => s.FeatureType_BF01))
             .ForMember(d => d.BF02, o => o.MapFrom(s => s.FeatureLocation_BF02))
             .ForMember(d => d.BF03, o => o.MapFrom(s => s.FeatureName_BF03))
             .ForMember(d => d.BH01, o => o.MapFrom(s => s.FuncClass_BH01))
             .ForMember(d => d.BH02, o => o.MapFrom(s => s.UrbanCode_BH02))
             .ForMember(d => d.BH03, o => o.MapFrom(s => s.NHSDesig_BH03))
             .ForMember(d => d.BH04, o => o.MapFrom(s => s.NatHwyFreightNet_BH04))
             .ForMember(d => d.BH05, o => o.MapFrom(s => s.STRAHNETDesig_BH05))
             .ForMember(d => d.BH06, o => o.MapFrom(s => s.LRSRouteID_BH06))
             .ForMember(d => d.BH07, o => o.MapFrom(s => (double?)s.LRSMilePoint_BH07))
             .ForMember(d => d.BH08, o => o.MapFrom(s => (double?)s.LanesOnHwy_BH08))
             .ForMember(d => d.BH09, o => o.MapFrom(s => (double?)s.AADT_BH09))
             .ForMember(d => d.BH10, o => o.MapFrom(s => (double?)s.AADTT_BH10))
             .ForMember(d => d.BH11, o => o.MapFrom(s => (int?)s.YearAADT_BH11))
             .ForMember(d => d.BH12, o => o.MapFrom(s => (double?)s.HwyMaxVertClearance_BH12))
             .ForMember(d => d.BH13, o => o.MapFrom(s => (double?)s.HwyMinVertClearance_BH13))
             .ForMember(d => d.BH14, o => o.MapFrom(s => (double?)s.HwyMinHorizClearanceLeft_BH14))
             .ForMember(d => d.BH15, o => o.MapFrom(s => (double?)s.HwyMinHorizClearanceRight_BH15))
             .ForMember(d => d.BH16, o => o.MapFrom(s => (double?)s.HwyMaxUsableSurfaceWidth_BH16))
             .ForMember(d => d.BH17, o => o.MapFrom(s => (int?)s.BypassDetourLength_BH17))
             .ForMember(d => d.BH18, o => o.MapFrom(s => s.CrossingBridgeNo_BH18))
             .ForMember(d => d.BRR01, o => o.MapFrom(s => s.RailroadServiceType_BRR01))
             .ForMember(d => d.BRR02, o => o.MapFrom(s => (double?)s.RailroadMinVertClearance_BRR02))
             .ForMember(d => d.BRR03, o => o.MapFrom(s => (double?)s.RailroadMinHorizOffset_BRR03))
             .ForMember(d => d.BN01, o => o.MapFrom(s => s.NavWaterway_BN01))
             .ForMember(d => d.BN02, o => o.MapFrom(s => (double?)s.NavMinVertClearance_BN02))
             .ForMember(d => d.BN03, o => o.MapFrom(s => (double?)s.MovableMaxNavVertClearance_BN03))
             .ForMember(d => d.BN04, o => o.MapFrom(s => (double?)s.NavChannelWidth_BN04))
             .ForMember(d => d.BN05, o => o.MapFrom(s => (double?)s.NavChannelMinHorizClearance_BN05))
             .ForMember(d => d.BN06, o => o.MapFrom(s => s.SubstructNavProtection_BN06))
            // child collection
            .ForMember(d => d.Routes, o => o.MapFrom(s => s.Stage_BridgeRoutes));

            /////******* ROUTES ********/////

            CreateMap<Stage_BridgeRoute, Route>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
            .ForMember(d => d.FeatureId, o => o.MapFrom(s => s.FeatureID))
            .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
            .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
            .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
            .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
                .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
            .ForMember(d => d.BRT01, o => o.MapFrom(s => s.RouteDesignation_BRT01))
            .ForMember(d => d.BRT02, o => o.MapFrom(s => s.RouteNumber_BRT02))
            .ForMember(d => d.BRT03, o => o.MapFrom(s => s.RouteDirection_BRT03))
            .ForMember(d => d.BRT04, o => o.MapFrom(s => s.RouteType_BRT04))
            .ForMember(d => d.BRT05, o => o.MapFrom(s => s.ServiceType_BRT05));

            /////***** INSPECTIONS *****/////

            CreateMap<Stage_BridgeInspection, Inspection>()
             .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
             .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
             .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
             .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
             .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
             .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
             .ForMember(d => d.BIE01, o => o.MapFrom(s => s.InspectionType_BIE01))
             .ForMember(d => d.BIE02, o => o.MapFrom(s => s.BeginDate_BIE02.HasValue
                 ? s.BeginDate_BIE02.Value.ToString("yyyy-MM-dd")
                 : null))
             .ForMember(d => d.BIE03, o => o.MapFrom(s => s.CompletionDate_BIE03.HasValue
                 ? s.CompletionDate_BIE03.Value.ToString("yyyy-MM-dd")
                 : null))
             .ForMember(d => d.BIE04, o => o.MapFrom(s => s.NC_BridgeInspector_BIE04))
             .ForMember(d => d.BIE05, o => o.MapFrom(s => (int?)s.InspectInterval_BIE05))
             .ForMember(d => d.BIE06, o => o.MapFrom(s => s.InspectDueDate_BIE06.HasValue
                 ? s.InspectDueDate_BIE06.Value.ToString("yyyy-MM-dd")
                 : null))
             .ForMember(d => d.BIE07, o => o.MapFrom(s => s.RBI_Method_BIE07))
             .ForMember(d => d.BIE08, o => o.MapFrom(s => s.QltyControlDate_BIE08.HasValue
                 ? s.QltyControlDate_BIE08.Value.ToString("yyyy-MM-dd")
                 : null))
             .ForMember(d => d.BIE09, o => o.MapFrom(s => s.QltyAssuranceDate_BIE09.HasValue
                 ? s.QltyAssuranceDate_BIE09.Value.ToString("yyyy-MM-dd")
                 : null))
             .ForMember(d => d.BIE10, o => o.MapFrom(s => s.InspectDataUpdateDate_BIE10.HasValue
                 ? s.InspectDataUpdateDate_BIE10.Value.ToString("yyyy-MM-dd")
                 : null))
             .ForMember(d => d.BIE11, o => o.MapFrom(s => s.InspectionNote_BIE11))
             .ForMember(d => d.BIE12, o => o.MapFrom(s => s.InspectEquipment_BIE12));


            /////***** POSTING EVALUATION *****/////

            CreateMap<Stage_BridgePostingEvaluation, PostingEvaluation>()
           .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
           .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
           .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
           .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
          .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
                .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
           .ForMember(d => d.BEP01, o => o.MapFrom(s => s.LegalLoadConfig_BEP01))
           .ForMember(d => d.BEP02, o => o.MapFrom(s => s.LegalLoadRatingFactor_BEP02))
           .ForMember(d => d.BEP03, o => o.MapFrom(s => s.PostingType_BEP03))
           .ForMember(d => d.BEP04, o => o.MapFrom(s => s.PostingValue_BEP04));

            /////***** POSTING STATUS *****/////

            CreateMap<Stage_BridgePostingStatus, PostingStatus>()
           .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
           .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
           .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
           .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
           .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
           .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
           .ForMember(d => d.BPS01, o => o.MapFrom(s => s.LoadPostingStatus_BPS01))
           .ForMember(d => d.BPS02, o => o.MapFrom(s => s.PostingStatusChangeDate_BPS02.HasValue
                                                                   ? s.PostingStatusChangeDate_BPS02.Value.ToString("yyyy-MM-dd")
                                                                   : null));

            /////***** SPAN SETS *****/////

            CreateMap<Stage_BridgeSpanSet, SpanSet>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
            .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
            .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
            .ForMember(dest => dest.BL01, opt => opt.MapFrom(src => src.StateCode_BL01))
            .ForMember(dest => dest.BID01, opt => opt.MapFrom(src => src.BridgeNo_BID01))
            .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
            .ForMember(d => d.BSP01, o => o.MapFrom(s => s.SpanConfigDesig_BSP01))
            .ForMember(d => d.BSP02, o => o.MapFrom(s => (double?)s.NumberOfSpans_BSP02))
            .ForMember(d => d.BSP03, o => o.MapFrom(s => (double?)s.NumberOfBeamLines_BSP03))
            .ForMember(d => d.BSP04, o => o.MapFrom(s => s.SpanMaterial_BSP04))
            .ForMember(d => d.BSP05, o => o.MapFrom(s => s.SpanContinuity_BSP05))
            .ForMember(d => d.BSP06, o => o.MapFrom(s => s.SpanType_BSP06))
            .ForMember(d => d.BSP07, o => o.MapFrom(s => s.SpanProtectSystem_BSP07))
            .ForMember(d => d.BSP08, o => o.MapFrom(s => s.DeckInteraction_BSP08))
            .ForMember(d => d.BSP09, o => o.MapFrom(s => s.DeckMaterial_BSP09))
            .ForMember(d => d.BSP10, o => o.MapFrom(s => s.WearingSurface_BSP10))
            .ForMember(d => d.BSP11, o => o.MapFrom(s => s.DeckProtectSystem_BSP11))
            .ForMember(d => d.BSP12, o => o.MapFrom(s => s.DeckReinforcSystem_BSP12))
            .ForMember(d => d.BSP13, o => o.MapFrom(s => s.DeckStayInPlaceForms_BSP13));

            /////***** SUBSTRUCTURE SETS *****/////

                CreateMap<Stage_BridgeSubstructureSet, SubstructureSet>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
            .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
            .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
            .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
            .ForMember(d => d.BSB01, o => o.MapFrom(s => s.SubstructConfigDesig_BSB01))
            .ForMember(d => d.BSB02, o => o.MapFrom(s => (double?)s.NoSubstructUnits_BSB02))
            .ForMember(d => d.BSB03, o => o.MapFrom(s => s.SubstructMaterial_BSB03))
            .ForMember(d => d.BSB04, o => o.MapFrom(s => s.SubstructType_BSB04))
            .ForMember(d => d.BSB05, o => o.MapFrom(s => s.SubstructProtectSystem_BSB05))
            .ForMember(d => d.BSB06, o => o.MapFrom(s => s.FoundationType_BSB06))
            .ForMember(d => d.BSB07, o => o.MapFrom(s => s.FoundationProtectSystem_BSB07));


            /////***** WORK *****/////

            CreateMap<Stage_BridgeWork, Work>()
           .ForMember(d => d.Id, o => o.MapFrom(s => s.ID))
           .ForMember(d => d.SubmitId, o => o.MapFrom(s => s.SubmitId))
           .ForMember(d => d.SubmittedBy, o => o.MapFrom(s => s.SubmittedBy))
           .ForMember(d => d.RecordStatus, o => o.MapFrom(s => s.RecordStatus))
           .ForMember(d => d.BW02, o => o.MapFrom(s => (int?)s.YearWorkPerformed_BW02))
           .ForMember(d => d.BW03, o => o.MapFrom(s => s.WorkPerformed_BW03));
        }







    }


}
