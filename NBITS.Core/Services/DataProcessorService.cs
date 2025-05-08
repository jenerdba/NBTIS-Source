
using RulesEngine.Models;
using NBTIS.Core.DTOs;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NBTIS.Core.Hub;
using Microsoft.AspNetCore.SignalR;
using NBTIS.Core.Services;

using static NBTIS.Core.DTOs.SNBIRecord;
using NBTIS.Core.Infrastructure;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Presentation;
using static FastExpressionCompiler.ExpressionCompiler;
using Microsoft.AspNetCore.Rewrite;
using System.Collections.Generic;
using NBTIS.Core.Interfaces;


namespace NBTIS.Core.Services
{
    public class DataProcessor 
    {
        private readonly IRulesService _rulesService;
        //private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<DataProcessor> _logger;
        private readonly SNBISanitizer _snbiSanitizer;
        private readonly IProgressNotifier _notifier;

        public DataProcessor(IRulesService rulesService, ILogger<DataProcessor> logger, SNBISanitizer snbiSanitizer, IProgressNotifier notifier)
        {
            _rulesService = rulesService;
            _logger = logger;
            _snbiSanitizer = snbiSanitizer;
            _notifier = notifier;
        }

        public async Task<List<RuleResultTree>> ValidateRecordAsync(SNBIRecord record, SNBIRecord sanitizedRecord, string submittedBy)
        {
            var resultsForRecord = new List<RuleResultTree>();

            if (IsBorderBridge(record))
            {
                _logger.LogWarning($"{record.BID01} - Border Bridge Neighboring State");
                resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("PrimaryBorderBridge", record, submittedBy));
                await ValidateBorderFeaturesAndRoutes(record, resultsForRecord);
            }
            else
            {
                resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("Critical", record));
                resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("Primary", record, submittedBy));

                await ValidateAdditionalComponents(record, resultsForRecord);
                await BridgeSafetyValidator.ValidateAsync(record, resultsForRecord);
            }

            var fatalKeys = BuildFatalKeySets(resultsForRecord);


            // ===> Do the silent fix-up for max-length fields
            _snbiSanitizer.PopulateSanitizedRecord(record, sanitizedRecord, fatalKeys);

            return resultsForRecord;
        }

        private bool ContainsCriticalError(List<RuleResultTree> results)
        {
            return results.Any(result => result.Rule.Properties.ContainsKey("ErrorType") && result.Rule.Properties["ErrorType"].ToString() == "Critical");
        }

        private async Task ValidateBorderFeaturesAndRoutes(SNBIRecord record, List<RuleResultTree> resultsForRecord)
        {
            if (record.Features != null)
            {
                foreach (var feature in record.Features)
                {
                    feature.BL01 = record.BL01;
                    feature.BL02 = record.BL02;
                    feature.BID01 = record.BID01;
                    feature.BCL01 = record.BCL01;
                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("FeaturesBorderBridge", feature));

                    if (feature.Routes != null)
                    {
                        foreach (var route in feature.Routes)
                        {
                            route.BL01 = record.BL01;
                            route.BL02 = record.BL02;
                            route.BID01 = record.BID01;
                            route.BCL01 = record.BCL01;
                            route.BF01 = feature.BF01;
                            route.BF02 = feature.BF02;
                            route.BF03 = feature.BF03;
                            resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("RoutesBorderBridge", route));
                        }
                    }
                }
            }
        }

        public RuleResultTree CreateStateMismatchResult(string errorMessage)
        {
            Rule stateMismatchRule = new Rule { RuleName = "StateMismatch", ErrorMessage = errorMessage };
            return new RuleResultTree
            {
                Rule = stateMismatchRule,
                IsSuccess = false
            };
        }

        private async Task ValidateAdditionalComponents(SNBIRecord record, List<RuleResultTree> resultsForRecord)
        {

            if (record.Elements != null)
            {
                foreach (var element in record.Elements)
                {
                    element.BL01 = record.BL01;
                    element.BL02 = record.BL02;
                    element.BID01 = record.BID01;
                    element.BCL01 = record.BCL01;

                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("Elements", element, record));
                }
            }
            if (record.Features != null)
            {
                foreach (var feature in record.Features)
                {

                    feature.BL01 = record.BL01;
                    feature.BL02 = record.BL02;
                    feature.BID01 = record.BID01;
                    feature.BCL01 = record.BCL01;
                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("Features", feature, record));

                    if (feature.Routes != null)
                    {
                        foreach (var route in feature.Routes)
                        {
                            route.BL01 = record.BL01;
                            route.BL02 = record.BL02;
                            route.BID01 = record.BID01;
                            route.BCL01 = record.BCL01;
                            route.BF01 = feature.BF01;
                            route.BF02 = feature.BF02;
                            route.BF03 = feature.BF03;

                            resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("Routes", route));
                        }
                    }
                }
            }
            if (record.Inspections != null)
            {
                foreach (var inspection in record.Inspections)
                {
                    inspection.BL01 = record.BL01;
                    inspection.BL02 = record.BL02;
                    inspection.BID01 = record.BID01;
                    inspection.BCL01 = record.BCL01;
                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("Inspections", inspection));
                }
            }

            if (record.PostingEvaluations != null)
            {
                foreach (var postingEvaluation in record.PostingEvaluations)
                {
                    postingEvaluation.BL01 = record.BL01;
                    postingEvaluation.BL02 = record.BL02;
                    postingEvaluation.BID01 = record.BID01;
                    postingEvaluation.BCL01 = record.BCL01;
                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("PostingEvaluations", postingEvaluation, record));
                }
            }

            if (record.PostingStatuses != null)
            {
                foreach (var postingStatus in record.PostingStatuses)
                {
                    postingStatus.BL01 = record.BL01;
                    postingStatus.BL02 = record.BL02;
                    postingStatus.BID01 = record.BID01;
                    postingStatus.BCL01 = record.BCL01; ;
                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("PostingStatuses", postingStatus));
                }
            }

            if (record.SpanSets != null)
            {
                foreach (var spanSet in record.SpanSets)
                {
                    spanSet.BL01 = record.BL01;
                    spanSet.BL02 = record.BL02;
                    spanSet.BID01 = record.BID01;
                    spanSet.BCL01 = record.BCL01;

                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("SpanSets", spanSet, record));
                }
            }

            if (record.SubstructureSets != null)
            {
                foreach (var substructureSet in record.SubstructureSets)
                {
                    substructureSet.BL01 = record.BL01;
                    substructureSet.BL02 = record.BL02;
                    substructureSet.BID01 = record.BID01;
                    substructureSet.BCL01 = record.BCL01;
                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("SubstructureSets", substructureSet));
                }
            }

            if (record.Works != null)
            {
                foreach (var work in record.Works)
                {
                    work.BL01 = record.BL01;
                    work.BL02 = record.BL02;
                    work.BID01 = record.BID01;
                    work.BCL01 = record.BCL01;
                    work.BW01 = record.BW01;
                    resultsForRecord.AddRange(await _rulesService.ValidateDataAsync("Works", work, record));
                }
            }
        }

        public bool IsBorderBridge(SNBIRecord record)
        {

            //return !string.IsNullOrWhiteSpace(record.BL10.ToString()) && !string.IsNullOrWhiteSpace(record.BL01.ToString()) && record.BL10 != record.BL01 && record.BL07 != "N";

            // 1) If BL07 says “N”, it’s not a border bridge at all
            if (record.BL07 == "N")
                return false;

            // 2) If BL10 is empty or null, not a border
            if (string.IsNullOrWhiteSpace(record.BL10))
                return false;

            // 3) If BL10 is numeric → compare to BL01
            if (int.TryParse(record.BL10, out int bl10Value))
            {
                return bl10Value != record.BL01;
            }

            // 4) Otherwise, it’s a border only if it’s one of the country codes
            return record.BL10 == "CA"
                || record.BL10 == "MX";
        }

        public List<SNBIRecord> RemoveNonNBIBridges(List<SNBIRecord> data, List<NonNBIBridge> nonNBIBridges)
        {
            int writeIndex = 0;

            for (int readIndex = 0; readIndex < data.Count; readIndex++)
            {
                var x = data[readIndex];
                bool isNonNBI =
                    (x.BG01.HasValue && x.BG01.Value < 20.0) ||
                    (!x.BG01.HasValue && x.BG02.HasValue && x.BG02.Value < 20.0);

                if (isNonNBI)
                {
                    // Add to the public NonNBIBridges list
                    nonNBIBridges.Add(new NonNBIBridge
                    {
                        BL01 = x.BL01,
                        BID01 = x.BID01,
                        BCL01 = x.BCL01,
                        BG01 = x.BG01,
                        BG02 = x.BG02
                    });
                }
                else
                {
                    // Keep the item in the data list
                    data[writeIndex] = x;
                    writeIndex++;
                }
            }

            // Remove the excess items from the original list
            if (writeIndex < data.Count)
            {
                data.RemoveRange(writeIndex, data.Count - writeIndex);
            }

            // Return the modified data list with non-NBI bridges removed
            return data;
        }

        public enum RecordType
        {
            Primary,
            Feature,
            Route,
            SpanSet,
            SubstructureSet,
            PostingStatus,
            PostingEvaluation,
            Inspection,
            Element,
            Work
        }

        // Build the dictionary of fatal key sets:
        private Dictionary<RecordType, HashSet<string>> BuildFatalKeySets(IEnumerable<RuleResultTree> ruleResults)
        {
            var fatalKeys = new Dictionary<RecordType, HashSet<string>>(Enum.GetValues(typeof(RecordType))
                .Cast<RecordType>()
                .ToDictionary(rt => rt, rt => new HashSet<string>(StringComparer.OrdinalIgnoreCase)));

            foreach (var result in ruleResults)
            {
                if (!result.IsSuccess &&
                    result.Rule?.Properties != null &&
                    result.Rule.Properties.TryGetValue("IsFatal", out var isFatalValue) &&
                    isFatalValue is string isFatal &&
                    isFatal.Equals("Yes", StringComparison.OrdinalIgnoreCase) &&
                    result.Inputs != null &&
                    result.Inputs.TryGetValue("input1", out var input))
                {
                    switch (input)
                    {
                        case SNBIRecord primary:
                            {
                                string key = $"{primary.BID01 ?? string.Empty}-{primary.BL01?.ToString() ??  string.Empty}";
                                fatalKeys[RecordType.Primary].Add(key);
                                break;
                            }
                        case SNBIRecord.Feature fRecord:
                            {
                                string key = fRecord.BF01 ?? string.Empty;
                                fatalKeys[RecordType.Feature].Add(key);
                                break;
                            }
                        case SNBIRecord.Route rRecord:
                            {
                                string key = $"{rRecord.BF01 ?? string.Empty}-{rRecord.BRT01 ?? string.Empty}";
                                fatalKeys[RecordType.Route].Add(key);
                                break;
                            }
                        case SNBIRecord.SpanSet spanRecord:
                            {
                                string key = spanRecord.BSP01 ?? string.Empty;
                                fatalKeys[RecordType.SpanSet].Add(key);
                                break;
                            }
                        case SNBIRecord.SubstructureSet subRecord:
                            {
                                string key = subRecord.BSB01 ?? string.Empty;
                                fatalKeys[RecordType.SubstructureSet].Add(key);
                                break;
                            }
                        case SNBIRecord.PostingStatus psRecord:
                            {
                                string key = psRecord.BPS02 ?? string.Empty;
                                fatalKeys[RecordType.PostingStatus].Add(key);
                                break;
                            }
                        case SNBIRecord.PostingEvaluation peRecord:
                            {
                                string key = peRecord.BEP01 ?? string.Empty;
                                fatalKeys[RecordType.PostingEvaluation].Add(key);
                                break;
                            }
                        case SNBIRecord.Inspection insRecord:
                            {
                                string key = $"{insRecord.BIE01 ?? string.Empty}-{insRecord.BIE02 ?? string.Empty}";
                                fatalKeys[RecordType.Inspection].Add(key);
                                break;
                            }
                        case SNBIRecord.Element elRecord:
                            {
                                string key = $"{elRecord.BE01 ?? string.Empty}-{elRecord.BE02 ?? string.Empty}";
                                fatalKeys[RecordType.Element].Add(key);
                                break;
                            }
                        case SNBIRecord.Work workRecord:
                            {
                                string key = workRecord.BW02?.ToString() ?? string.Empty;
                                fatalKeys[RecordType.Work].Add(key);
                                break;
                            }
                    }
                }
            }
            return fatalKeys;
        }


    }

}

