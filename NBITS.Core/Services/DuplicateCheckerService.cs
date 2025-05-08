
using Microsoft.Extensions.Logging;
using NBTIS.Data.Models;
using NBTIS.Core.DTOs;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace NBTIS.Core.Services
{
    public interface IDuplicateChecker
    {
        List<SNBIRecord> CheckForDuplicates(List<SNBIRecord> data);
        Dictionary<Type, IList> DuplicateRecords { get; }
        int GetTotalDuplicateBridges();
    }

    public class DuplicateChecker : IDuplicateChecker
    {
        private readonly ILogger<DataProcessor> _logger;

        public Dictionary<Type, IList> DuplicateRecords { get; } = new Dictionary<Type, IList>();

        public DuplicateChecker(ILogger<DataProcessor> logger)
        {
            _logger = logger;
        }

        public int GetTotalDuplicateBridges()
        {
            if (DuplicateRecords.TryGetValue(typeof(ProcessingReport.Primary), out var duplicates))
            {
                return duplicates.Count;
            }
            return 0;
        }

        public List<SNBIRecord> CheckForDuplicates(List<SNBIRecord> data)
        {
            List<SNBIRecord> distinctData;

            try
            {
                if (data == null) throw new ArgumentNullException(nameof(data));

                var duplicatePrimary = GetDuplicatePrimary(data);
                DuplicateRecords[typeof(ProcessingReport.Primary)] = new List<ProcessingReport.Primary>(duplicatePrimary);

                distinctData = GetDistinctRecordsByPrimary(data);

                var duplicateElements = GetDuplicateElements(distinctData);
                DuplicateRecords[typeof(SNBIRecord.Element)] = duplicateElements;

                //Remove duplicate elements
                if (duplicateElements.Count > 0)
                {
                    RemoveDuplicates(distinctData, record => record.Elements, x => new { x.BE01, x.BE02 });
                }

                var duplicateFeatures = GetDuplicateFeatures(distinctData);
                DuplicateRecords[typeof(SNBIRecord.Feature)] = duplicateFeatures;

                //Remove duplicate features
                if (duplicateFeatures.Count > 0)
                {
                    RemoveDuplicates(distinctData, record => record.Features, x => x.BF01);
                }

                //Duplicate Routes
                var duplicateRoutes = GetDuplicateRoutes(distinctData);
                DuplicateRecords[typeof(SNBIRecord.Route)] = duplicateRoutes.ToList();
                if (duplicateRoutes.Count() > 0)
                {
                    RemoveDuplicateRoutes(distinctData);
                }

                //Remove duplicate inspections
                var duplicateInspections = GetDuplicateInspections(distinctData);
                DuplicateRecords[typeof(SNBIRecord.Inspection)] = duplicateInspections.ToList();
                if (duplicateInspections.Count() > 0)
                {
                    RemoveDuplicates(distinctData, record => record.Inspections, x => new { x.BIE01, x.BIE02 });
                }

                //Duplicate Posting Evaluations
                var duplicatePostingEvaluations = GetDuplicatePostingEvaluations(distinctData);
                DuplicateRecords[typeof(SNBIRecord.PostingEvaluation)] = duplicatePostingEvaluations.ToList();
                if (duplicatePostingEvaluations.Count() > 0)
                {
                    RemoveDuplicates(distinctData, record => record.PostingEvaluations, x => x.BEP01);
                }

                //Duplicate PostingStatuses
                var duplicatePostingStatuses = GetDuplicatePostingStatuses(distinctData);
                DuplicateRecords[typeof(SNBIRecord.PostingStatus)] = duplicatePostingStatuses.ToList();
                if (duplicatePostingStatuses.Count() > 0)
                {
                    RemoveDuplicates(distinctData, record => record.PostingStatuses, x => x.BPS01);
                }

                //Duplicate SpanSets
                var duplicateSpanSets = GetDuplicateSpanSets(distinctData);
                DuplicateRecords[typeof(SNBIRecord.SpanSet)] = duplicateSpanSets.ToList();
                if (duplicateSpanSets.Count() > 0)
                {
                    RemoveDuplicates(distinctData, record => record.SpanSets, x => x.BSP01);
                }

                //Duplicate SubstructureSets
                var duplicateSubstructureSets = GetDuplicateSubstructureSets(distinctData);
                DuplicateRecords[typeof(SNBIRecord.SubstructureSet)] = duplicateSubstructureSets.ToList();
                if (duplicateSubstructureSets.Count() > 0)
                {
                    RemoveDuplicates(distinctData, record => record.SubstructureSets, x => x.BSB01);
                }

                //Duplicate Works
                var duplicateWorks = GetDuplicateWorks(distinctData);
                DuplicateRecords[typeof(SNBIRecord.Work)] = duplicateWorks.ToList();
                if (duplicateWorks.Count() > 0)
                {
                    RemoveDuplicates(distinctData, record => record.Works, x => x.BW02);
                }



                return distinctData;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to check for duplicates: " + ex.Message);
                return data;
            }
        }

        private void RemoveDuplicateRoutes(IEnumerable<SNBIRecord> distinctData)
        {
            distinctData.Where(record => record.Features != null && record.Features.Any())
                .ToList()
                .ForEach(record =>
                {
                    record.Features.Where(feature => feature.Routes != null && feature.Routes.Any())
                    .ToList()
                    .ForEach(feature =>
                    {
                        feature.Routes = feature.Routes
                            .GroupBy(route => new { feature.BF01, route.BRT01 })  // Group by BF01 in Feature and BRT01 in Route
                            .Select(group => group.First())  // Select the first Route from each group, effectively removing duplicates
                            .ToList();
                    });
                });
        }

        private List<SNBIRecord> GetDistinctRecordsByPrimary(List<SNBIRecord> data)
        {
            return data.GroupBy(record => new { record.BL01, record.BID01 })
                       .Select(group => group.First())
                       .ToList();
        }

        private static void RemoveDuplicates<T, TKey>(List<SNBIRecord> items, Func<SNBIRecord, ICollection<T>> collectionSelector, Func<T, TKey> keySelector)
        {
            items.ForEach(item =>
            {
                var collection = collectionSelector(item);
                if (collection != null && collection.Any())
                {
                    var distinctItems = collection.GroupBy(keySelector).Select(g => g.First()).ToList();
                    collection.Clear();
                    foreach (var distinctItem in distinctItems)
                    {
                        collection.Add(distinctItem);
                    }
                }
            });
        }

        private IEnumerable<SNBIRecord.Route> GetDuplicateRoutes(List<SNBIRecord> data)
        {
            try
            {
                var routes = data
                    .Where(record => record.Features != null)
                    .SelectMany(record => record.Features, (record, child) => new
                    {
                        record.BL01,
                        record.BID01,
                        record.BCL01,
                        record.BL02,
                        child.BF01,
                        child.Routes

                    })
                    .Where(record => record.Routes != null)
                    .SelectMany(featureInfo => featureInfo.Routes, (featureInfo, route) => new
                    {
                        featureInfo.BL01,
                        featureInfo.BL02,
                        featureInfo.BID01,
                        featureInfo.BCL01,
                        featureInfo.BF01,
                        Route = route
                    })
                    .ToList();

                var duplicates = routes
                   .GroupBy(routeInfo => new { routeInfo.BL01, routeInfo.BID01, routeInfo.BF01, routeInfo.Route.BRT01, })
                   .Where(group => group.Count() > 1)
                   .SelectMany(group => group)
                   .Select(x => new SNBIRecord.Route
                   {
                       BL01 = x.BL01,
                       BL02 = x.BL02,
                       BID01 = x.BID01,
                       BCL01 = x.BCL01,
                       BF01 = x.BF01,
                       BRT01 = x.Route.BRT01,
                       BRT02 = x.Route.BRT02,
                       BRT03 = x.Route.BRT03,
                       BRT04 = x.Route.BRT04,
                       BRT05 = x.Route.BRT05

                   })
                    .OrderBy(x => x.BL01)
                    .ThenBy(x => x.BL02)
                    .ThenBy(x => x.BID01)
                    .ThenBy(x => x.BF01)
                   .ToList();

                return duplicates;
            }
            catch
            {
                throw;
            }
        }

        private IEnumerable<SNBIRecord.Work> GetDuplicateWorks(List<SNBIRecord> data)
        {
            var bridgeWorks = data
               .Where(record => record.Works != null)
               .SelectMany(record => record.Works, (record, child) => new
               {
                   record.BL01,
                   record.BL02,
                   record.BID01,
                   record.BCL01,
                   child.BW02,
                   child.BW03
               })
               .ToList();

            var duplicates = bridgeWorks
                .GroupBy(record => new { record.BL01, record.BID01, record.BW02 })
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .Select(x => new SNBIRecord.Work
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BW02 = x.BW02,
                    BW03 = x.BW03
                })
                 .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ToList();

            return duplicates;
        }

        private IEnumerable<SNBIRecord.SubstructureSet> GetDuplicateSubstructureSets(List<SNBIRecord> data)
        {
            var bridgeSubstractureSet = data
                .Where(record => record.SubstructureSets != null)
                .SelectMany(record => record.SubstructureSets, (record, child) => new
                {
                    record.BL01,
                    record.BL02,
                    record.BID01,
                    record.BCL01,
                    child.BSB01,
                    child.BSB02,
                    child.BSB03,
                    child.BSB04,
                    child.BSB05,
                    child.BSB06,
                    child.BSB07
                })
                .ToList();

            var duplicates = bridgeSubstractureSet
                .GroupBy(record => new { record.BL01, record.BID01, record.BSB01 })
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .Select(x => new SNBIRecord.SubstructureSet
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BSB01 = x.BSB01,
                    BSB02 = x.BSB02,
                    BSB03 = x.BSB03,
                    BSB04 = x.BSB04,
                    BSB05 = x.BSB05,
                    BSB06 = x.BSB06,
                    BSB07 = x.BSB07
                })
                 .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ToList();

            return duplicates;
        }

        private IEnumerable<SNBIRecord.SpanSet> GetDuplicateSpanSets(List<SNBIRecord> data)
        {
            var bridgeSpanSets = data
                .Where(record => record.SpanSets != null)
                .SelectMany(record => record.SpanSets, (record, child) => new
                {
                    record.BL01,
                    record.BL02,
                    record.BID01,
                    record.BCL01,
                    child.BSP01,
                    child.BSP02,
                    child.BSP03,
                    child.BSP04,
                    child.BSP05,
                    child.BSP06,
                    child.BSP07,
                    child.BSP08,
                    child.BSP09,
                    child.BSP10,
                    child.BSP11,
                    child.BSP12,
                    child.BSP13

                })
                .ToList();

            var duplicates = bridgeSpanSets
                .GroupBy(record => new { record.BL01, record.BID01, record.BSP01 })
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .Select(x => new SNBIRecord.SpanSet
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BSP01 = x.BSP01,
                    BSP02 = x.BSP02,
                    BSP03 = x.BSP03,
                    BSP04 = x.BSP04,
                    BSP05 = x.BSP05,
                    BSP06 = x.BSP06,
                    BSP07 = x.BSP07,
                    BSP08 = x.BSP08,
                    BSP09 = x.BSP09,
                    BSP10 = x.BSP10,
                    BSP11 = x.BSP11,
                    BSP12 = x.BSP12,
                    BSP13 = x.BSP13
                })
                 .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ToList();

            return duplicates;
        }

        private IEnumerable<SNBIRecord.PostingStatus> GetDuplicatePostingStatuses(List<SNBIRecord> data)
        {
            var bridgePostingStatuses = data
                .Where(record => record.PostingStatuses != null)
                .SelectMany(record => record.PostingStatuses, (record, child) => new
                {
                    record.BL01,
                    record.BL02,
                    record.BID01,
                    record.BCL01,
                    child.BPS01,
                    child.BPS02

                })
                .ToList();

            var duplicates = bridgePostingStatuses
                .GroupBy(record => new { record.BL01, record.BID01, record.BPS02 })
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .Select(x => new SNBIRecord.PostingStatus
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BPS01 = x.BPS01,
                    BPS02 = x.BPS02
                })
                 .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ThenBy(x => x.BPS02)
                .ToList();

            return duplicates;
        }

        private IEnumerable<SNBIRecord.PostingEvaluation> GetDuplicatePostingEvaluations(List<SNBIRecord> data)
        {
            var bridgePostingEvaluations = data
                .Where(record => record.PostingEvaluations != null)
                .SelectMany(record => record.PostingEvaluations, (record, child) => new
                {
                    record.BL01,
                    record.BL02,
                    record.BID01,
                    record.BCL01,
                    child.BEP01,
                    child.BEP02,
                    child.BEP03,
                    child.BEP04
                })
                .ToList();

            var duplicates = bridgePostingEvaluations
                .GroupBy(record => new { record.BL01, record.BID01, record.BEP01 })
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .Select(x => new SNBIRecord.PostingEvaluation
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BEP01 = x.BEP01,
                    BEP02 = x.BEP02,
                    BEP03 = x.BEP03,
                    BEP04 = x.BEP04
                })
                 .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ToList();

            return duplicates;
        }

        private IEnumerable<SNBIRecord.Inspection> GetDuplicateInspections(List<SNBIRecord> data)
        {
            var bridgeInspections = data
                .Where(record => record.Inspections != null)
                .SelectMany(record => record.Inspections, (record, child) => new
                {
                    record.BL01,
                    record.BL02,
                    record.BID01,
                    record.BCL01,
                    child.BIE01,
                    child.BIE02,
                    child.BIE03,
                    child.BIE04,
                    child.BIE05,
                    child.BIE06,
                    child.BIE07,
                    child.BIE08,
                    child.BIE09,
                    child.BIE10,
                    child.BIE11,
                    child.BIE12
                })
                .ToList();

            var duplicates = bridgeInspections
                .GroupBy(record => new { record.BL01, record.BID01, record.BIE01, record.BIE02 }) // Grouping by composite key
                .Where(group => group.Count() > 1)
                .SelectMany(group => group) // Flatten the groups of duplicates into a single list
                .Select(x => new SNBIRecord.Inspection
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BIE01 = x.BIE01,
                    BIE02 = x.BIE02,
                    BIE03 = x.BIE03,
                    BIE04 = x.BIE04,
                    BIE05 = x.BIE05,
                    BIE06 = x.BIE06,
                    BIE07 = x.BIE07,
                    BIE08 = x.BIE08,
                    BIE09 = x.BIE09,
                    BIE10 = x.BIE10,
                    BIE11 = x.BIE11,
                    BIE12 = x.BIE12
                })
                 .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ToList();

            return duplicates;
        }

        private List<SNBIRecord.Feature> GetDuplicateFeatures(List<SNBIRecord> data)
        {
            var bridgeFeatures = data
                .Where(record => record.Features != null)
                .SelectMany(record => record.Features, (record, feature) => new
                {
                    record.BL01,
                    record.BL02,
                    record.BID01,
                    record.BCL01,
                    feature.BF01,
                    feature.BF02,
                    feature.BF03,
                    feature.BH01,
                    feature.BH02,
                    feature.BH03,
                    feature.BH04,
                    feature.BH05,
                    feature.BH06,
                    feature.BH07,
                    feature.BRR01,
                    feature.BRR02,
                    feature.BRR03,
                    feature.BN01,
                    feature.BN02,
                    feature.BN03
                })
                .ToList();

            var duplicates = bridgeFeatures
                .GroupBy(record => new { record.BL01, record.BID01, record.BF01 }) // Grouping by composite key
                .Where(group => group.Count() > 1)
                .SelectMany(group => group) // Flatten the groups of duplicates into a single list
                .Select(x => new SNBIRecord.Feature
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BF01 = x.BF01,
                    BF02 = x.BF02,
                    BF03 = x.BF03,
                    BH01 = x.BH01,
                    BH02 = x.BH02,
                    BH03 = x.BH03,
                    BH04 = x.BH04,
                    BH05 = x.BH05,
                    BH06 = x.BH06,
                    BH07 = x.BH07,
                    BRR01 = x.BRR01,
                    BRR02 = x.BRR02,
                    BRR03 = x.BRR03,
                    BN01 = x.BN01,
                    BN02 = x.BN02,
                    BN03 = x.BN03
                })
                 .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ToList();

            return duplicates;
        }

        private List<SNBIRecord.Element> GetDuplicateElements(List<SNBIRecord> data)
        {
            var bridgeElements = data
                .Where(record => record.Elements != null)
                .SelectMany(record => record.Elements, (record, element) => new
                {
                    record.BL01,
                    record.BL02,
                    record.BID01,
                    record.BCL01,
                    element.BE01,
                    element.BE02,
                    element.BE03,
                    element.BCS01,
                    element.BCS02,
                    element.BCS03,
                    element.BCS04
                })
                .ToList();

            var duplicates = bridgeElements
                .GroupBy(element => new { element.BL01, element.BID01, element.BE01, element.BE02 }) // Grouping by composite key
                .Where(group => group.Count() > 1)
                .SelectMany(group => group) // Flatten the groups of duplicates into a single list
                .Select(x => new SNBIRecord.Element
                {
                    BL01 = x.BL01,
                    BL02 = x.BL02,
                    BID01 = x.BID01,
                    BCL01 = x.BCL01,
                    BE01 = x.BE01,
                    BE02 = x.BE02,
                    BE03 = x.BE03,
                    BCS01 = x.BCS01,
                    BCS02 = x.BCS02,
                    BCS03 = x.BCS03,
                    BCS04 = x.BCS04
                })
                .OrderBy(x => x.BL01)
                .ThenBy(x => x.BL02)
                .ThenBy(x => x.BID01)
                .ToList();

            return duplicates;

        }

        private List<ProcessingReport.Primary> GetDuplicatePrimary(List<SNBIRecord> data)
        {
            var duplicates = new List<ProcessingReport.Primary>();

            var query = data
                .GroupBy(record => new { record.BL01, record.BID01 })
                .Select(g => new { StateCode = g.Key.BL01, BridgeNo = g.Key.BID01, Count = g.Count() })
                .Where(g => g.Count > 1);

            foreach (var result in query)
            {
                var duplicateEntries = data.Where(x => x.BL01 == result.StateCode && x.BID01 == result.BridgeNo)
                    .Select(a => new ProcessingReport.Primary
                    {
                        BL01 = a.BL01,
                        BL02 = a.BL02,
                        BID01 = a.BID01,
                        BID02 = a.BID02,
                        BCL01 = a.BCL01,
                        BL11 = a.BL11
                    }).ToList();

                duplicates.AddRange(duplicateEntries);  // Add range instead of replacing
            }

            // Order the results by BL01, then BL02, then BID01
            duplicates = duplicates.OrderBy(p => p.BL01).ThenBy(p => p.BL02).ThenBy(p => p.BID01).ToList();

            return duplicates;
        }
    }
}
