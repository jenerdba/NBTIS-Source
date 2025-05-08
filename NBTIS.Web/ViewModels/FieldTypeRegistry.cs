namespace NBTIS.Web.ViewModels
{
    public static class FieldTypeRegistry
    {
        // Key: Dataset + Identifier → Value: Type
        private static readonly Dictionary<string, Type> _fieldTypes = new()
        {
            // Primary
            ["Primary:SubmitId"] = typeof(long),
            ["Primary:StateCode_BL01"] = typeof(byte),
            ["Primary:BridgeNo_BID01"] = typeof(string),
            ["Primary:SubmittedBy"] = typeof(string),
            ["Primary:BridgeName_BID02"] = typeof(string),
            ["Primary:PrevBridgeNo_BID03"] = typeof(string),
            ["Primary:CountyCode_BL02"] = typeof(short),
            ["Primary:PlaceCode_BL03"] = typeof(short),
            ["Primary:HighwayDist_BL04"] = typeof(string),
            ["Primary:Latitude_BL05"] = typeof(decimal),
            ["Primary:Latitude_BL06"] = typeof(decimal),
            ["Primary:BorderBridgeNo_BL07"] = typeof(string),
            ["Primary:BorderBridgeStateCode_BL08"] = typeof(byte),
            ["Primary:BorderBridgeInspectResp_BL09"] = typeof(string),
            ["Primary:BorderBridgeLeadState_BL10"] = typeof(string),
            ["Primary:BridgeLocation_BL1"] = typeof(string),
            ["Primary:MPO_BL12"] = typeof(string),
            ["Primary:OwnerBcl01"] = typeof(string),
            ["Primary:MaintResp_BCL02"] = typeof(string),
            ["Primary:FedTribalAccess_BCL03"] = typeof(string),
            ["Primary:HistSignificance_BCL04"] = typeof(string),
            ["Primary:Toll_BCL05"] = typeof(string),
            ["Primary:EmergencyEvacDesig_BCL06"] = typeof(string),
            ["Primary:BridgeRailings_BRH01"] = typeof(string),
            ["Primary:Transitions_BRH02"] = typeof(string),
            ["Primary:NBISBridgeLength_BG01"] = typeof(decimal),
            ["Primary:TotalBridgeLength_BG02"] = typeof(decimal),
            ["Primary:MaxSpanLength_BG03"] = typeof(decimal),
            ["Primary:MinSpanLength_BG04"] = typeof(decimal),
            ["Primary:BridgeWidthOut_BG05"] = typeof(decimal),
            ["Primary:BridgeWidthCurb_BG06"] = typeof(decimal),
            ["Primary:LeftCurbWidth_BG07"] = typeof(decimal),
            ["Primary:RightCurbWidth_BG08"] = typeof(decimal),
            ["Primary:ApproachRoadwayWidth_BG09"] = typeof(decimal),
            ["Primary:BridgeMedian_BG10"] = typeof(string),
            ["Primary:Skew_BG11"] = typeof(byte),
            ["Primary:CurvedBridge_BG12"] = typeof(string),
            ["Primary:MaxBridgeHeight_BG13"] = typeof(decimal),
            ["Primary:SidehillBridge_BG14"] = typeof(string),
            ["Primary:IrregularDeckArea_BG15"] = typeof(decimal),
            ["Primary:CalculatedDeckArea_BG16"] = typeof(decimal),
            ["Primary:DesignLoad_BLR01"] = typeof(string),
            ["Primary:DesignMethod_BLR02"] = typeof(string),
            ["Primary:LoadRatingDate_BLR03"] = typeof(DateOnly),
            ["Primary:LoadRatingMethod_BLR04"] = typeof(string),
            ["Primary:InventoryLRFactor_BLR05"] = typeof(decimal),
            ["Primary:OperatingLRFactor_BLR06"] = typeof(decimal),
            ["Primary:ControllingLegalLRFactor_BLR07"] = typeof(decimal),
            ["Primary:RoutinePermitLoads_BLR08"] = typeof(string),
            ["Primary:NSTMInspectReq_BIR01"] = typeof(string),
            ["Primary:FatigueDetails_BIR02"] = typeof(string),
            ["Primary:UnderwaterInspectReq_BIR03"] = typeof(string),
            ["Primary:ComplexFeature_BIR04"] = typeof(string),
            ["Primary:DeckCondRate_BC01"] = typeof(string),
            ["Primary:SuperstrCondRate_BC02"] = typeof(string),
            ["Primary:SubstructCondRate_BC03"] = typeof(string),
            ["Primary:CulvertCondRate_BC04"] = typeof(string),
            ["Primary:BridgeRailCondRate_BC05"] = typeof(string),
            ["Primary:BridgeRailTransitCondRate_BC06"] = typeof(string),
            ["Primary:BridgeBearingCondRate_BC07"] = typeof(string),
            ["Primary:BridgeJointCondRate_BC08"] = typeof(string),
            ["Primary:ChannelCondRate_BC09"] = typeof(string),
            ["Primary:ChannelProtectCondRate_BC10"] = typeof(string),
            ["Primary:ScourCondRate_BC11"] = typeof(string),
            ["Primary:BridgeCondClass_BC12"] = typeof(string),
            ["Primary:LowestCondRateCode_BC13"] = typeof(string),
            ["Primary:ApproachRoadwayAlign_BAP01"] = typeof(string),
            ["Primary:OvertopLikelihood_BAP02"] = typeof(string),
            ["Primary:ScourVulnerability_BAP03"] = typeof(string),
            ["Primary:ScourPlanOfAction_BAP04"] = typeof(string),
            ["Primary:SeismicVulnerability_BAP05"] = typeof(string),
            ["Primary:YearBuilt_W01"] = typeof(short),
            ["Primary:NSTMInspectCond_BC14"] = typeof(string),
            ["Primary:UnderwaterInspectCond_BC15"] = typeof(string),

            // Features
            ["Feature:FeatureType_BF01"] = typeof(string),
            ["Feature:FeatureLocation_BF02"] = typeof(string),
            ["Feature:FeatureName_BF03"] = typeof(string),
            ["Feature:FuncClass_BH01"] = typeof(string),
            ["Feature:UrbanCode_BH02"] = typeof(string),
            ["Feature:NHSDesig_BH03"] = typeof(string),
            ["Feature:NatHwyFreightNet_BH04"] = typeof(string),
            ["Feature:STRAHNETDesig_BH05"] = typeof(string),
            ["Feature:LRSRouteID_BH06"] = typeof(string),
            ["Feature:LRSMilePoint_BH07"] = typeof(decimal),
            ["Feature:LanesOnHwy_BH08"] = typeof(byte),
            ["Feature:AADT_BH09"] = typeof(decimal),
            ["Feature:AADTT_BH10"] = typeof(decimal),
            ["Feature:YearAADT_BH11"] = typeof(short),
            ["Feature:HwyMaxVertClearance_BH12"] = typeof(decimal),
            ["Feature:HwyMinVertClearance_BH13"] = typeof(decimal),
            ["Feature:HwyMinHorizClearanceLeft_BH14"] = typeof(decimal),
            ["Feature:HwyMinHorizClearanceRight_BH15"] = typeof(decimal),
            ["Feature:HwyMaxUsableSurfaceWidth_BH16"] = typeof(decimal),
            ["Feature:BypassDetourLength_BH17"] = typeof(decimal),
            ["Feature:CrossingBridgeNo_BH18"] = typeof(string),
            ["Feature:RailroadServiceType_BRR01"] = typeof(string),
            ["Feature:RailroadMinVertClearance_BRR02"] = typeof(decimal),
            ["Feature:RailroadMinHorizOffset_BRR03"] = typeof(decimal),
            ["Feature:NavWaterway_BN01"] = typeof(string),
            ["Feature:NavMinVertClearance_BN02"] = typeof(decimal),
            ["Feature:MovableMaxNavVertClearance_BN03"] = typeof(decimal),
            ["Feature:NavChannelWidth_BN04"] = typeof(decimal),
            ["Feature:NavChannelMinHorizClearance_BN05"] = typeof(decimal),
            ["Feature:SubstructNavProtection_BN06"] = typeof(string),
            ["Feature:RecordStatus"] = typeof(string),

            //Elements
            ["Element:ElementNo_BE01"] = typeof(string),
            ["Element:ElementParentNo_BE02"] = typeof(string),
            ["Element:ElementTotalQuantity_BE03"] = typeof(int),
            ["Element:ElementCS1_BCS01"] = typeof(int?),
            ["Element:ElementCS2_BCS02"] = typeof(int?),
            ["Element:ElementCS2_BCS03"] = typeof(int?),
            ["Element:ElementCS2_BCS04"] = typeof(int?),

            //Inspection
            ["Inspection:InspectionType_BIE01"] = typeof(string),
            ["Inspection:BeginDate_BIE02"] = typeof(DateOnly?),
            ["Inspection:CompletionDate_BIE03"] = typeof(DateOnly?),
            ["Inspection:NC_BridgeInspector_BIE04"] = typeof(string),
            ["Inspection:InspectInterval_BIE05"] = typeof(byte?),
            ["Inspection:InspectDueDate_BIE06"] = typeof(DateOnly?),
            ["Inspection:RBI_Method_BIE07"] = typeof(string),
            ["Inspection:QltyControlDate_BIE08"] = typeof(DateOnly?),
            ["Inspection:QltyAssuranceDate_BIE09"] = typeof(DateOnly?),
            ["Inspection:InspectDataUpdateDate_BIE10"] = typeof(DateOnly?),
            ["Inspection:InspectionNote_BIE11"] = typeof(string),
            ["Inspection:RecordStatus"] = typeof(string),
            ["Inspection:InspectEquipment_BIE12"] = typeof(string),

            //Posting Evaluation
            ["PostingEvaluation:LegalLoadConfig_BEP01"] = typeof(string),
            ["PostingEvaluation:LegalLoadRatingFactor_BEP02"] = typeof(decimal?),
            ["PostingEvaluation:PostingType_BEP03"] = typeof(string),
            ["PostingEvaluation:PostingValue_BEP04"] = typeof(string),
            ["PostingEvaluation:RecordStatus"] = typeof(string),

            //Posting Status
            ["PostingStatus:PostingStatusChangeDate_BPS02"] = typeof(DateOnly?),
            ["PostingStatus:LoadPostingStatus_BPS01"] = typeof(string),
            ["PostingStatus:RecordStatus"] = typeof(string),

            //Span Set
            ["SpanSet:SpanConfigDesig_BSP01"] = typeof(string),
            ["SpanSet:NumberOfSpans_BSP02"] = typeof(decimal?),
            ["SpanSet:NumberOfBeamLines_BSP03"] = typeof(decimal?),
            ["SpanSet:SpanMaterial_BSP04"] = typeof(string),
            ["SpanSet:SpanContinuity_BSP05"] = typeof(string),
            ["SpanSet:SpanType_BSP06"] = typeof(string),
            ["SpanSet:SpanProtectSystem_BSP07"] = typeof(string),
            ["SpanSet:DeckInteraction_BSP08"] = typeof(string),
            ["SpanSet:DeckMaterial_BSP09"] = typeof(string),
            ["SpanSet:WearingSurface_BSP10"] = typeof(string),
            ["SpanSet:DeckProtectSystem_BSP11"] = typeof(string),
            ["SpanSet:DeckReinforcSystem_BSP12"] = typeof(string),
            ["SpanSet:DeckStayInPlaceForms_BSP13"] = typeof(string),

            //Substructure Set
            ["SubstructureSet:SubstructConfigDesig_BSB01"] = typeof(string),
            ["SubstructureSet:NoSubstructUnits_BSB02"] = typeof(decimal?),
            ["SubstructureSet:SubstructMaterial_BSB03"] = typeof(string),
            ["SubstructureSet:SubstructType_BSB04"] = typeof(string),
            ["SubstructureSet:SubstructProtectSystem_BSB05"] = typeof(string),
            ["SubstructureSet:FoundationType_BSB06"] = typeof(string),
            ["SubstructureSet:FoundationProtectSystem_BSB07"] = typeof(string),

            //Route
            ["Route:RouteDesignation_BRT01"] = typeof(string),
            ["Route:RouteNumber_BRT02"] = typeof(string),
            ["Route:RouteDirection_BRT03"] = typeof(string),
            ["Route:RouteType_BRT04"] = typeof(string),
            ["Route:ServiceType_BRT05"] = typeof(string),

            //Work
            ["Work:YearWorkPerformed_BW02"] = typeof(short?),
            ["Work:WorkPerformed_BW03"] = typeof(string)
        };

        public static Type? GetFieldType(string dataSet, string identifier)
        {
            var key = $"{dataSet}:{identifier}";
            return _fieldTypes.TryGetValue(key, out var type) ? type : null;
        }




    }
}
