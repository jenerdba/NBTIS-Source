using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NBTIS.Core.DTOs;
using NBTIS.Data.Models;
using RulesEngine.Models;
using NBTIS.Core.Exceptions;
using NBTIS.Core.Enums;
using NBTIS.Core.Interfaces;
using NBTIS.Core.Mapping;
using AutoMapper;
using System.Threading;


namespace NBTIS.Core.Services
{
    public class FileValidationService
    {
        private readonly DataProcessor _processor;
        private readonly IDuplicateChecker _duplicateChecker;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _uploadTempPath;
        private readonly DataContext _context;
        private readonly StateValidatorService _stateValidatorService;
        private readonly FedAgencyValidatorService _fedAgencyValidatorService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IProgressNotifier _notifier;
        private readonly IMapper _mapper;

        public FileValidationService(
               DataProcessor processor,
               IDuplicateChecker duplicateChecker,
               ILogger<FileValidationService> logger,
               JsonSerializerOptions jsonOptions,
               IWebHostEnvironment env,
               DataContext context,
               StateValidatorService stateValidatorService,
        FedAgencyValidatorService fedAgencyValidatorService,
        ICurrentUserService currentUserService,
        ILoggerFactory loggerFactory,
        IProgressNotifier notifier,
        IMapper mapper)
        {
            _processor = processor;
            _duplicateChecker = duplicateChecker;
            _jsonOptions = jsonOptions;
            _uploadTempPath = Path.Combine(env.ContentRootPath, "temp");
            _context = context;
            _stateValidatorService = stateValidatorService;
            _fedAgencyValidatorService = fedAgencyValidatorService;
            _currentUserService = currentUserService;
            _logger = logger;
            _loggerFactory = loggerFactory;
            _notifier = notifier;
            _mapper = mapper;

        }

        public async Task<ProcessFileResult> ValidateFileAsync(
      string? tempFileName,
      string? uploadId,
      DateTime uploadDate,
      long submitId,
      string? submittedBy,
      string? connectionId,
      bool revalidate = false,
      CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Load records
                List<SNBIRecord> records = revalidate
                    ? await LoadRecordsFromStageAsync(submitId, cancellationToken)
                    : await LoadRecordsFromFileAsync(tempFileName, submitId, cancellationToken);

                if (!records.Any())
                    throw new InvalidOperationException("No records found to validate.");

                // 2. Normalize and de-duplicate
                var nonNBIBridges = new List<NonNBIBridge>();
                var nbiRecords = _processor.RemoveNonNBIBridges(records, nonNBIBridges);

                if (!nbiRecords.Any())
                {
                    await SetSubmittalStatusAsync(submitId, SubmittalStatus.ValidationFailed, token: cancellationToken);
                    throw new NoNbiBridgesException();
                }

                var distinctRecords = _duplicateChecker.CheckForDuplicates(nbiRecords);

                ///////******* 3. Validate data *******///////
                ///
                var (validationResults, outputData) = await ValidateAllDataAsync(distinctRecords, submittedBy, connectionId);

                var stateMismatch = validationResults
                    .FirstOrDefault(r => r.Rule?.RuleName == "StateMismatch");

                if (stateMismatch is not null)
                {
                    await SetSubmittalStatusAsync(submitId, SubmittalStatus.ValidationFailed, token: cancellationToken);
                    throw new StateMismatchException(
                        $"State Mismatch: {stateMismatch.Rule.ErrorMessage}");
                }

                // 4. Check for fatal validation errors
                var failedResults = validationResults.Where(r => !r.IsSuccess).ToList();
                bool hasFatal = failedResults.Any(IsFatalItemRule);

                if (hasFatal)
                {
                    await SetSubmittalStatusAsync(submitId, SubmittalStatus.ValidationFailed, token: cancellationToken);
                }
                else if (!revalidate)
                {
                    // Set status to NEW only if this is a fresh validation
                    await SetSubmittalStatusAsync(submitId, SubmittalStatus.New, token: cancellationToken);
                }

                // 5. Return result
                var (fatal, safety, critical, general, flag) = CountErrorTypes(failedResults);

                return new ProcessFileResult
                {
                    ReportData = new ProcessingReport
                    {
                        SubmitId = submitId,
                        SubmittedBy = submittedBy,
                        TotalRecordsUploaded = records.Count,
                        TotalRecordsOmitted = fatal,
                        TotalSafetyErrors = safety,
                        TotalCriticalErrors = critical,
                        TotalGeneralErrors = general,
                        TotalFlags = flag,
                        TotalDuplicateBridges = _duplicateChecker.GetTotalDuplicateBridges(),
                        NonNBIBridges = nonNBIBridges
                    },
                    ruleResults = validationResults,
                    StagingData = outputData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FileValidationService.ValidateFileAsync");
                throw;
            }
        }


        public async Task SetSubmittalStatusAsync(
    long submitId,
    SubmittalStatus newStatus,
    long? mergedInto = null,
    DateTime? submitDateOverride = null,
    CancellationToken token = default)
        {
            var record = await _context.SubmittalLogs.FirstOrDefaultAsync(r => r.SubmitId == submitId, token);

            if (record is null)
            {
                _logger.LogWarning($"No submittal log record found for SubmitId: {submitId}");
                return;
            }

            record.StatusCode = (byte)newStatus;

            if (mergedInto.HasValue)
            {
                record.MergedIntoSubmitId = mergedInto.Value;
            }

            // Set submit date if applicable
            if (newStatus is SubmittalStatus.DivisionReview or SubmittalStatus.Merged or SubmittalStatus.ValidationFailed)
            {
                record.SubmitDate = submitDateOverride ?? DateTime.Now;
                record.Submitter = _currentUserService.UserId;
            }

            await _context.SaveChangesAsync(token);
        }


        private Task<List<SNBIRecord>> LoadRecordsFromFileAsync(
            string fileName,
            long submitId,
            CancellationToken ct)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Uploaded file not found.", fileName);

            return DeserializeFileAsync(fileName, submitId);
        }

        private static readonly Func<DataContext, long, IAsyncEnumerable<Stage_BridgePrimary>> _compiledBatch =
         EF.CompileAsyncQuery(
                (DataContext ctx, long submitId) =>
                    ctx.Stage_BridgePrimaries
                        .AsNoTracking()
                        .Where(p => p.SubmitId == submitId)
                        .Include(p => p.Stage_BridgeElements)
                        .Include(p => p.Stage_BridgeFeatures)
                            .ThenInclude(f => f.Stage_BridgeRoutes)
                        .Include(p => p.Stage_BridgeInspections)
                        .Include(p => p.Stage_BridgePostingEvaluations)
                        .Include(p => p.Stage_BridgePostingStatuses)
                        .Include(p => p.Stage_BridgeSpanSets)
                        .Include(p => p.Stage_BridgeSubstructureSets)
                        .Include(p => p.Stage_BridgeWorks));

        private async Task<List<SNBIRecord>> LoadRecordsFromStageAsync(
            long submitId,
            CancellationToken ct)
        {
            var primaries = _compiledBatch(_context, submitId);

            var records = new List<SNBIRecord>(capacity: 8192); // pre-size for perf

            await foreach (var p in primaries.WithCancellation(ct))
            {
                records.Add(_mapper.Map<SNBIRecord>(p));
            }

            return records;
        }


        public async Task<(List<RuleResultTree> ValidationResults, List<SNBIRecord> OutputData)> 
            ValidateAllDataAsync(List<SNBIRecord>? data, string submittedBy, string? connectionId)
        {
            try
            {
                if (data == null || !data.Any())
                    throw new ArgumentNullException(nameof(data));

                var results = new List<RuleResultTree>();
                var outputData = new List<SNBIRecord>();
                int totalRecords = data.Count;
                ProgressTracker progress = new ProgressTracker();

                // Reset temporary counts before processing
                TemporaryCounts.Reset();

                if (int.TryParse(submittedBy, out int numericSelectedState))
                {
                    var first = data.First();
                    if (first.BL01.HasValue && first.BL01.Value != numericSelectedState)
                    {
                        string errorMessage = $"State code does not match for record {first.BID01}. "
                                            + $"Expected {submittedBy}, found {first.BL01.Value.ToString() ?? "null"}.";
                        results.Add(_processor.CreateStateMismatchResult(errorMessage));

                        // Return immediately with no sanitized data if the entire file fails state-code check
                        return (results, new List<SNBIRecord>());
                    }
                }

                // --- Loop through each record
                foreach (var record in data)
                {
                    SNBIRecord sanitizedRecord = new SNBIRecord();

                    // Validate the original record 
                    var recordResults = await _processor.ValidateRecordAsync(record, sanitizedRecord, submittedBy);

                    // Collect any validation errors
                    results.AddRange(recordResults);

                    // Add the sanitized record to our output list
                    outputData.Add(sanitizedRecord);

                    // Update progress if needed
                    if (connectionId != null)
                    {
                        await UpdateProgress(connectionId, totalRecords, progress);
                    }
                }

                // Return both the validation results and all the sanitized records
                return (results, outputData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DataProcessorService.ValidateAllDataAsync().");
                // Return empty stuff on error
                return (new List<RuleResultTree>(), new List<SNBIRecord>());
            }
        }

        private async Task UpdateProgress(string connectionId, int totalRecords, ProgressTracker progress)
        {
            progress.ProcessedRecords++;
            int incrementStep = 10;

            if (totalRecords >= 9)
            {
                int segmentSize = (int)Math.Round(totalRecords / 8.0);

                if (progress.ProcessedRecords % segmentSize == 0 && progress.ProcessedRecords != totalRecords)
                {
                    progress.PercentCompleted += incrementStep;
                    await _notifier.UpdateProgressAsync(connectionId, progress.PercentCompleted);
                }
            }
            else
            {
                incrementStep = 80 / totalRecords;

                if (progress.ProcessedRecords != totalRecords)
                {
                    progress.PercentCompleted += incrementStep;
                    await _notifier.UpdateProgressAsync(connectionId, progress.PercentCompleted);
                }
            }

            if (progress.ProcessedRecords == totalRecords)
            {
                progress.PercentCompleted = 90;
                await _notifier.UpdateProgressAsync(connectionId, progress.PercentCompleted);
            }
        }


        private bool IsFatalItemRule(RuleResultTree result)
        {
            // Ensure we have properties to work with
            if (result.Rule?.Properties is not IDictionary<string, object> props)
                return false;

            // Check if the ItemId is either BID01 or BL01
            if (!props.TryGetValue("ItemId", out var itemId) ||
                (itemId?.ToString() != "BID01" && itemId?.ToString() != "BL01"))
            {
                return false;
            }

            // Finally, check if IsFatal is set to Yes.
            return props.TryGetValue("IsFatal", out var isFatal) &&
                   string.Equals(isFatal?.ToString(), "Yes", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<List<SNBIRecord>> DeserializeFileAsync(string filePath, long submitId)
        {
            try
            {
                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var records = await JsonSerializer.DeserializeAsync<List<SNBIRecord>>(stream, _jsonOptions);
                return records ?? new List<SNBIRecord>();
            }
            catch (Exception ex)
            {
                await SetSubmittalStatusAsync(submitId, SubmittalStatus.ValidationFailed);

                throw new JsonFormatException($"Json Format Exception: {ex.Message}");
            }
        }

        private static (int fatal, int safety, int critical, int general, int flag) CountErrorTypes(IEnumerable<RuleResultTree> results)
        {
            int safety = 0, critical = 0, general = 0, flag = 0;

            // We'll need these for fallback logic:
            var fallbackRecordIds = new Dictionary<SNBIRecord, int>();
            int nextFallbackId = 1;

            var fatalPrimaryRecordKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var result in results)
            {
                // Skip successful results and those without properties.
                if (result.IsSuccess || result.Rule?.Properties == null)
                    continue;

                // 1) Tally up error types (Safety, Critical, Error, Flag).
                if (result.Rule.Properties.TryGetValue("ErrorType", out var errorTypeValue))
                {
                    var errorType = errorTypeValue?.ToString().ToLowerInvariant();
                    switch (errorType)
                    {
                        case "safety": safety++; break;
                        case "critical": critical++; break;
                        case "error": general++; break;
                        case "flag": flag++; break;
                    }
                }

                // 2) Count bridge records with Bridge Number BID01 or State Code BL01 is null or not valid. 
                if (result.Rule?.Properties?.TryGetValue("IsFatal", out var isFatalObj) == true &&
             string.Equals(isFatalObj?.ToString(), "Yes", StringComparison.OrdinalIgnoreCase))
                {
                    // Check "ItemId"
                    if (result.Rule.Properties.TryGetValue("ItemId", out var itemIdObj))
                    {
                        var itemId = itemIdObj?.ToString();
                        if (itemId == "BID01" || itemId == "BL01")
                        {
                            if (result.Inputs?.TryGetValue("input1", out var input) == true && input is SNBIRecord record)
                            {
                                // Convert BID01 and BL01 to strings (handle nulls).
                                string bid01 = record.BID01 ?? string.Empty;
                                string bl01 = record.BL01?.ToString() ?? string.Empty;

                                // If both are empty, we use the fallback dictionary to differentiate
                                // multiple distinct records with nulls in both fields.
                                if (string.IsNullOrEmpty(bid01) && string.IsNullOrEmpty(bl01))
                                {
                                    if (!fallbackRecordIds.TryGetValue(record, out var assignedId))
                                    {
                                        assignedId = nextFallbackId++;
                                        fallbackRecordIds[record] = assignedId;
                                    }
                                    bid01 = "FALLBACK";
                                    bl01 = assignedId.ToString();
                                }

                                string primaryKey = $"{bid01}-{bl01}";
                                fatalPrimaryRecordKeys.Add(primaryKey);
                            }
                        }
                    }
                }

            }

            int fatal = fatalPrimaryRecordKeys.Count;
            return (fatal, safety, critical, general, flag);
        }

        public async Task<string> GenerateExcelReportAsync(
        DateTime uploadDate,
        //string connectionId,
        ProcessFileResult processFileResult,
        CancellationToken cancellationToken = default)
        {
            try
            {
                var reportGeneratorLogger = _loggerFactory.CreateLogger<ProcessingReportGenerator>();

                string reportDateTime = uploadDate.ToString("yyyy-MM-dd HH:mm:ss");
                string userId = _currentUserService.UserId;
                var reportGenerator = new ProcessingReportGenerator(_stateValidatorService, _fedAgencyValidatorService, _context, _currentUserService, reportGeneratorLogger);

                // GenerateExcelContent creates an Excel file and returns its temporary file path.
                string tempExcelFileName = await reportGenerator.GenerateExcelContent(
                    ruleResults: processFileResult.ruleResults ?? new List<RuleResultTree>(),
                    reportData: processFileResult.ReportData,
                    stagingData: processFileResult.StagingData,
                    uploadDate: uploadDate,
                    _uploadTempPath: _uploadTempPath
                    );

                return tempExcelFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                throw;
            }
        }
    }
}
