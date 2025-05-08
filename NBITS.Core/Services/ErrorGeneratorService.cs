using RulesEngine.Models;
using NBTIS.Core.DTOs;
using System.Collections.Concurrent;
using System.Reflection;
using static NBTIS.Core.DTOs.ProcessingReport;
using static NBTIS.Core.Utilities.Constants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NBTIS.Core.Services
{
    public class ErrorGeneratorService
    {
        private readonly StateValidatorService _stateValidatorService;
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyInfoCache = new ConcurrentDictionary<(Type, string), PropertyInfo?>();

        public ErrorGeneratorService(StateValidatorService stateValidatorService )
        {
            _stateValidatorService = stateValidatorService;
        }

        public List<ProcessingReport.Error> GenerateSafetyErrors(List<RuleResultTree> ruleResults)
        {
            var errors = new List<ProcessingReport.Error>();

            foreach (var result in ruleResults)
            {
                if (result.Inputs.TryGetValue("input1", out var input) && input is SNBIRecord record)
                {
                    // Attempt to retrieve the 'ErrorType' property and check if it equals "Safety"
                    if (!result.IsSuccess && result.Rule.Properties.TryGetValue("ErrorType", out var errorTypeValue) && errorTypeValue is string errorType && errorType == "Safety")
                    {
                        
                            var latestPostingStatus = record.PostingStatuses?.OrderByDescending(s => s.BPS02).FirstOrDefault();

                        ProcessingReport.Error error = new ProcessingReport.Error
                            {
                                State = record.BL01,
                                BID01 = record.BID01,
                                BCL01 = record.BCL01,
                                ItemId = "BPS01",
                                ItemName = "Load Posting Status",
                                SubmittedValue = latestPostingStatus?.BPS01 ?? "",
                                ValidationType = result.Rule.RuleName?.Contains("BLR07") ?? false ? "Posted Bridge" : "Closed Bridge",
                                ErrorType = errorType,
                                ErrorCode = result.Rule.RuleName,
                                Description = result.ExceptionMessage
                            };
                            errors.Add(error);
                    }
                    

                }
            }

            List<ProcessingReport.Error> sortedErrors = errors.OrderBy(e => e.State).ThenBy(e => e.BID01).ThenBy(e => e.ItemId).ToList();

            return sortedErrors;
        }

        public List<ProcessingReport.Error> GeneratePrimaryErrors(IEnumerable<RuleResultTree> ruleResults)
        {
            List<ProcessingReport.Error> errors = new List<ProcessingReport.Error>();

            foreach (var result in ruleResults)
            {
                if (result.Inputs.TryGetValue("input1", out object input) && input is SNBIRecord record)
                {
                        var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                        var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                        var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                        var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                    // var (itemId, itemName) = GetItemId(result);

                    string? submittedValue = GetPropertyValue(record, itemId);

                    ProcessingReport.Error error = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            DataSet = dataSet,
                        ErrorCode = result.Rule.RuleName,
                        Description = result.ExceptionMessage
                        };
                        errors.Add(error);
                    
                }
            };

            var sortedErrors = errors.OrderBy(e => e.State).ThenBy(e => e.ErrorType).ThenBy(e => e.BID01).ThenBy(e => e.ItemId).ToList();


            return sortedErrors;
        }

        public List<ElementError> GenerateElementErrors(List<RuleResultTree> ruleResults)
        {
            List<ElementError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.Element elRecord)
                {
                    string? submittedValue = GetPropertyValue(elRecord, itemId);

                    var error = new ElementError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = elRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = elRecord.BID01,
                            BCL01 = elRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        ElementDetails = elRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet) && parsedDataSet == DataSet.Elements)
                {
                    var error = new ElementError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage
                        },
                        ElementDetails = new SNBIRecord.Element()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue;
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        public List<FeatureError> GenerateFeatureErrors(List<RuleResultTree> ruleResults)
        {
            List<FeatureError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.Feature fsRecord)
                {
                    string? submittedValue = GetPropertyValue(fsRecord, itemId);

                    var error = new FeatureError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = fsRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = fsRecord.BID01,
                            BCL01 = fsRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        FeatureDetails = fsRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record
                        && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet)
                        && parsedDataSet == DataSet.Features)
                {
                    var error = new FeatureError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        FeatureDetails = new SNBIRecord.Feature()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue; 
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                 .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        public List<RouteError> GenerateRouteErrors(List<RuleResultTree> ruleResults)
        {
            List<RouteError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.Route routeRecord)
                {
                    string? submittedValue = GetPropertyValue(routeRecord, itemId);

                    var error = new RouteError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = routeRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = routeRecord.BID01,
                            BCL01 = routeRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        RouteDetails = routeRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record
                        && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet)
                        && parsedDataSet == DataSet.Routes)
                {
                    //string? submittedValue = GetPropertyValue(routeRecord, itemId);

                    var error = new RouteError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        RouteDetails = new SNBIRecord.Route()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue;
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        public List<InspectionError> GenerateInspectionErrors(List<RuleResultTree> ruleResults)
        {
            List<InspectionError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.Inspection inspRecord)
                {
                    string? submittedValue = GetPropertyValue(inspRecord, itemId);

                    var error = new InspectionError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = inspRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = inspRecord.BID01,
                            BCL01 = inspRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        InspectionDetails = inspRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record
                        && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet)
                        && parsedDataSet == DataSet.Inspections)
                {
                    var error = new InspectionError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        InspectionDetails = new SNBIRecord.Inspection()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue; 
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        public List<SpanSetError> GenerateSpanSetErrors(List<RuleResultTree> ruleResults)
        {
            List<SpanSetError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.SpanSet spanRecord)
                {
                    string? submittedValue = GetPropertyValue(spanRecord, itemId);

                    var error = new SpanSetError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = spanRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = spanRecord.BID01,
                            BCL01 = spanRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        SpanSetDetails = spanRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record
                        && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet)
                        && parsedDataSet == DataSet.SpanSets)
                {
                    var error = new SpanSetError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        SpanSetDetails = new SNBIRecord.SpanSet()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue;
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        public List<PostingStatusError> GeneratePostingStatusErrors(List<RuleResultTree> ruleResults)
        {
            List<PostingStatusError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.PostingStatus spanRecord)
                {
                    string? submittedValue = GetPropertyValue(spanRecord, itemId);

                    var error = new PostingStatusError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = spanRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = spanRecord.BID01,
                            BCL01 = spanRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        PostingStatusDetails = spanRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record
                        && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet)
                        && parsedDataSet == DataSet.PostingStatuses)
                {
                    var error = new PostingStatusError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        PostingStatusDetails = new SNBIRecord.PostingStatus()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue;
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }
       
        public List<PostingEvaluationsError> GeneratePostingEvaluationErrors(List<RuleResultTree> ruleResults)
        {
            List<PostingEvaluationsError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.PostingEvaluation peRecord)
                {
                    string? submittedValue = GetPropertyValue(peRecord, itemId);

                    var postingEvaluationError = new PostingEvaluationsError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = peRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = peRecord.BID01,
                            BCL01 = peRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        PostingEvaluationsDetails = peRecord
                    };
                    errors.Add(postingEvaluationError);
                }
                else if (input is SNBIRecord record
                        && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet)
                        && parsedDataSet == DataSet.PostingEvaluations)
                {
                    var postingEvaluationError = new PostingEvaluationsError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        PostingEvaluationsDetails = new SNBIRecord.PostingEvaluation()
                    };
                    errors.Add(postingEvaluationError);
                }
                else
                {
                    continue;
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        public List<SubstructureSetError> GenerateSubstructureSetErrors(List<RuleResultTree> ruleResults)
        {
            List<SubstructureSetError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.SubstructureSet subRecord)
                {
                    string? submittedValue = GetPropertyValue(subRecord, itemId);

                    var error = new SubstructureSetError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = subRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = subRecord.BID01,
                            BCL01 = subRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        SubstructureSetDetails = subRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet) && parsedDataSet == DataSet.SubstructureSets)
                {
                    var error = new SubstructureSetError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        SubstructureSetDetails = new SNBIRecord.SubstructureSet()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue; // Skip if input is neither SNBIRecord.PostingEvaluation nor SNBIRecord
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        public List<WorkError> GenerateWorkErrors(List<RuleResultTree> ruleResults)
        {
            List<WorkError> errors = new();

            foreach (var result in ruleResults)
            {
                if (result.IsSuccess || result.Inputs == null)
                    continue;

                if (!result.Inputs.TryGetValue("input1", out object input))
                    continue;

                var errorType = result.Rule?.Properties?.GetValueOrDefault("ErrorType") as string ?? "";
                var itemId = result.Rule?.Properties?.GetValueOrDefault("ItemId") as string ?? "";
                var itemName = result.Rule?.Properties?.GetValueOrDefault("ItemName") as string ?? "";
                var dataSet = result.Rule?.Properties?.GetValueOrDefault("DataSet") as string ?? "";

                if (input is SNBIRecord.Work workRecord)
                {
                    string? submittedValue = GetPropertyValue(workRecord, itemId);

                    var error = new WorkError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = workRecord.BL01,
                            //County = !isFedAgency ? GetCounty(peRecord.BL01, peRecord.BL02) : null,
                            BID01 = workRecord.BID01,
                            BCL01 = workRecord.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            SubmittedValue = submittedValue,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        WorkDetails = workRecord
                    };
                    errors.Add(error);
                }
                else if (input is SNBIRecord record && Enum.TryParse<DataSet>(dataSet, true, out var parsedDataSet) && parsedDataSet == DataSet.Works)
                {
                    var error = new WorkError
                    {
                        ErrorDetails = new ProcessingReport.Error
                        {
                            State = record.BL01,
                            BID01 = record.BID01,
                            BCL01 = record.BCL01,
                            ItemId = itemId,
                            ItemName = itemName,
                            ErrorType = errorType,
                            ErrorCode = result.Rule.RuleName,
                            Description = result.ExceptionMessage,
                            DataSet = dataSet
                        },
                        WorkDetails = new SNBIRecord.Work()
                    };
                    errors.Add(error);
                }
                else
                {
                    continue; 
                }
            }

            var sortedErrors = errors
                .OrderBy(e => e.ErrorDetails.State)
                .ThenBy(e => e.ErrorDetails.ErrorType)
                .ThenBy(e => e.ErrorDetails.BID01)
                .ThenBy(e => e.ErrorDetails.ItemId)
                .ToList();

            return sortedErrors;
        }

        //private (string, string) GetItemId(RuleResultTree result)
        //{
        //    // Initial checks for null to avoid processing when not necessary
        //    if (result?.Rule?.RuleName == null)
        //    {
        //        return (string.Empty, string.Empty);
        //    }

        //    // Extracting ItemId from RuleName
        //    var parts = result.Rule.RuleName.Split('-');
        //    if (parts.Length == 0)
        //    {
        //        return (string.Empty, string.Empty);
        //    }

        //    // Assign the first part as itemId
        //    string itemId = parts[0].Trim();  // Trim to remove any leading/trailing spaces

        //    // Look up itemName using itemId in the dictionary
        //    if (Constants.bridgeData.TryGetValue(itemId, out string itemName))
        //    {
        //        return (itemId, itemName);
        //    }

        //    return (string.Empty, string.Empty);
        //}

        private string? GetPropertyValue(object record, string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || record == null)
            {
                return null;
            }

            Type recordType = record.GetType();
            var key = (recordType, propertyName);

            var propertyInfo = PropertyInfoCache.GetOrAdd(key, _ => recordType.GetProperty(propertyName));

            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(record);
                return value?.ToString();
            }

            return null;
        }


        internal string GetState(int? BL01)
        {
            if (!BL01.HasValue)
                return string.Empty;

            return _stateValidatorService.GetAbbreviationByCode(BL01.Value.ToString());
        }

        //internal string GetCounty(int? BL01, int? BL02)
        //{
        //    if (!BL01.HasValue)
        //        return string.Empty;

        //    return BL02.HasValue ? Counties.GetCountyNameByCode(BL01.Value, BL02.Value) : string.Empty;
        //}




    }
}
