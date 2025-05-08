using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBTIS.Core.DTOs;
using NBTIS.Data.Models;
using static NBTIS.Core.DTOs.SNBIRecord;

namespace NBTIS.Core.Services
{


    public class BridgeStagingLoaderService
    {
        private readonly DataContext _context;
        private readonly ILogger<BridgeStagingLoaderService> _logger;

        public BridgeStagingLoaderService(DataContext context, ILogger<BridgeStagingLoaderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<StagingResultDTO> PopulateStageAsync(long submitId, string submittedBy, List<SNBIRecord> stagingData, CancellationToken cancellationToken = default)
        {
            if (stagingData == null)
                throw new ArgumentNullException(nameof(stagingData));

            if (!stagingData.Any())
                throw new ArgumentException("No SNBI records provided in staging data.", nameof(stagingData));

            var stopwatch = Stopwatch.StartNew();
            var result = new StagingResultDTO();

            _logger.LogInformation("Starting staging process for submitId: {SubmitId} by State/Agency - {SubmittedBy}.", submitId, submittedBy);

            // Create the execution strategy to support retries for transient faults.
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {

                using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Master collections
                var allStageBridges = new List<Stage_BridgePrimary>();
                var allElements = new List<Stage_BridgeElement>();
                var allFeatures = new List<Stage_BridgeFeature>();
                var allRoutes = new List<Stage_BridgeRoute>();
                var allInspections = new List<Stage_BridgeInspection>();
                var allPostingEvaluations = new List<Stage_BridgePostingEvaluation>();
                var allPostingStatuses = new List<Stage_BridgePostingStatus>();
                var allSpanSets = new List<Stage_BridgeSpanSet>();
                var allSubstructureSets = new List<Stage_BridgeSubstructureSet>();
                var allWorks = new List<Stage_BridgeWork>();

                // List to store pending route mappings until features are inserted
                var pendingRoutes = new List<(Stage_BridgeFeature bridgeFeature, List<SNBIRecord.Route> routes)>();

                foreach (var snbiRecord in stagingData)
                {
                    // Map and add StageBridge
                    var stageBridge = MapStageBridge(snbiRecord, submitId, submittedBy);
                    allStageBridges.Add(stageBridge);

                    // -------- Map Child Collections --------
                    if (snbiRecord.Elements?.Any() == true)
                        allElements.AddRange(snbiRecord.Elements.Select(e => MapElement(e, submitId, submittedBy)));

                    if (snbiRecord.Features?.Any() == true)
                    {
                        foreach (var feature in snbiRecord.Features)
                        {
                            var bridgeFeature = MapFeature(feature, submitId, submittedBy);
                            allFeatures.Add(bridgeFeature);

                            // Store the routes to be processed later once the feature's identity is set.
                            if (feature.Routes?.Any() == true)
                            {
                                pendingRoutes.Add((bridgeFeature, feature.Routes.ToList()));
                            }
                        }
                    }

                    if (snbiRecord.Inspections?.Any() == true)
                        allInspections.AddRange(snbiRecord.Inspections.Select(i => MapInspection(i, submitId, submittedBy)));

                    if (snbiRecord.PostingEvaluations?.Any() == true)
                        allPostingEvaluations.AddRange(snbiRecord.PostingEvaluations.Select(pe => MapPostingEvaluation(pe, submitId, submittedBy)));

                    if (snbiRecord.PostingStatuses?.Any() == true)
                        allPostingStatuses.AddRange(snbiRecord.PostingStatuses.Select(ps => MapPostingStatus(ps, submitId, submittedBy)));

                    if (snbiRecord.SpanSets?.Any() == true)
                        allSpanSets.AddRange(snbiRecord.SpanSets.Select(s => MapSpanSet(s, submitId, submittedBy)));

                    if (snbiRecord.SubstructureSets?.Any() == true)
                        allSubstructureSets.AddRange(snbiRecord.SubstructureSets.Select(s => MapSubstructureSet(s, submitId, submittedBy)));

                    if (snbiRecord.Works?.Any() == true)
                        allWorks.AddRange(snbiRecord.Works.Select(w => MapWork(w, submitId, submittedBy)));
                }

                // -------- Bulk Insert & Count --------

                result.BridgesInserted = allStageBridges.Count;
                if (allStageBridges.Any())
                    await _context.BulkInsertAsync(allStageBridges);

                result.ElementsInserted = allElements.Count;
                if (allElements.Any())
                    await _context.BulkInsertAsync(allElements);

                result.FeaturesInserted = allFeatures.Count;
                if (allFeatures.Any())
                    // Using BulkConfig with SetOutputIdentity = true to update in-memory feature IDs**
                    await _context.BulkInsertAsync(allFeatures, new BulkConfig { SetOutputIdentity = true });

                // Now that features have been inserted and their identity columns populated,
                // process the pending routes.
                if (pendingRoutes.Any())
                {
                    foreach (var (bridgeFeature, routes) in pendingRoutes)
                    {
                        // Map routes using the now-updated bridgeFeature.Id
                        allRoutes.AddRange(routes.Select(r => MapRoute(r, bridgeFeature, submitId, submittedBy)));
                    }
                }

                result.RoutesInserted = allRoutes.Count;
                if (allRoutes.Any())
                    await _context.BulkInsertAsync(allRoutes);

                result.InspectionsInserted = allInspections.Count;
                if (allInspections.Any())
                    await _context.BulkInsertAsync(allInspections);

                result.PostingEvaluationsInserted = allPostingEvaluations.Count;
                if (allPostingEvaluations.Any())
                    await _context.BulkInsertAsync(allPostingEvaluations);

                result.PostingStatusesInserted = allPostingStatuses.Count;
                if (allPostingStatuses.Any())
                    await _context.BulkInsertAsync(allPostingStatuses);


                try
                {
                    result.SpanSetsInserted = allSpanSets.Count;
                    if (allSpanSets.Any())
                        await _context.BulkInsertAsync(allSpanSets);
                }
                catch(Microsoft.Data.SqlClient.SqlException ex)
                {
                    _logger.LogError(ex, "Error occurred during BulkInsert(allSpanSets): {SubmitId}.", submitId);
                }

                result.SubstructureSetsInserted = allSubstructureSets.Count;
                if (allSubstructureSets.Any())
                    await _context.BulkInsertAsync(allSubstructureSets);

                result.WorksInserted = allWorks.Count;
                if (allWorks.Any())
                    await _context.BulkInsertAsync(allWorks);

                await transaction.CommitAsync();
                stopwatch.Stop();
                result.ProcessingTime = stopwatch.Elapsed;

                _logger.LogInformation("Staging completed in {Duration} seconds. Bridges: {Bridges}, Elements: {Elements}, Features: {Features}, Routes: {Routes}, Inspections: {Inspections}, Posting Evaluations: {PostEvals}, Posting Statuses: {PostStatuses}, SpanSets: {SpanSets}, SubstructureSets: {SubSets}, Works: {Works}.",
                    result.ProcessingTime.TotalSeconds,
                    result.BridgesInserted, result.ElementsInserted, result.FeaturesInserted,
                    result.RoutesInserted, result.InspectionsInserted, result.PostingEvaluationsInserted,
                    result.PostingStatusesInserted, result.SpanSetsInserted, result.SubstructureSetsInserted, result.WorksInserted);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during staging process for submitId: {SubmitId}.", submitId);
                await transaction.RollbackAsync();
                throw;
            }

            });
        }

        #region Mapping Methods

        private Stage_BridgePrimary MapStageBridge(SNBIRecord snbiRecord, long submitId, string submittedBy)
        {
            return new Stage_BridgePrimary
            {
                SubmitId = submitId,
                StateCode_BL01 = (byte)(snbiRecord.BL01 ?? 0),
                BridgeNo_BID01 = snbiRecord.BID01 ?? string.Empty,
                SubmittedBy = submittedBy,
                BridgeName_BID02 = snbiRecord.BID02,
                PrevBridgeNo_BID03 = snbiRecord.BID03,
                CountyCode_BL02 = snbiRecord.BL02 ?? 0,
                PlaceCode_BL03 = snbiRecord.BL03,
                HighwayDist_BL04 = snbiRecord.BL04,
                Latitude_BL05 = (decimal?)snbiRecord.BL05,
                Longitude_BL06 = (decimal?)snbiRecord.BL06,
                BorderBridgeNo_BL07 = snbiRecord.BL07,
                BorderBridgeStateCode_BL08 = snbiRecord.BL08,
                BorderBridgeInspectResp_BL09 = snbiRecord.BL09,
                BorderBridgeLeadState_BL10 = snbiRecord.BL10,
                BridgeLocation_BL11 = snbiRecord.BL11,
                MPO_BL12 = snbiRecord.BL12,
                Owner_BCL01 = snbiRecord.BCL01 ?? string.Empty,
                MaintResp_BCL02 = snbiRecord.BCL02 ?? string.Empty,
                FedTribalAccess_BCL03 = snbiRecord.BCL03,
                HistSignificance_BCL04 = snbiRecord.BCL04,
                Toll_BCL05 = snbiRecord.BCL05,
                EmergencyEvacDesig_BCL06 = snbiRecord.BCL06,
                BridgeRailings_BRH01 = snbiRecord.BRH01,
                Transitions_BRH02 = snbiRecord.BRH02,
                NBISBridgeLength_BG01 = (decimal?)snbiRecord.BG01,
                TotalBridgeLength_BG02 = (decimal?)snbiRecord.BG02,
                MaxSpanLength_BG03 = (decimal?)snbiRecord.BG03,
                MinSpanLength_BG04 = (decimal?)snbiRecord.BG04,
                BridgeWidthOut_BG05 = (decimal?)snbiRecord.BG05,
                BridgeWidthCurb_BG06 = (decimal?)snbiRecord.BG06,
                LeftCurbWidth_BG07 = (decimal?)snbiRecord.BG07,
                RightCurbWidth_BG08 = (decimal?)snbiRecord.BG08,
                ApproachRoadwayWidth_BG09 = (decimal?)snbiRecord.BG09,
                BridgeMedian_BG10 = snbiRecord.BG10,
                Skew_BG11 = snbiRecord.BG11.HasValue ? (byte?)Convert.ToByte(snbiRecord.BG11.Value) : null,
                CurvedBridge_BG12 = snbiRecord.BG12,
                MaxBridgeHeight_BG13 = snbiRecord.BG13.HasValue ? (int?)Convert.ToInt16(snbiRecord.BG13.Value) : null,
                SidehillBridge_BG14 = snbiRecord.BG14,
                IrregularDeckArea_BG15 = (decimal?)snbiRecord.BG15,
                CalcDeckArea_BG16 = (decimal?)snbiRecord.BG16,
                DesignLoad_BLR01 = snbiRecord.BLR01,
                DesignMethod_BLR02 = snbiRecord.BLR02,
                LoadRatingDate_BLR03 = DateOnly.TryParse(snbiRecord.BLR03, out var d) ? d : (DateOnly?)null,
                LoadRatingMethod_BLR04 = snbiRecord.BLR04,
                InventoryLRFactor_BLR05 = (decimal?)snbiRecord.BLR05,
                OperatingLRFactor_BLR06 = (decimal?)snbiRecord.BLR06,
                ControllingLegalLRFactor_BLR07 = (decimal?)snbiRecord.BLR07,
                RoutinePermitLoads_BLR08 = snbiRecord.BLR08,
                NSTMInspectReq_BIR01 = snbiRecord.BIR01,
                FatigueDetails_BIR02 = snbiRecord.BIR02,
                UnderwaterInspectReq_BIR03 = snbiRecord.BIR03,
                ComplexFeature_BIR04 = snbiRecord.BIR04,
                DeckCondRate_BC01 = snbiRecord.BC01,
                SuperstrCondRate_BC02 = snbiRecord.BC02,
                SubstructCondRate_BC03 = snbiRecord.BC03,
                CulvertCondRate_BC04 = snbiRecord.BC04,
                BridgeRailCondRate_BC05 = snbiRecord.BC05,
                BridgeRailTransitCondRate_BC06 = snbiRecord.BC06,
                BridgeBearingCondRate_BC07 = snbiRecord.BC07,
                BridgeJointCondRate_BC08 = snbiRecord.BC08,
                ChannelCondRate_BC09 = snbiRecord.BC09,
                ChannelProtectCondRate_BC10 = snbiRecord.BC10,
                ScourCondRate_BC11 = snbiRecord.BC11,
                BridgeCondClass_BC12 = snbiRecord.BC12,
                LowestCondRateCode_BC13 = snbiRecord.BC13,
                ApproachRoadwayAlign_BAP01 = snbiRecord.BAP01,
                OvertopLikelihood_BAP02 = snbiRecord.BAP02,
                ScourVulnerability_BAP03 = snbiRecord.BAP03,
                ScourPOA_BAP04 = snbiRecord.BAP04,
                SeismicVulnerability_BAP05 = snbiRecord.BAP05,
                YearBuilt_BW01 = snbiRecord.BW01.HasValue ? (short?)snbiRecord.BW01.Value : null,
                NSTMInspectCond_BC14 = snbiRecord.BC14,
                UnderwaterInspectCond_BC15 = snbiRecord.BC15
            };
        }



        // -------- Elements --------
        private Stage_BridgeElement MapElement(SNBIRecord.Element element, long submitId, string submittedBy) => new()
        {

            SubmitId = submitId,
            StateCode_BL01 = (byte)(element.BL01 ?? 0),
            BridgeNo_BID01 = element.BID01 ?? string.Empty,
            SubmittedBy = submittedBy,
            ElementNo_BE01 = element.BE01 ?? string.Empty,
            ElementParentNo_BE02 = element.BE02 ?? string.Empty,
            ElementTotalQuantity_BE03 = element.BE03.HasValue ? (int)element.BE03 : 0,
            ElementCS1_BCS01 = (int?)element.BCS01,
            ElementCS2_BCS02 = (int?)element.BCS02,
            ElementCS3_BCS03 = (int?)element.BCS03,
            ElementCS4_BCS04 = (int?)element.BCS04,
            RecordStatus = element.RecordStatus ?? "Active",
        };

        // -------- Features --------
        private Stage_BridgeFeature MapFeature(
             SNBIRecord.Feature feature,
             long submitId,
             string submittedBy) => new Stage_BridgeFeature
     {
         SubmitId = submitId,
         StateCode_BL01 = (byte)(feature.BL01 ?? 0),
         BridgeNo_BID01 = feature.BID01 ?? string.Empty,
         SubmittedBy = submittedBy,
         FeatureType_BF01 = feature.BF01 ?? string.Empty,
         FeatureLocation_BF02 = feature.BF02 ?? null,
         FeatureName_BF03 = feature.BF03 ?? null,
         FuncClass_BH01 = feature.BH01 ?? null,
         UrbanCode_BH02 = feature.BH02 ?? null,
         NHSDesig_BH03 = feature.BH03 ?? null,
         NatHwyFreightNet_BH04 = feature.BH04 ?? string.Empty,
         STRAHNETDesig_BH05 = feature.BH05 ?? null,
         LRSRouteID_BH06 = feature.BH06 ?? null,
         LRSMilePoint_BH07 = (decimal?)feature.BH07,
         LanesOnHwy_BH08 = feature.BH08.HasValue
                                       ? (byte?)feature.BH08.Value
                                       : null,
         AADT_BH09 = (decimal?)feature.BH09,
         AADTT_BH10 = (decimal?)feature.BH10,
         YearAADT_BH11 = feature.BH11.HasValue
                                       ? (short?)feature.BH11.Value
                                       : null,
         HwyMaxVertClearance_BH12 = (decimal?)feature.BH12,
         HwyMinVertClearance_BH13 = (decimal?)feature.BH13,
         HwyMinHorizClearanceLeft_BH14 = (decimal?)feature.BH14,
         HwyMinHorizClearanceRight_BH15 = (decimal?)feature.BH15,
         HwyMaxUsableSurfaceWidth_BH16 = (decimal?)feature.BH16,
         BypassDetourLength_BH17 = feature.BH17,
         CrossingBridgeNo_BH18 = feature.BH18 ?? null,
         RailroadServiceType_BRR01 = feature.BRR01 ?? null,
         RailroadMinVertClearance_BRR02 = (decimal?)feature.BRR02,
         RailroadMinHorizOffset_BRR03 = (decimal?)feature.BRR03,
         NavWaterway_BN01 = feature.BN01 ?? null,
         NavMinVertClearance_BN02 = (decimal?)feature.BN02,
         MovableMaxNavVertClearance_BN03 = (decimal?)feature.BN03,
         NavChannelWidth_BN04 = (decimal?)feature.BN04,
         NavChannelMinHorizClearance_BN05 = (decimal?)feature.BN05,
         SubstructNavProtection_BN06 = feature.BN06 ?? null,
         RecordStatus = feature.RecordStatus ?? "Active"
     };


        // -------- Routes --------
        public Stage_BridgeRoute MapRoute(
            SNBIRecord.Route route,
            Stage_BridgeFeature stageBridgeFeature,
            long submitId,
            string submittedBy)
        {
            return new Stage_BridgeRoute
            {
                FeatureID = stageBridgeFeature.ID,
                SubmitId = submitId,
                StateCode_BL01 = stageBridgeFeature.StateCode_BL01,
                BridgeNo_BID01 = stageBridgeFeature.BridgeNo_BID01,
                SubmittedBy = submittedBy,
                RouteDesignation_BRT01 = route.BRT01,
                RouteNumber_BRT02 = route.BRT02,
                RouteDirection_BRT03 = route.BRT03,
                RouteType_BRT04 = route.BRT04,
                ServiceType_BRT05 = route.BRT05,
                RecordStatus = route.RecordStatus ?? "Active"
            };
        }


        // -------- Inspections --------
        private Stage_BridgeInspection MapInspection(SNBIRecord.Inspection inspection, long submitId, string submittedBy) => new()
        {
            SubmitId = submitId,
            StateCode_BL01 = (byte)(inspection.BL01 ?? 0),
            BridgeNo_BID01 = inspection.BID01 ?? string.Empty,
            SubmittedBy = submittedBy,
            InspectionType_BIE01 = inspection.BIE01 ?? string.Empty,
            BeginDate_BIE02 = ParseNullableDateOnly(inspection.BIE02),
            CompletionDate_BIE03 = ParseNullableDateOnly(inspection.BIE03),
            NC_BridgeInspector_BIE04 = inspection.BIE04,
            InspectInterval_BIE05 = inspection.BIE05.HasValue
                                      ? (byte?)inspection.BIE05.Value
                                      : null,
            InspectDueDate_BIE06 = ParseNullableDateOnly(inspection.BIE06),
            RBI_Method_BIE07 = inspection.BIE07,
            QltyControlDate_BIE08 = ParseNullableDateOnly(inspection.BIE08),
            QltyAssuranceDate_BIE09 = ParseNullableDateOnly(inspection.BIE09),
            InspectDataUpdateDate_BIE10 = ParseNullableDateOnly(inspection.BIE10),
            InspectionNote_BIE11 = inspection.BIE11,
            InspectEquipment_BIE12 = inspection.BIE12,


            RecordStatus = inspection.RecordStatus ?? "Active",
        };

        // -------- Posting Evaluations --------
        private Stage_BridgePostingEvaluation MapPostingEvaluation(SNBIRecord.PostingEvaluation pe, long submitId, string submittedBy) => new()
        {
            SubmitId = submitId,
            StateCode_BL01 = (byte)(pe.BL01 ?? 0),
            BridgeNo_BID01 = pe.BID01 ?? string.Empty,
            SubmittedBy = submittedBy,
            LegalLoadConfig_BEP01 = pe.BEP01 ?? string.Empty,
            LegalLoadRatingFactor_BEP02 = pe.BEP02.HasValue ? (decimal?)pe.BEP02.Value : null,
            PostingType_BEP03 = pe.BEP03,
            PostingValue_BEP04 = pe.BEP04,
            RecordStatus = pe.RecordStatus ?? "Active",
        };

        // -------- Posting Status --------
        private Stage_BridgePostingStatus MapPostingStatus(SNBIRecord.PostingStatus ps, long submitId, string submittedBy) => new()
        {
            SubmitId = submitId,
            StateCode_BL01 = (byte)(ps.BL01 ?? 0),
            BridgeNo_BID01 = ps.BID01 ?? string.Empty,
            SubmittedBy = submittedBy,
            PostingStatusChangeDate_BPS02 = ParseNullableDateOnly(ps.BPS02),
            LoadPostingStatus_BPS01 = ps.BPS01 ?? null,
            RecordStatus = ps.RecordStatus ?? "Active",
        };

        // ------------------ Span Sets ------------------
        private Stage_BridgeSpanSet MapSpanSet(SNBIRecord.SpanSet span, long submitId, string submittedBy) => new()
        {
            SubmitId = submitId,
            StateCode_BL01 = (byte)(span.BL01 ?? 0),
            BridgeNo_BID01 = span.BID01 ?? string.Empty,
            SubmittedBy = submittedBy,
            SpanConfigDesig_BSP01 = span.BSP01 ?? string.Empty,
            NumberOfSpans_BSP02 = span.BSP02.HasValue
                                     ? (decimal?)span.BSP02.Value
                                     : null,
            NumberOfBeamLines_BSP03 = span.BSP03.HasValue
                                     ? (decimal?)span.BSP03.Value
                                     : null,
            SpanMaterial_BSP04 = span.BSP04,
            SpanContinuity_BSP05 = span.BSP05,
            SpanType_BSP06 = span.BSP06,
            SpanProtectSystem_BSP07 = span.BSP07,
            DeckInteraction_BSP08 = span.BSP08,
            DeckMaterial_BSP09 = span.BSP09,
            WearingSurface_BSP10 = span.BSP10,
            DeckProtectSystem_BSP11 = span.BSP11,
            DeckReinforcSystem_BSP12 = span.BSP12,
            DeckStayInPlaceForms_BSP13 = span.BSP13,
            RecordStatus = span.RecordStatus ?? "Active"
        };


        // ------------------ Substructure Sets ------------------
        private Stage_BridgeSubstructureSet MapSubstructureSet(SNBIRecord.SubstructureSet sub, long submitId, string submittedBy) => new()
        {
            SubmitId = submitId,
            StateCode_BL01 = (byte)(sub.BL01 ?? 0),
            BridgeNo_BID01 = sub.BID01 ?? string.Empty,
            SubmittedBy = submittedBy,
            SubstructConfigDesig_BSB01 = sub.BSB01 ?? string.Empty,
            NoSubstructUnits_BSB02 = sub.BSB02.HasValue
                                         ? (decimal?)sub.BSB02.Value
                                         : null,
            SubstructMaterial_BSB03 = sub.BSB03,
            SubstructType_BSB04 = sub.BSB04,
            SubstructProtectSystem_BSB05 = sub.BSB05,
            FoundationType_BSB06 = sub.BSB06,
            FoundationProtectSystem_BSB07 = sub.BSB07,
            RecordStatus = sub.RecordStatus ?? "Active"
        };

        // ------------------ Work ------------------
        private Stage_BridgeWork MapWork(SNBIRecord.Work work, long submitId, string submittedBy) => new()
        {
            SubmitId = submitId,
            StateCode_BL01 = (byte)(work.BL01 ?? 0),
            BridgeNo_BID01 = work.BID01 ?? string.Empty,
            SubmittedBy = submittedBy,
            YearWorkPerformed_BW02 = (short)(work.BW02 ?? 0), // Default to 0 if null
            WorkPerformed_BW03 = work.BW03,
            RecordStatus = work.RecordStatus ?? "Active",
        };

        #endregion

        #region Date Helpers

        private DateOnly? ParseNullableDateOnly(string? dateStr)
        {
            if (!string.IsNullOrWhiteSpace(dateStr) &&
                DateOnly.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }

        #endregion
    }

}
