go
--use [NBTIS-Dev-DB]
--Rename: usp_DeleteBridgeInventoryPartial
CREATE or alter  PROCEDURE [NBI].[usp_DeleteBridgeInventoryPartial]
    @SubmitId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete from BridgeElements
    DELETE T
    FROM [NBI].[BridgeElements] T
    INNER JOIN [NBI].[Stage_BridgeElements] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgeFeatures
    DELETE T
    FROM [NBI].[BridgeFeatures] T
    INNER JOIN [NBI].[Stage_BridgeFeatures] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgeInspections
    DELETE T
    FROM [NBI].[BridgeInspections] T
    INNER JOIN [NBI].[Stage_BridgeInspections] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgePostingEvaluations
    DELETE T
    FROM [NBI].[BridgePostingEvaluations] T
    INNER JOIN [NBI].[Stage_BridgePostingEvaluations] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgePostingStatuses
    DELETE T
    FROM [NBI].[BridgePostingStatuses] T
    INNER JOIN [NBI].[Stage_BridgePostingStatuses] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgePrimary
    DELETE T
    FROM [NBI].[BridgePrimary] T
    INNER JOIN [NBI].[Stage_BridgePrimary] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgeRoutes
    DELETE T
    FROM [NBI].[BridgeRoutes] T
    INNER JOIN [NBI].[Stage_BridgeRoutes] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgeSpanSets
    DELETE T
    FROM [NBI].[BridgeSpanSets] T
    INNER JOIN [NBI].[Stage_BridgeSpanSets] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgeSubstructureSets
    DELETE T
    FROM [NBI].[BridgeSubstructureSets] T
    INNER JOIN [NBI].[Stage_BridgeSubstructureSets] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;

    -- Delete from BridgeWorks
    DELETE T
    FROM [NBI].[BridgeWorks] T
    INNER JOIN [NBI].[Stage_BridgeWorks] S
        ON T.StateCode_BL01 = S.StateCode_BL01
        AND T.BridgeNo_BID01 = S.BridgeNo_BID01
        AND T.SubmittedBy = S.SubmittedBy
    WHERE S.SubmitId = @SubmitId;
END;
GO

----Rename: usp_DeleteBridgeInventoryFull
CREATE or alter  PROCEDURE [NBI].[usp_DeleteBridgeInventoryFull]
    @SubmitId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

	  /*------------------------------------------------------------------*/
    /* 1.  DELETE existing live rows for the SAME SubmittedBy values    */
    /*------------------------------------------------------------------*/
  

    /* BridgeElements */
    DELETE T
    FROM  NBI.BridgeElements       AS T
    JOIN  NBI.Stage_BridgeElements AS S
          ON S.SubmitId    = @SubmitId      -- batch filter
         AND S.SubmittedBy = T.SubmittedBy; -- match key
    
    /* BridgeRoutes */
    DELETE T
    FROM  NBI.BridgeRoutes       AS T
    JOIN  NBI.Stage_BridgeRoutes AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;
	
	/* BridgeFeatures */
    DELETE T
    FROM  NBI.BridgeFeatures       AS T
    JOIN  NBI.Stage_BridgeFeatures AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;

    /* BridgeInspections */
    DELETE T
    FROM  NBI.BridgeInspections       AS T
    JOIN  NBI.Stage_BridgeInspections AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;

    /* BridgePostingEvaluations */
    DELETE T
    FROM  NBI.BridgePostingEvaluations       AS T
    JOIN  NBI.Stage_BridgePostingEvaluations AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;

    /* BridgePostingStatuses */
    DELETE T
    FROM  NBI.BridgePostingStatuses       AS T
    JOIN  NBI.Stage_BridgePostingStatuses AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;

    /* BridgeSpanSets */
    DELETE T
    FROM  NBI.BridgeSpanSets       AS T
    JOIN  NBI.Stage_BridgeSpanSets AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;

    /* BridgeSubstructureSets */
    DELETE T
    FROM  NBI.BridgeSubstructureSets       AS T
    JOIN  NBI.Stage_BridgeSubstructureSets AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;

    /* BridgeWorks */
    DELETE T
    FROM  NBI.BridgeWorks       AS T
    JOIN  NBI.Stage_BridgeWorks AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;

	/* BridgePrimary */
    DELETE T
    FROM  NBI.BridgePrimary       AS T
    JOIN  NBI.Stage_BridgePrimary AS S
          ON S.SubmitId    = @SubmitId
         AND S.SubmittedBy = T.SubmittedBy;
end
go

CREATE or alter  PROCEDURE [NBI].[usp_DeleteStageRecordsBySubmitId]
    @SubmitId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
	/*------------------------------------------------------------------*/
    /* 3.  Purge the Stage_* rows for this SubmitId                     */
    /*------------------------------------------------------------------*/
    DELETE FROM NBI.Stage_BridgeElements         WHERE SubmitId = @SubmitId;
	DELETE FROM NBI.Stage_BridgeRoutes           WHERE SubmitId = @SubmitId;
    DELETE FROM NBI.Stage_BridgeFeatures         WHERE SubmitId = @SubmitId;
    DELETE FROM NBI.Stage_BridgeInspections      WHERE SubmitId = @SubmitId;
    DELETE FROM NBI.Stage_BridgePostingEvaluations WHERE SubmitId = @SubmitId;
    DELETE FROM NBI.Stage_BridgePostingStatuses  WHERE SubmitId = @SubmitId;  
    DELETE FROM NBI.Stage_BridgeSpanSets         WHERE SubmitId = @SubmitId;
    DELETE FROM NBI.Stage_BridgeSubstructureSets WHERE SubmitId = @SubmitId;
    DELETE FROM NBI.Stage_BridgeWorks            WHERE SubmitId = @SubmitId;
	DELETE FROM NBI.Stage_BridgePrimary          WHERE SubmitId = @SubmitId;
end
go

CREATE or alter PROCEDURE [NBI].[usp_BulkMoveStageToBridgeInventory]
    @SubmitId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
	 /*------------------------------------------------------------------*/
    /* 2.  INSERT the new batch into live tables (one block shown)      */
    /*------------------------------------------------------------------*/
    -- BridgeElements
    INSERT INTO [NBI].[BridgeElements] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy, ElementNo_BE01, 
        ElementParentNo_BE02, ElementTotalQuantity_BE03, 
        ElementCS1_BCS01, ElementCS2_BCS02, ElementCS3_BCS03, ElementCS4_BCS04
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy, ElementNo_BE01, 
        ElementParentNo_BE02, ElementTotalQuantity_BE03, 
        ElementCS1_BCS01, ElementCS2_BCS02, ElementCS3_BCS03, ElementCS4_BCS04
    FROM [NBI].[Stage_BridgeElements]
    WHERE SubmitId = @SubmitId  and RecordStatus  = 'Active';

  
  -- BridgePrimary
    INSERT INTO [NBI].[BridgePrimary] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        BridgeName_BID02, PrevBridgeNo_BID03, CountyCode_BL02, PlaceCode_BL03,
        HighwayDist_BL04, Latitude_BL05, Longitude_BL06, BorderBridgeNo_BL07,
        BorderBridgeStateCode_BL08, BorderBridgeInspectResp_BL09, BorderBridgeLeadState_BL10,
        BridgeLocation_BL11, MPO_BL12, Owner_BCL01, MaintResp_BCL02, FedTribalAccess_BCL03,
        HistSignificance_BCL04, Toll_BCL05, EmergencyEvacDesig_BCL06,
        BridgeRailings_BRH01, Transitions_BRH02, NBISBridgeLength_BG01, TotalBridgeLength_BG02,
        MaxSpanLength_BG03, MinSpanLength_BG04, BridgeWidthOut_BG05, BridgeWidthCurb_BG06,
        LeftCurbWidth_BG07, RightCurbWidth_BG08, ApproachRoadwayWidth_BG09, BridgeMedian_BG10,
        Skew_BG11, CurvedBridge_BG12, MaxBridgeHeight_BG13, SidehillBridge_BG14, IrregularDeckArea_BG15,
        CalcDeckArea_BG16, DesignLoad_BLR01, DesignMethod_BLR02, LoadRatingDate_BLR03,
        LoadRatingMethod_BLR04, InventoryLRFactor_BLR05, OperatingLRFactor_BLR06,
        ControllingLegalLRFactor_BLR07, RoutinePermitLoads_BLR08, NSTMInspectReq_BIR01,
        FatigueDetails_BIR02, UnderwaterInspectReq_BIR03, ComplexFeature_BIR04,
        DeckCondRate_BC01, SuperstrCondRate_BC02, SubstructCondRate_BC03,
        CulvertCondRate_BC04, BridgeRailCondRate_BC05, BridgeRailTransitCondRate_BC06,
        BridgeBearingCondRate_BC07, BridgeJointCondRate_BC08, ChannelCondRate_BC09,
        ChannelProtectCondRate_BC10, ScourCondRate_BC11, BridgeCondClass_BC12,
        LowestCondRateCode_BC13, ApproachRoadwayAlign_BAP01, OvertopLikelihood_BAP02,
        ScourVulnerability_BAP03, ScourPOA_BAP04, SeismicVulnerability_BAP05,
        YearBuilt_BW01, NSTMInspectCond_BC14, UnderwaterInspectCond_BC15
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        BridgeName_BID02, PrevBridgeNo_BID03, CountyCode_BL02, PlaceCode_BL03,
        HighwayDist_BL04, Latitude_BL05, Longitude_BL06, BorderBridgeNo_BL07,
        BorderBridgeStateCode_BL08, BorderBridgeInspectResp_BL09, BorderBridgeLeadState_BL10,
        BridgeLocation_BL11, MPO_BL12, Owner_BCL01, MaintResp_BCL02, FedTribalAccess_BCL03,
        HistSignificance_BCL04, Toll_BCL05, EmergencyEvacDesig_BCL06,
        BridgeRailings_BRH01, Transitions_BRH02, NBISBridgeLength_BG01, TotalBridgeLength_BG02,
        MaxSpanLength_BG03, MinSpanLength_BG04, BridgeWidthOut_BG05, BridgeWidthCurb_BG06,
        LeftCurbWidth_BG07, RightCurbWidth_BG08, ApproachRoadwayWidth_BG09, BridgeMedian_BG10,
        Skew_BG11, CurvedBridge_BG12, MaxBridgeHeight_BG13, SidehillBridge_BG14, IrregularDeckArea_BG15,
        CalcDeckArea_BG16, DesignLoad_BLR01, DesignMethod_BLR02, LoadRatingDate_BLR03,
        LoadRatingMethod_BLR04, InventoryLRFactor_BLR05, OperatingLRFactor_BLR06,
        ControllingLegalLRFactor_BLR07, RoutinePermitLoads_BLR08, NSTMInspectReq_BIR01,
        FatigueDetails_BIR02, UnderwaterInspectReq_BIR03, ComplexFeature_BIR04,
        DeckCondRate_BC01, SuperstrCondRate_BC02, SubstructCondRate_BC03,
        CulvertCondRate_BC04, BridgeRailCondRate_BC05, BridgeRailTransitCondRate_BC06,
        BridgeBearingCondRate_BC07, BridgeJointCondRate_BC08, ChannelCondRate_BC09,
        ChannelProtectCondRate_BC10, ScourCondRate_BC11, BridgeCondClass_BC12,
        LowestCondRateCode_BC13, ApproachRoadwayAlign_BAP01, OvertopLikelihood_BAP02,
        ScourVulnerability_BAP03, ScourPOA_BAP04, SeismicVulnerability_BAP05,
        YearBuilt_BW01, NSTMInspectCond_BC14, UnderwaterInspectCond_BC15
    FROM [NBI].[Stage_BridgePrimary]
    WHERE SubmitId = @SubmitId ; -- and RecordStatus  = 'Active' Note this will not have record status active


    -- BridgeInspections
    INSERT INTO [NBI].[BridgeInspections] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        InspectionType_BIE01, BeginDate_BIE02, CompletionDate_BIE03,
        NC_BridgeInspector_BIE04, InspectInterval_BIE05, InspectDueDate_BIE06,
        RBI_Method_BIE07, QltyControlDate_BIE08, QltyAssuranceDate_BIE09,
        InspectDataUpdateDate_BIE10, InspectionNote_BIE11, InspectEquipment_BIE12
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        InspectionType_BIE01, BeginDate_BIE02, CompletionDate_BIE03,
        NC_BridgeInspector_BIE04, InspectInterval_BIE05, InspectDueDate_BIE06,
        RBI_Method_BIE07, QltyControlDate_BIE08, QltyAssuranceDate_BIE09,
        InspectDataUpdateDate_BIE10, InspectionNote_BIE11, InspectEquipment_BIE12
    FROM [NBI].[Stage_BridgeInspections]
    WHERE SubmitId = @SubmitId  and RecordStatus  = 'Active';

    -- BridgePostingEvaluations
    INSERT INTO [NBI].[BridgePostingEvaluations] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        LegalLoadConfig_BEP01, LegalLoadRatingFactor_BEP02,
        PostingType_BEP03, PostingValue_BEP04
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        LegalLoadConfig_BEP01, LegalLoadRatingFactor_BEP02,
        PostingType_BEP03, PostingValue_BEP04
    FROM [NBI].[Stage_BridgePostingEvaluations]
    WHERE SubmitId = @SubmitId  and RecordStatus  = 'Active';

    -- BridgePostingStatuses
    INSERT INTO [NBI].[BridgePostingStatuses] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        PostingStatusChangeDate_BPS02, LoadPostingStatus_BPS01
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        PostingStatusChangeDate_BPS02, LoadPostingStatus_BPS01
    FROM [NBI].[Stage_BridgePostingStatuses]
    WHERE SubmitId = @SubmitId and PostingStatusChangeDate_BPS02 is not null and RecordStatus  = 'Active';

    


	  -- BridgeFeatures
    INSERT INTO [NBI].[BridgeFeatures] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        FeatureType_BF01, FeatureLocation_BF02, FeatureName_BF03,
        FuncClass_BH01, UrbanCode_BH02, NHSDesig_BH03, NatHwyFreightNet_BH04,
        STRAHNETDesig_BH05, LRSRouteID_BH06, LRSMilePoint_BH07,
        LanesOnHwy_BH08, AADT_BH09, AADTT_BH10, YearAADT_BH11,
        HwyMaxVertClearance_BH12, HwyMinVertClearance_BH13,
        HwyMinHorizClearanceLeft_BH14, HwyMinHorizClearanceRight_BH15,
        HwyMaxUsableSurfaceWidth_BH16, BypassDetourLength_BH17,
        CrossingBridgeNo_BH18, RailroadServiceType_BRR01,
        RailroadMinVertClearance_BRR02, RailroadMinHorizOffset_BRR03,
        NavWaterway_BN01, NavMinVertClearance_BN02, MovableMaxNavVertClearance_BN03,
        NavChannelWidth_BN04, NavChannelMinHorizClearance_BN05,
        SubstructNavProtection_BN06 --,SysStartTime,SysEndTime
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        FeatureType_BF01, FeatureLocation_BF02, FeatureName_BF03,
        FuncClass_BH01, UrbanCode_BH02, NHSDesig_BH03, NatHwyFreightNet_BH04,
        STRAHNETDesig_BH05, LRSRouteID_BH06, LRSMilePoint_BH07,
        LanesOnHwy_BH08, AADT_BH09, AADTT_BH10, YearAADT_BH11,
        HwyMaxVertClearance_BH12, HwyMinVertClearance_BH13,
        HwyMinHorizClearanceLeft_BH14, HwyMinHorizClearanceRight_BH15,
        HwyMaxUsableSurfaceWidth_BH16, BypassDetourLength_BH17,
        CrossingBridgeNo_BH18, RailroadServiceType_BRR01,
        RailroadMinVertClearance_BRR02, RailroadMinHorizOffset_BRR03,
        NavWaterway_BN01, NavMinVertClearance_BN02, MovableMaxNavVertClearance_BN03,
        NavChannelWidth_BN04, NavChannelMinHorizClearance_BN05,
        SubstructNavProtection_BN06 --,getdate(),getdate()
    FROM [NBI].[Stage_BridgeFeatures]
    WHERE SubmitId = @SubmitId  and RecordStatus  = 'Active';
	
--	and FeatureType_BF01 is not null
--	and StateCode_BL01 is not null
--and BridgeNo_BID01 is not null
--and SubmittedBy is not null
--and FeatureType_BF01 is not null
--;



    -- BridgeRoutes
    INSERT INTO [NBI].[BridgeRoutes] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,FeatureType_BF01,
         RouteDesignation_BRT01, RouteNumber_BRT02,
        RouteDirection_BRT03, RouteType_BRT04, ServiceType_BRT05
    )
    SELECT 
        [NBI].[Stage_BridgeRoutes].StateCode_BL01, [NBI].[Stage_BridgeRoutes].BridgeNo_BID01, [NBI].[Stage_BridgeRoutes].SubmittedBy,[Stage_bridgefeatures].FeatureType_BF01,
         [NBI].[Stage_BridgeRoutes].RouteDesignation_BRT01, [NBI].[Stage_BridgeRoutes].RouteNumber_BRT02,
        [NBI].[Stage_BridgeRoutes].RouteDirection_BRT03, [NBI].[Stage_BridgeRoutes].RouteType_BRT04, [NBI].[Stage_BridgeRoutes].ServiceType_BRT05
    FROM [NBI].[Stage_BridgeRoutes]
	inner join nbi.[Stage_bridgefeatures] on nbi.[Stage_BridgeRoutes].FeatureID = [Stage_bridgefeatures].ID
    WHERE [NBI].[Stage_BridgeRoutes].SubmitId = @SubmitId  and [Stage_BridgeRoutes].RecordStatus  = 'Active';

    -- BridgeSpanSets
    INSERT INTO [NBI].[BridgeSpanSets] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        SpanConfigDesig_BSP01, NumberOfSpans_BSP02, NumberOfBeamLines_BSP03,
        SpanMaterial_BSP04, SpanContinuity_BSP05, SpanType_BSP06,
        SpanProtectSystem_BSP07, DeckInteraction_BSP08, DeckMaterial_BSP09,
        WearingSurface_BSP10, DeckProtectSystem_BSP11, DeckReinforcSystem_BSP12,
        DeckStayInPlaceForms_BSP13
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        SpanConfigDesig_BSP01, NumberOfSpans_BSP02, NumberOfBeamLines_BSP03,
        SpanMaterial_BSP04, SpanContinuity_BSP05, SpanType_BSP06,
        SpanProtectSystem_BSP07, DeckInteraction_BSP08, DeckMaterial_BSP09,
        WearingSurface_BSP10, DeckProtectSystem_BSP11, DeckReinforcSystem_BSP12,
        DeckStayInPlaceForms_BSP13
    FROM [NBI].[Stage_BridgeSpanSets]
    WHERE SubmitId = @SubmitId  and RecordStatus  = 'Active';

    -- BridgeSubstructureSets
    INSERT INTO [NBI].[BridgeSubstructureSets] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        SubstructConfigDesig_BSB01, NoSubstructUnits_BSB02, SubstructMaterial_BSB03,
        SubstructType_BSB04, SubstructProtectSystem_BSB05,
        FoundationType_BSB06, FoundationProtectSystem_BSB07
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        SubstructConfigDesig_BSB01, NoSubstructUnits_BSB02, SubstructMaterial_BSB03,
        SubstructType_BSB04, SubstructProtectSystem_BSB05,
        FoundationType_BSB06, FoundationProtectSystem_BSB07
    FROM [NBI].[Stage_BridgeSubstructureSets]
    WHERE SubmitId = @SubmitId  and RecordStatus  = 'Active';

    -- BridgeWorks
    INSERT INTO [NBI].[BridgeWorks] WITH (TABLOCK)
    (
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        YearWorkPerformed_BW02, WorkPerformed_BW03
    )
    SELECT 
        StateCode_BL01, BridgeNo_BID01, SubmittedBy,
        YearWorkPerformed_BW02, WorkPerformed_BW03
    FROM [NBI].[Stage_BridgeWorks]
    WHERE SubmitId = @SubmitId  and RecordStatus  = 'Active';

END;
GO

GO
/****** Object:  StoredProcedure [NBI].[usp_AcceptFullBridgeBatch]    Script Date: 4/29/2025 12:43:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--drop proc [usp_AcceptFullBridgeBatch]
--rename usp_AcceptBridgeBatch
create or alter   PROCEDURE [NBI].[usp_AcceptBridgeBatch] --( or just AcceptBrid)
    @SubmitId BIGINT,
	@IsPartial bit
AS
BEGIN
/* ======================================================================
   Name      : NBI.usp_ImportBridgeBatch
   Purpose   : (1) Remove existing “live” rows for every SubmittedBy value
                     found in the current staging batch
               (2) Move the batch from Stage_* tables into the live tables
               (3) Purge the Stage_* tables
   Params    : @SubmitId – BIGINT key generated when the batch was staged
   Notes     : • XACT_ABORT ON  → any run-time error rolls back the txn
               • WITH (TABLOCK) → enables minimal logging for big INSERTS
               • Tune further with TRUNCATE TABLE if staging is transient
   ====================================================================== */
    --this is just to make sure if it is not in HQ review we return and will not do any execution.
	if not exists(select top 1 statuscode  from NBI.SubmittalLogs where StatusCode = 5 and SubmitId=@SubmitId)
	begin
	print 'there is no record available with status code 5'
	return;
	end

    SET NOCOUNT ON;
    SET XACT_ABORT ON;
	
	BEGIN TRAN;

	
	
	IF @IsPartial <> 0  -- For partial
		BEGIN
		exec  [NBI].usp_DeleteBridgeInventoryPartial @SubmitId=@SubmitId
			
		END
	else 
	  BEGIN
	  --For full
			exec  [NBI].usp_DeleteBridgeInventoryFull @SubmitId=@SubmitId
	  END

	  print 'usp_BulkMoveStageToBridgeInventory'
    exec [NBI].usp_BulkMoveStageToBridgeInventory  @SubmitId=@SubmitId     
	print 'usp_DeleteStageRecordsBySubmitId'
	exec  [NBI].usp_DeleteStageRecordsBySubmitId @SubmitId=@SubmitId

    COMMIT TRAN;
END
