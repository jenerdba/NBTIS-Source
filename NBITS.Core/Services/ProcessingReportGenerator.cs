using ClosedXML.Excel;
using RulesEngine.Models;
using NBTIS.Core.DTOs;
using static NBTIS.Core.DTOs.ProcessingReport;
using System.Collections;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using NBTIS.Data.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing.Template;


namespace NBTIS.Core.Services
{
    public class ProcessingReportGenerator
    {
        private string reportDate;
        private readonly ErrorGeneratorService _errorGenerator;
        private readonly StateValidatorService _stateValidatorService;
        private readonly FedAgencyValidatorService _fedAgencies;
        private readonly DataContext _context;
        private readonly ILogger<ProcessingReportGenerator> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ProcessingReportGenerator(
            StateValidatorService stateValidatorService,
            FedAgencyValidatorService fedAgencies,
            DataContext context,
            ICurrentUserService currentUserService,
            ILogger<ProcessingReportGenerator> logger)
        {
            _stateValidatorService = stateValidatorService;
            _fedAgencies = fedAgencies;
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
            _errorGenerator = new ErrorGeneratorService(_stateValidatorService);
        }

        private string AddTitle(string stateAgencyCode)
        {
            if (!string.IsNullOrEmpty(stateAgencyCode))
            {
                bool isNumeric = int.TryParse(stateAgencyCode, out int numericCode);

                // If it's an integer, attempt to retrieve the state name by its code
                string titleValue = isNumeric ? _stateValidatorService.GetNameByCode(stateAgencyCode) : _fedAgencies.GetNameByCode(stateAgencyCode);

                return titleValue;
            }

            return string.Empty;
        }

        public async Task<string> GenerateExcelContent(List<RuleResultTree> ruleResults, ProcessingReport reportData, List<SNBIRecord> stagingData, DateTime uploadDate, string _uploadTempPath)
        {
            reportDate = uploadDate.ToString("yyyy-MM-dd HH:mm:ss");
            string title = AddTitle(reportData.SubmittedBy);

            // Load the template Excel file
            var templatePath = "Templates/snbi_report_template.xlsx";
            using var workbook = new XLWorkbook(templatePath);

            //Processing Summary Tab
            PopulateProcessingSummaryWs(workbook.Worksheet(1), reportData, title);

            var wss = workbook.Worksheets;

            PopulateToBeRemovedWs(workbook.Worksheet("To Be Removed"), stagingData, title);


            //Temporary Counts Tab
            CreateTemporaryCountSheet(workbook.Worksheet("Temporary Codes"), title);

            //updateProgress(95);

            //Safety Validation
            var safetyErrors = _errorGenerator.GenerateSafetyErrors(ruleResults);
            PopulateSafetyWs(workbook.Worksheet("Safety Validation"), safetyErrors, title);

            //Non-NBIS Length Bridges
            PopulateNonNBISBridgesWs(workbook.Worksheet("Non-NBIS Length Bridges"), reportData.NonNBIBridges, title);

            var primaryRuleResults = ruleResults.Where(r => !r.IsSuccess && r.Rule?.Properties?.GetValueOrDefault("DataSet")?.ToString() == "Primary");
            List<ProcessingReport.Error> errors = _errorGenerator.GeneratePrimaryErrors(primaryRuleResults);
            PopulatePrimaryWs(workbook.Worksheet("Primary Errors"), errors, title);

            //updateProgress(98);

            List<ElementError> elementErrors = _errorGenerator.GenerateElementErrors(ruleResults);
            PopulateElementWs(workbook.Worksheet("Element Errors"), elementErrors, title);

            List<FeatureError> featureErrors = _errorGenerator.GenerateFeatureErrors(ruleResults);
            List<RouteError> routeErrors = _errorGenerator.GenerateRouteErrors(ruleResults);
            PopulateFeatureWs(workbook.Worksheet("Feature & Route Errors"), featureErrors, routeErrors, title);

            List<InspectionError> inspectionErrors = _errorGenerator.GenerateInspectionErrors(ruleResults);
            PopulateInspectionWs(workbook.Worksheet("Inspection Errors"), inspectionErrors, title);

            List<SpanSetError> spanSetErrors = _errorGenerator.GenerateSpanSetErrors(ruleResults);
            PopulateSpanSetWs(workbook.Worksheet("Span Set Errors"), spanSetErrors, title);

            List<PostingStatusError> postingStatusErrors = _errorGenerator.GeneratePostingStatusErrors(ruleResults);
            PopulatePostingStatusWs(workbook.Worksheet("Posting Status Errors"), postingStatusErrors, title);

            List<PostingEvaluationsError> postingEvaluationErrors = _errorGenerator.GeneratePostingEvaluationErrors(ruleResults);
            PopulatePostingEvaluationsWs(workbook.Worksheet("Posting Evaluation Errors"), postingEvaluationErrors, title);

            List<SubstructureSetError> substructureSetErrors = _errorGenerator.GenerateSubstructureSetErrors(ruleResults);
            PopulateSubstructureSetWs(workbook.Worksheet("Substructure Set Errors"), substructureSetErrors, title);

            var workRuleResults = ruleResults.Where(r => !r.IsSuccess && r.Rule?.Properties?.GetValueOrDefault("DataSet")?.ToString() == "Work");
            List<WorkError> workErrors = _errorGenerator.GenerateWorkErrors(ruleResults);
            PopulateWorkWs(workbook.Worksheet("Work Errors"), workErrors, title);

            //Validation Summary
            List<ProcessingReport.Error> combinedErrors =
           [
               .. errors.ConvertAll(e => new ProcessingReport.Error { State = e.State, BID01 = e.BID01, BCL01 = e.BCL01, ItemId = e.ItemId, ItemName = e.ItemName, SubmittedValue = e.SubmittedValue, ErrorType = e.ErrorType, ErrorCode = e.ErrorCode, Description = e.Description, DataSet = e.DataSet }),
                 .. safetyErrors.ConvertAll(e => new ProcessingReport.Error { State = e.State, BID01 = e.BID01, BCL01 = e.BCL01, ItemId = e.ItemId, ItemName = e.ItemName, SubmittedValue = e.SubmittedValue, ErrorType = e.ErrorType,  ErrorCode = e.ErrorCode, Description = e.Description, DataSet = e.DataSet }),
                 .. elementErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType,  ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. featureErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. routeErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. inspectionErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. spanSetErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. postingStatusErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. postingEvaluationErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. substructureSetErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),
                 .. workErrors.ConvertAll(e => new ProcessingReport.Error { State = e.ErrorDetails.State, BID01 = e.ErrorDetails.BID01, BCL01 = e.ErrorDetails.BCL01, ItemId = e.ErrorDetails.ItemId, ItemName = e.ErrorDetails.ItemName, SubmittedValue = e.ErrorDetails.SubmittedValue, ErrorType = e.ErrorDetails.ErrorType, ErrorCode = e.ErrorDetails.ErrorCode, Description = e.ErrorDetails.Description, DataSet = e.ErrorDetails.DataSet }),

             ];

            List<ErrorSummary> errorSummary = SortSummary(SummarizeErrors(combinedErrors));
            
            PopulateValidationSummaryWs(workbook.Worksheet("Validation Summary"), errorSummary, title);

            //Critical Errors Summary
            var criticalErrors = combinedErrors.Where(e => e.ErrorType == "Critical").OrderBy(e => e.BCL01).ThenBy(e => e.BID01).ThenBy(e => e.ItemId);

            PopulateCriticalErrorSummaryWs(workbook.Worksheet("Critical Error Summary"), criticalErrors, title);

            //All Errors Worksheet
            List<ProcessingReport.Error> sortedErrors = SortErrors(combinedErrors);
            
            //----------- Insert Submittal Errors into DB ---------------
            await InsertSubmittalErrorsDB(reportData.SubmitId, reportData.SubmittedBy, sortedErrors, _currentUserService.UserId);

            PopulateAllErrorsWs(workbook.Worksheet("Error Summary"), sortedErrors, title);

            //Populate Duplicate Worksheets
            PopulateDuplicatePrimaryWs(workbook.Worksheet("Duplicate Bridge Records"), reportData.Duplicates, title);
            PopulateDuplicateElementWs(workbook.Worksheet("Duplicate Elements"), reportData.Duplicates, title);
            PopulateDuplicateFeatureWs(workbook.Worksheet("Duplicate Features"), reportData.Duplicates, title);
            PopulateDuplicateRoutesWs(workbook.Worksheet("Duplicate Routes"), reportData.Duplicates, title);
            PopulateDuplicateInspectionsWs(workbook.Worksheet("Duplicate Inspections"), reportData.Duplicates, title);
            PopulateDuplicatePostEvaluationsWs(workbook.Worksheet("Duplicate Posting Evaluations"), reportData.Duplicates, title);
            PopulateDuplicatePostStatusesWs(workbook.Worksheet("Duplicate Posting Statuses"), reportData.Duplicates, title);
            PopulateDuplicateSpanSetsWs(workbook.Worksheet("Duplicate Span Sets"), reportData.Duplicates, title);
            PopulateDuplicateSubSetsWs(workbook.Worksheet("Duplicate Substructure Sets"), reportData.Duplicates, title);
            PopulateDuplicateWorkWs(workbook.Worksheet("Duplicate Work"), reportData.Duplicates, title);

            //updateProgress(99);

            // Generate a unique temporary file name
            var tempExcelFileName = Path.Combine(_uploadTempPath, Guid.NewGuid().ToString() + ".xlsx");

            // Save the workbook to a temporary file on disk
            workbook.SaveAs(tempExcelFileName);

            return tempExcelFileName;
        }

        private async Task InsertSubmittalErrorsDB(long submitId, string submittedBy, List<Error> sortedErrors, string userId)
        {
            try
            {
                string submitterEmail = userId;

                // Loop over each error in the sortedErrors list and map to a new SubmittalError entity.
                foreach (var error in sortedErrors)
                {
                    var subError = new SubmittalError
                    {
                        SubmitId = submitId,
                        SubmittedBy = submittedBy,
                        StateCode = error.State.HasValue ? error.State.Value : null,
                        BridgeNo = !string.IsNullOrEmpty(error.BID01) && error.BID01.Length > 100
                                    ? error.BID01.Substring(0, 100)
                                    : error.BID01,
                        Owner = !string.IsNullOrEmpty(error.BCL01) && error.BCL01.Length > 100
                                ? error.BCL01.Substring(0, 100)
                                : error.BCL01,
                        ItemId = error.ItemId,
                        ErrorType = error.ErrorType,
                        ErrorCode = error.ErrorCode ?? string.Empty,
                        ErrorDescription = error.Description,
                        // Validate: SubmittedValue (max 1000 characters)
                        SubmittedValue = !string.IsNullOrEmpty(error.SubmittedValue) && error.SubmittedValue.Length > 1000
                                         ? error.SubmittedValue.Substring(0, 1000)
                                         : error.SubmittedValue,
                        SubmitDate = DateTime.Now,
                        Submitter = submitterEmail,
                        DataSet = error.DataSet
                    };

                    // Add the submittal error to the context.
                    _context.SubmittalErrors.Add(subError);
                }

                // Save all changes to the database.
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertSubmittalErrorsDB");
            }
        }


        private void PopulateProcessingSummaryWs(IXLWorksheet ws, ProcessingReport report, string title)
        {
            ws.Cell(2, 1).Value = title;
            ws.Cell("A1").Value = $"{reportDate}";

            var rowData = new List<object[]>
            {
                new object[] { "Total Bridge Records Uploaded", report.TotalRecordsUploaded },
                new object[] { "Total Bridge Records Omitted Due to Fatal Errors", report.TotalRecordsOmitted },
                new object[] { "Total Safety Errors", report.TotalSafetyErrors },
                new object[] { "Total Critical Errors", report.TotalCriticalErrors },
                new object[] { "Total General Errors", report.TotalGeneralErrors },
                new object[] { "Total Flags", report.TotalFlags },
                new object[] { "Total Duplicate Bridges", report.TotalDuplicateBridges }
            };

            int startRow = 5;
            ws.Cell(startRow, 1).InsertData(rowData);

            int lastRow = startRow + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 2); // From row 5, column 1 to the last row, column 2

            CreateTable(tableRange, "NBI Data Processing Summary");
        }

        private void CreateTable(IXLRange tableRange, string tableName)
        {
            var table = tableRange.CreateTable(tableName);
            table.EmphasizeFirstColumn = true; //First column contains row headers

            table.HeadersRow().Style.Fill.BackgroundColor = XLColor.FromHtml("#366092");
            table.HeadersRow().Style.Font.FontColor = XLColor.White;
            table.Theme = XLTableTheme.None;
        }

        private void PopulateValidationSummaryWs(IXLWorksheet ws, List<ErrorSummary> errors, string title)
        {
            if (errors == null || errors.Count == 0)
                return;

            ws.Cell("A1").Value = $"Report Date: {reportDate}";
            ws.Cell(2, 1).Value = title;

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in errors)
            {
                rowData.Add(
                [
                    idCounter,
                    error.ErrorType.ToString(),
                    error.Frequency,
                    error.ItemId ?? "",
                    error.Description ?? ""
                ]);

                idCounter++;
            }
            ws.Cell(row, 1).InsertData(rowData);

            int lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 5);

            CreateTable(tableRange, "NBI Data Validation Summary");
        }

        private static List<ErrorSummary> SortSummary(List<ErrorSummary> errors)
        {

            int GetErrorTypePriority(string errorType)
            {
                return errorType switch
                {
                    "Safety" => 1,
                    "Critical" => 2,
                    "Error" => 3,
                    "Flag" => 4,
                    _ => int.MaxValue // Handles unexpected values
                };
            }

            // Sorting Code
            List<ErrorSummary> sortedErrors = errors
                .OrderBy(e => GetErrorTypePriority(e.ErrorType))
                .ThenBy(e => e.State)
                .ThenBy(e => e.BID01)
                .ToList();
            return sortedErrors;
        }

        private static List<ProcessingReport.Error> SortErrors(List<ProcessingReport.Error> errors)
        {

            int GetErrorTypePriority(string errorType)
            {
                return errorType switch
                {
                    "Safety" => 1,
                    "Critical" => 2,
                    "Error" => 3,
                    "Flag" => 4,
                    _ => int.MaxValue // Handles unexpected values
                };
            }

            // Sorting Code
            List<ProcessingReport.Error> sortedErrors = errors
                .OrderBy(e => GetErrorTypePriority(e.ErrorType))
                .ThenBy(e => e.State)
                .ThenBy(e => e.BID01)
                .ToList();
            return sortedErrors;
        }

        private void PopulateAllErrorsWs(IXLWorksheet ws, IEnumerable<ProcessingReport.Error> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            // Setup static content
            ws.Cell("A1").Value = $"Report Date: {reportDate}";
            ws.Cell(2, 1).Value = title;

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.State,
                    error.BID01 ?? "",
                    error.BCL01 ?? "",
                    error.ItemId ?? "",
                    error.ItemName ?? "",
                    error.SubmittedValue,
                    error.ErrorType,
                    error.Description ?? ""
                ]);
            }

            ws.Cell(row, 1).InsertData(rowData);

            var lastRow = 5 + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 9);

            CreateTable(tableRange, "Error Summary");
        }

        private List<ErrorSummary> SummarizeErrors(List<ProcessingReport.Error> combinedErrors)
        {
            var errorSummary = combinedErrors
                .GroupBy(e => new { e.ItemId, e.ErrorType, e.Description })
                .Select(group => new ErrorSummary
                {
                    ItemId = group.Key.ItemId,
                    ErrorType = group.Key.ErrorType,
                    Description = group.Key.Description,
                    Frequency = group.Count()
                })
                .OrderBy(summary => summary.ErrorType).ThenBy(summary => summary.ItemId)
                .ToList();

            return errorSummary;
        }

        private void PopulateCriticalErrorSummaryWs(IXLWorksheet ws, IEnumerable<ProcessingReport.Error> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int idCounter = 1;
            int row = 5;
            var rowData = new List<object[]>();

            foreach (var error in errors)
            {
                rowData.Add(
                 [
                    idCounter,
                    error.State,
                    error.BID01 ?? "",
                    error.BCL01 ?? "",
                    error.ItemId ?? "",
                    error.ItemName ?? "",
                    error.SubmittedValue,
                    error.ErrorType,
                    error.Description ?? ""
                 ]);

                idCounter++;
            }

            ws.Cell(row, 1).InsertData(rowData);

            var lastRow = row + rowData.Count - 1;

            var tableRange = ws.Range(4, 1, lastRow, 9);

            CreateTable(tableRange, "Critical Error Summary");
         
            //ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        }

        private void PopulateSafetyWs(IXLWorksheet ws, IEnumerable<ProcessingReport.Error> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            // Setup the header of the worksheet
            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            // Populate data starting from the fifth row
            int row = 5;
            int idCounter = 1;
            var rowData = new List<object[]>();

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.State,
                    error.BID01,
                    error.BCL01,
                    error.SubmittedValue,
                    error.ValidationType,
                    error.Description
                ]);
            }

            ws.Cell(5, 1).InsertData(rowData);

            int lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 7); 

            CreateTable(tableRange, "Safety Validation for Closed and Posted Bridges");
        }

        private void PopulateNonNBISBridgesWs(IXLWorksheet ws, List<NonNBIBridge> nonNBIBridges, string title)
        {
            if (nonNBIBridges == null || !nonNBIBridges.Any())
            {
                ws.Delete();
                return;
            }

            // Setup the header of the worksheet
            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Populate data starting from the fifth row
            int row = 5;
            int idCounter = 1;
            var rowData = new List<object[]>();

            foreach (var record in nonNBIBridges)
            {
                rowData.Add(
                [
                    idCounter++,
                    record.BL01,
                    record.BID01,
                    record.BCL01,
                    record.BG01,
                    record.BG02
                ]);
            }

            ws.Cell(5, 1).InsertData(rowData);

            int lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 6);

            CreateTable(tableRange, "Non-NBIS Length Bridges");
            
        }

        private void CreateTemporaryCountSheet(IXLWorksheet ws, string title)
        {
            if (TemporaryCounts.AllTemporaryCountsZero())
            {
                ws.Delete();
                return;
            }

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Prepare data for temporary counts
            var tempCounts = new SortedDictionary<string, int>
            {
                { "BAP02", TemporaryCounts.BAP02Count },
                { "BAP03", TemporaryCounts.BAP03Count },
                { "BC11", TemporaryCounts.BC11Count },
                { "BCL04", TemporaryCounts.BCL04Count },
                { "BH04", TemporaryCounts.BH04Count },
                { "BIE12", TemporaryCounts.BIE12Count },
                { "BRH01", TemporaryCounts.BRH01Count },
                { "BRH02", TemporaryCounts.BRH02Count },
                { "BRT03", TemporaryCounts.BRT03Count },
                { "BPS01", TemporaryCounts.BPS01Count },
                { "BSB03", TemporaryCounts.BSB03Count },
                { "BSB04", TemporaryCounts.BSB04Count },
                { "BSB05", TemporaryCounts.BSB05Count },
                { "BSB06", TemporaryCounts.BSB06Count },
                { "BSB07", TemporaryCounts.BSB07Count },
                { "BN06", TemporaryCounts.BN06Count },
                { "BSP04", TemporaryCounts.BSP04Count },
                { "BSP05", TemporaryCounts.BSP05Count },
                { "BSP06", TemporaryCounts.BSP06Count },
                { "BSP09", TemporaryCounts.BSP09Count },
                { "BSP10", TemporaryCounts.BSP10Count },
                { "BSP11", TemporaryCounts.BSP11Count },
                { "BSP12", TemporaryCounts.BSP12Count },
                { "BW03", TemporaryCounts.BW03Count }
            };

            // Populate data starting from the fifth row
            var rowData = new List<object[]>();
            //int idCounter = 1;

            foreach (var tempCount in tempCounts)
            {
                if (tempCount.Value > 0)
                {
                    rowData.Add(
                      [
                           $"{tempCount.Key}",
                           $" {tempCount.Value}"
                      ]);
                }
            }

            if (rowData.Count > 0)
            {
                ws.Cell(5, 1).InsertData(rowData);

                var lastRow = 5 + rowData.Count - 1;

                var tableRange = ws.Range(4, 1, lastRow, 2);

                ws.Column(2).Style.NumberFormat.Format = "#,##0";

                CreateTable(tableRange, "Total Number of Items with Temporary Codes");
            }

        }

        private void PopulateToBeRemovedWs(IXLWorksheet ws, List<SNBIRecord> stagingData, string title)
        {
            // Setup static content
            ws.Cell("A1").Value = $"Report Date: {reportDate}";
            ws.Cell("A2").Value = title;

            int currentRow = 4;

            // --- SECTION 2: Omitted Feature and Route Records ---
            currentRow = PopulateOmittedFeatures(ws, stagingData, currentRow);
            currentRow = PopulateOmittedRoutes(ws, stagingData, currentRow);

            // --- SECTION 3: Omitted Span Sets ---
            currentRow = PopulateOmittedSpanSets(ws, stagingData, currentRow);

            // --- SECTION 4: Omitted Substructure Sets ---
            currentRow = PopulateOmittedSubstructureSets(ws, stagingData, currentRow);

            // --- SECTION 5: Omitted Posting Statuses ---
            currentRow = PopulateOmittedPostingStatuses(ws, stagingData, currentRow);

            // --- SECTION 6: Omitted Posting Evaluations ---
            currentRow = PopulateOmittedPostingEvaluations(ws, stagingData, currentRow);

            // --- SECTION 7: Omitted Inspection Records ---
            currentRow = PopulateOmittedInspectionRecords(ws, stagingData, currentRow);

            // --- SECTION 8: Omitted Element Records ---
            currentRow = PopulateOmittedElementRecords(ws, stagingData, currentRow);

            // --- SECTION 9: Omitted Work Records ---
            currentRow = PopulateOmittedWorkRecords(ws, stagingData, currentRow);
        }

        private int PopulateOmittedRoutes(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Compute the removed route records.
            var removedRoutes = stagingData
                .SelectMany(r => r.Features ?? Enumerable.Empty<SNBIRecord.Feature>())
                .SelectMany(f => f.Routes ?? Enumerable.Empty<SNBIRecord.Route>())
                .Where(rt => rt.RecordStatus == "Removed")
                .ToList();

            // If no removed routes exist, then exit.
            if (!removedRoutes.Any())
                return currentRow;

            StyleSectionTitle(ws, currentRow, 1, 5, "Omitted Routes");
            currentRow++;

            // Define headers: description first then item ID on a new line.
            var headers = new string[]
            {
                "Route Designation\r\nB.RT.01",
                "Route Number\r\nB.RT.02",
                "Route Direction\r\nB.RT.03",
                "Route Type\r\nB.RT.04",
                "Service Type\r\nB.RT.05"
            };

            // Style the header columns using the reusable method.
            StyleHeaderColumns(ws, currentRow, headers, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var routeData = new List<object[]>();
            foreach (var route in removedRoutes)
            {
                routeData.Add(new object[]
                {
            route.BRT01,  // Column 1: Route Designation
            route.BRT02,  // Column 2: Route Number
            route.BRT03,  // Column 3: Route Direction
            route.BRT04,  // Column 4: Route Type
            route.BRT05   // Column 5: Service Type
                });
            }

            // Insert the data in bulk and update the row counter.
            if (routeData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(routeData);
                currentRow += routeData.Count;
            }

            // Add spacing after this section.
            currentRow += 2;
            return currentRow;
        }      

        private int PopulateOmittedInspectionRecords(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            var removedInspections = stagingData
                .SelectMany(r => r.Inspections ?? Enumerable.Empty<SNBIRecord.Inspection>())
                .Where(i => i.RecordStatus == "Removed")
                .ToList();

            // If no removed inspection records exist, exit.
            if (!removedInspections.Any())
                return currentRow;

            StyleSectionTitle(ws, currentRow, 1, 10, "Omitted Inspection Records");
            currentRow++;

            var headers = new string[]
            {
        "Inspection Type\r\nB.IE.01",
        "Inspection Begin Date\r\nB.IE.02",
        "Inspection Completion Date\r\nB.IE.03",
        "Nationally Certified Bridge Inspector\r\nB.IE.04",
        "Inspection Interval\r\nB.IE.05",
        "Inspection Due Date\r\nB.IE.06",
        "Risk-Based Inspection Interval Method\r\nB.IE.07",
        "Inspection Quality Control Date\r\nB.IE.08",
        "Inspection Quality Assurance Date\r\nB.IE.09",
        "Inspection Data Update Date\r\nB.IE.10",
        "Inspection Note\r\nB.IE.11",
        "Inspection Equipment\r\nB.IE.12"
            };

            // Style header columns using the reusable method.
            StyleHeaderColumns(ws, currentRow, headers, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var inspectionData = new List<object[]>();
            foreach (var insp in removedInspections)
            {
                inspectionData.Add(new object[]
                {
                    insp.BIE01, // Inspection Type
                    insp.BIE02, // Inspection Begin Date
                    insp.BIE03, // Inspection Completion Date
                    insp.BIE04, // Nationally Certified Bridge Inspector
                    insp.BIE05, // Inspection Interval
                    insp.BIE06, // Inspection Due Date
                    insp.BIE07, // Risk-Based Inspection Interval Method
                    insp.BIE08, // Inspection Quality Control Date
                    insp.BIE09, // Inspection Quality Assurance Date
                    insp.BIE10, // Inspection Data Update Date
                    insp.BIE11, // Inspection Note
                    insp.BIE12  // Inspection Equipment
                });
            }

            // Insert the data in bulk and update the row counter.
            if (inspectionData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(inspectionData);
                currentRow += inspectionData.Count;
            }

            // Add spacing after this section.
            currentRow += 2;
            return currentRow;
        }


        private int PopulateOmittedFeatures(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Compute the removed feature records.
            var removedFeatures = stagingData
                .SelectMany(r => r.Features ?? Enumerable.Empty<SNBIRecord.Feature>())
                .Where(f => f.RecordStatus == "Removed")
                .ToList();

            // If there are no removed features then exit.
            if (!removedFeatures.Any())
                return currentRow;

            StyleSectionTitle(ws, currentRow, 1, 10, "Omitted Feature Records");
            currentRow++;

            var featureHeaders = new string[]
            {
                "State\r\nB.L.01",
                "Bridge Number\r\nB.ID.01",
                "Feature Type\r\nB.F.01",
                "Feature Location\r\nB.F.02",
                "Feature Name\r\nB.F.03",
                "Functional Classification\r\nB.H.01",
                "Urban Code\r\nB.H.02",
                "NHS Designation\r\nB.H.03",
                "National Highway Freight Network\r\nB.H.04",
                "STRAHNET Designation\r\nB.H.05",
                "LRS Route ID\r\nB.H.06",
                "LRS Mile Point\r\nB.H.07",
                "Lanes on Highway\r\nB.H.08",
                "Annual Average Daily Traffic\r\nB.H.09",
                "Annual Average Daily Truck Traffic\r\nB.H.10",
                "Year of AADT\r\nB.H.11",
                "Highway Max Usable Vertical Clearance\r\nB.H.12",
                "Highway Min Vertical Clearance\r\nB.H.13",
                "Highway Min Horizontal Clearance, Left\r\nB.H.14",
                "Highway Min Horizontal Clearance, Right\r\nB.H.15",
                "Highway Max Usable Surface Width\r\nB.H.16",
                "Bypass Detour Length\r\nB.H.17",
                "Crossing Bridge Number\r\nB.H.18",
                "Railroad Service Type\r\nB.RR.01",
                "Railroad Min Vertical Clearance\r\nB.RR.02",
                "Railroad Min Horizontal Offset\r\nB.RR.03",
                "Navigable Waterway\r\nB.N.01",
                "Navigation Min Vertical Clearance\r\nB.N.02",
                "Movable Bridge Max Navigation Vertical Clearance\r\nB.N.03",
                "Navigation Channel Width\r\nB.N.04",
                "Navigation Channel Min Horizontal Clearance\r\nB.N.05",
                "Substructure Navigation Protection\r\nB.N.06"
            };


            StyleHeaderColumns(ws, currentRow, featureHeaders, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var featureData = new List<object[]>();
            foreach (var feature in removedFeatures)
            {
                featureData.Add(new object[]
                {
            feature.BL01,      // Column 1: State
            feature.BID01,     // Column 2: Bridge Number
            feature.BF01,      // Column 3: Feature Type
            feature.BF02,      // Column 4: Feature Location
            feature.BF03,      // Column 5: Feature Name
            feature.BH01,      // Column 6: Functional Classification
            feature.BH02,      // Column 7: Urban Code
            feature.BH03,      // Column 8: NHS Designation
            feature.BH04,      // Column 9: National Highway Freight Network
            feature.BH05,      // Column 10: STRAHNET Designation
            feature.BH06,      // Column 11: LRS Route ID
            feature.BH07,      // Column 12: LRS Mile Point
            feature.BH08,      // Column 13: Lanes on Highway
            feature.BH09,      // Column 14: Annual Average Daily Traffic
            feature.BH10,      // Column 15: Annual Average Daily Truck Traffic
            feature.BH11,      // Column 16: Year of AADT
            feature.BH12,      // Column 17: Highway Max Usable Vertical Clearance
            feature.BH13,      // Column 18: Highway Min Vertical Clearance
            feature.BH14,      // Column 19: Highway Min Horizontal Clearance, Left
            feature.BH15,      // Column 20: Highway Min Horizontal Clearance, Right
            feature.BH16,      // Column 21: Highway Max Usable Surface Width
            feature.BH17,      // Column 22: Bypass Detour Length
            feature.BH18,      // Column 23: Crossing Bridge Number
            feature.BRR01,     // Column 24: Railroad Service Type
            feature.BRR02,     // Column 25: Railroad Min Vertical Clearance
            feature.BRR03,     // Column 26: Railroad Min Horizontal Offset
            feature.BN01,      // Column 27: Navigable Waterway
            feature.BN02,      // Column 28: Navigation Min Vertical Clearance
            feature.BN03,      // Column 29: Movable Bridge Max Navigation Vertical Clearance
            feature.BN04,      // Column 30: Navigation Channel Width
            feature.BN05,      // Column 31: Navigation Channel Min Horizontal Clearance
            feature.BN06       // Column 32: Substructure Navigation Protection
                });
            }

            // Insert the data in bulk and update the row counter.
            if (featureData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(featureData);
                currentRow += featureData.Count;
            }

            // Add spacing after this section
            currentRow += 2;
            return currentRow;
        }

      
        private int PopulateOmittedSpanSets(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Compute the removed span sets
            var removedSpanSets = stagingData
                .SelectMany(r => r.SpanSets ?? Enumerable.Empty<SNBIRecord.SpanSet>())
                .Where(s => s.RecordStatus == "Removed")
                .ToList();

            if (!removedSpanSets.Any())
                return currentRow;  // No data, so no section is added

            // --- SECTION: Omitted Span Sets ---
            // Style and insert section title over columns 1 to 10
            StyleSectionTitle(ws, currentRow, 1, 10, "Omitted Span Sets");
            currentRow++;

            // Define headers for the section.
            var headers = new string[]
            {
        "State\r\nB.L.01",
        "Bridge Number\r\nB.ID.01",
        "Span Config\r\nB.SP.01",
        "Number of Spans\r\nB.SP.02",
        "Number of Beam Lines\r\nB.SP.03",
        "Span Material\r\nB.SP.04)",
        "Span Continuity\r\nB.SP.05",
        "Span Type\r\nB.SP.06",
        "Span Protective System\r\nB.SP.07",
        "Deck Interaction\r\nB.SP.08",
        "Deck Material and Type\r\nB.SP.09",
        "Wearing Surface\r\nB.SP.10",
        "Deck Protective System\r\nB.SP.11",
        "Deck Reinforcing Protective System\r\nB.SP.12",
        "Deck Stay-In-Place Forms\r\nB.SP.13"
            };

            // Style header columns using the reusable method.
            StyleHeaderColumns(ws, currentRow, headers, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var spanData = new List<object[]>();
            foreach (var span in removedSpanSets)
            {
                spanData.Add(new object[]
                {
            span.BL01,      // Column 1: State
            span.BID01,     // Column 2: Bridge Number
            span.BSP01,     // Column 3: Span Config
            span.BSP02,     // Column 4: Number of Spans
            span.BSP03,     // Column 5: Number of Beam Lines
            span.BSP04,     // Column 6: Span Material
            span.BSP05,     // Column 7: Span Continuity
            span.BSP06,     // Column 8: Span Type
            span.BSP07,     // Column 9: Span Protective System
            span.BSP08,     // Column 10: Deck Interaction
            span.BSP09,     // Column 11: Deck Material and Type
            span.BSP10,     // Column 12: Wearing Surface
            span.BSP11,     // Column 13: Deck Protective System
            span.BSP12,     // Column 14: Deck Reinforcing Protective System
            span.BSP13      // Column 15: Deck Stay-In-Place Forms
                });
            }

            // Insert the data in bulk and update the row counter.
            if (spanData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(spanData);
                currentRow += spanData.Count;
            }

            // Add spacing after this section
            currentRow += 2;
            return currentRow;
        }

        private int PopulateOmittedSubstructureSets(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Gather the removed substructure set records.
            var removedSubs = stagingData
                .SelectMany(r => r.SubstructureSets ?? Enumerable.Empty<SNBIRecord.SubstructureSet>())
                .Where(s => s.RecordStatus == "Removed")
                .ToList();

            // If no data exists, return the current row without adding the section.
            if (!removedSubs.Any())
                return currentRow;

            // --- SECTION: Omitted Substructure Sets ---
            // Insert the section title over columns 1 to 10.
            StyleSectionTitle(ws, currentRow, 1, 10, "Omitted Substructure Sets");
            currentRow++;

            // Define headers for substructure sets.
            var headers = new string[]
            {
        "State\r\nB.L.01",
        "Bridge Number\r\nB.ID.01",
        "Substructure Config\r\nB.SB.01",
        "Number of Substructure Units\r\nB.SB.02",
        "Substructure Material\r\nB.SB.03",
        "Substructure Type\r\nB.SB.04",
        "Substructure Protective System\r\nB.SB.05",
        "Foundation Type\r\nB.SB.06",
        "Foundation Protective System\r\nB.SB.07"
            };

            // Style the header columns using the reusable method.
            StyleHeaderColumns(ws, currentRow, headers, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var subData = new List<object[]>();
            foreach (var sub in removedSubs)
            {
                subData.Add(new object[]
                {
            sub.BL01,     // Column 1: State
            sub.BID01,    // Column 2: Bridge Number
            sub.BSB01,    // Column 3: Substructure Config
            sub.BSB02,    // Column 4: Number of Substructure Units
            sub.BSB03,    // Column 5: Substructure Material
            sub.BSB04,    // Column 6: Substructure Type
            sub.BSB05,    // Column 7: Substructure Protective System
            sub.BSB06,    // Column 8: Foundation Type
            sub.BSB07     // Column 9: Foundation Protective System
                });
            }

            // Bulk insert the data and update the row counter.
            ws.Cell(currentRow, 1).InsertData(subData);
            currentRow += subData.Count;

            // Add spacing after this section.
            currentRow += 2;
            return currentRow;
        }


        private int PopulateOmittedPostingStatuses(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Gather removed posting statuses.
            var removedPostingStatuses = stagingData
                .SelectMany(r => r.PostingStatuses ?? Enumerable.Empty<SNBIRecord.PostingStatus>())
                .Where(ps => ps.RecordStatus == "Removed")
                .ToList();

            // If no data exists, return the current row.
            if (!removedPostingStatuses.Any())
                return currentRow;  // No data, so no section is added

            // --- SECTION: Omitted Posting Statuses ---
            // Insert section title over columns 1 to 10
            StyleSectionTitle(ws, currentRow, 1, 10, "Omitted Posting Statuses");
            currentRow++;

            // Define headers for posting statuses.
            var headers = new string[]
            {
                "State\r\nB.L.01",
                "Bridge Number\r\nB.ID.01",
                "Load Posting Status\r\nB.PS.01",
                "Posting Status Change Date\r\nB.PS.02"
            };

            // Style the header row using the reusable method.
            StyleHeaderColumns(ws, currentRow, headers, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var psData = new List<object[]>();
            foreach (var ps in removedPostingStatuses)
            {
                psData.Add(new object[]
                {
            ps.BL01,    // Column 1: State (B.L.01)
            ps.BID01,   // Column 2: Bridge Number (B.ID.01)
            ps.BPS01,   // Column 3: Load Posting Status (B.PS.01)
            ps.BPS02    // Column 4: Posting Status Change Date (B.PS.02)
                });
            }

            // Insert the data in bulk and update the row counter.
            if (psData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(psData);
                currentRow += psData.Count;
            }

            // Add spacing after the section.
            currentRow += 2;
            return currentRow;
        }

        private int PopulateOmittedPostingEvaluations(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Compute the removed posting evaluations.
            var removedPostingEvals = stagingData
                .SelectMany(r => r.PostingEvaluations ?? Enumerable.Empty<SNBIRecord.PostingEvaluation>())
                .Where(pe => pe.RecordStatus == "Removed")
                .ToList();

            if (!removedPostingEvals.Any())
                return currentRow;

            StyleSectionTitle(ws, currentRow, 1, 4, "Omitted Posting Evaluations");
            currentRow++;

            var postingEvalHeaders = new string[]
            {
                "Legal Load Configuration\r\nB.EP.01",
                "Legal Load Rating Factor\r\nB.EP.02",
                "Posting Type\r\nB.EP.03",
                "Posting Value\r\nB.EP.04"
            };

            // Style header columns using the reusable method.
            StyleHeaderColumns(ws, currentRow, postingEvalHeaders, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var postingEvalData = new List<object[]>();
            foreach (var eval in removedPostingEvals)
            {
                postingEvalData.Add(new object[]
                {
                    eval.BEP01,   // Column 1: Legal Load Configuration
                    eval.BEP03,   // Column 2: Posting Type
                    eval.BEP04,   // Column 3: Posting Value
                    eval.BEP02    // Column 4: Legal Load Rating Factor
                });
            }

            // Insert the data in bulk and update the row counter.
            if (postingEvalData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(postingEvalData);
                currentRow += postingEvalData.Count;
            }

            // Add spacing after this section
            currentRow += 2;
            return currentRow;
        }

        private int PopulateOmittedElementRecords(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Compute the removed element records.
            var removedElements = stagingData
                .SelectMany(r => r.Elements ?? Enumerable.Empty<SNBIRecord.Element>())
                .Where(el => el.RecordStatus == "Removed")
                .ToList();

            if (!removedElements.Any())
                return currentRow;

            StyleSectionTitle(ws, currentRow, 1, 10, "Omitted Element Records");
            currentRow++;

            var headers = new string[]
            {
                "Element Number\r\nB.E.01",
                "Element Parent Number\r\nB.E.02",
                "Element Total Quantity\r\nB.E.03",
                "Element Quantity Condition State One\r\nB.CS.01",
                "Element Quantity Condition State Two\r\nB.CS.02",
                "Element Quantity Condition State Three\r\nB.CS.03",
                "Element Quantity Condition State Four\r\nB.CS.04"       
            };

            // Style header columns using the reusable method.
            StyleHeaderColumns(ws, currentRow, headers, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var elementData = new List<object[]>();
            foreach (var el in removedElements)
            {
                elementData.Add(new object[]
                {
                    el.BE01,  // Element Number
                    el.BE02,  // Element Parent Number
                    el.BE03,  // Element Total Quantity
                    el.BCS01, // Element Quantity Condition State One
                    el.BCS02, // Element Quantity Condition State Two
                    el.BCS03, // Element Quantity Condition State Three
                    el.BCS04 // Element Quantity Condition State Four          
                });
            }

            // Insert the data in bulk and update the row counter.
            if (elementData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(elementData);
                currentRow += elementData.Count;
            }

            // Add spacing after this section.
            currentRow += 2;
            return currentRow;
        }

        private int PopulateOmittedWorkRecords(IXLWorksheet ws, List<SNBIRecord> stagingData, int currentRow)
        {
            // Compute the removed work records.
            var removedWorks = stagingData
                .SelectMany(r => r.Works ?? Enumerable.Empty<SNBIRecord.Work>())
                .Where(w => w.RecordStatus == "Removed")
                .ToList();

            if (!removedWorks.Any())
                return currentRow;

            StyleSectionTitle(ws, currentRow, 1, 10, "Omitted Work Records");
            currentRow++;

            var headers = new string[]
            {
                "Year Work Performed\r\nB.W.02",
                "Work Performed\r\nB.W.03"
            };

            StyleHeaderColumns(ws, currentRow, headers, startColumn: 1, rowHeight: 45);
            currentRow++;

            // Build the data for bulk insertion.
            var workData = new List<object[]>();
            foreach (var work in removedWorks)
            {
                workData.Add(new object[]
                {
                    work.BW02,  // Year Work Performed
                    work.BW03   // Work Performed
                });
            }

            // Insert the data in bulk and update the row counter.
            if (workData.Any())
            {
                ws.Cell(currentRow, 1).InsertData(workData);
                currentRow += workData.Count;
            }

            // Add spacing after this section.
            currentRow += 2;
            return currentRow;
        }

        private void StyleSectionTitle(IXLWorksheet ws, int row, int startColumn, int endColumn, string headerText)
        {
            var headerRange = ws.Range(ws.Cell(row, startColumn), ws.Cell(row, endColumn));
            headerRange.Merge();
            headerRange.Value = headerText;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private void StyleHeaderColumns(IXLWorksheet ws, int headerRow, IEnumerable<string> headers, int startColumn = 1, int rowHeight = 45)
        {
            int col = startColumn;
            foreach (var header in headers)
            {
                var cell = ws.Cell(headerRow, col);
                cell.Value = header;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#366092");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.LeftBorderColor = XLColor.White;
                cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.RightBorderColor = XLColor.White;
                col++;
            }
            ws.Row(headerRow).Height = rowHeight;
        }

        private void PopulatePrimaryWs(IXLWorksheet ws, IEnumerable<ProcessingReport.Error> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            // Setup static content
            ws.Cell("A1").Value = $"Report Date: {reportDate}";
            ws.Cell(2, 1).Value = title;

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            // Prepare the data to be inserted in bulk
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.State,
                    error.BID01,
                    error.BCL01,
                    error.ItemId,
                    error.ItemName,
                    error.SubmittedValue,
                    error.ErrorType,
                    error.Description
                ]);
            }

            ws.Cell(5, 1).InsertData(rowData);

            // Apply styles for entire columns outside the loop
            //ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = 5 + rowData.Count - 1; // Calculate the last row with data
            var tableRange = ws.Range(4, 1, lastRow, 9);

            CreateTable(tableRange, "Primary Errors");
        }

        private void PopulateElementWs(IXLWorksheet ws, IEnumerable<ElementError> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            // Setup static content
            ws.Cell("A1").Value = $"Report Date: {reportDate}";
            ws.Cell(2, 2).Value = title;

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            int idCounter = 1;
            var rowData = new List<object[]>();

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.ElementDetails.BE01 ?? "",
                    error.ElementDetails.BE02 ?? "",
                    error.ElementDetails.BE03,
                    error.ElementDetails.BCS01,
                    error.ElementDetails.BCS02,
                    error.ElementDetails.BCS03,
                    error.ElementDetails.BCS04,
                    error.ErrorDetails.ErrorType,
                    error.ErrorDetails.Description ?? ""
                ]);
            }

            ws.Cell(5, 1).InsertData(rowData);

            // Apply styles for entire columns outside the loop
            //ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //ws.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //ws.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //ws.Column(10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //ws.Column(11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = 5 + rowData.Count - 1; // Calculate the last row with data
            var tableRange = ws.Range(4, 1, lastRow, 13);

            CreateTable(tableRange, "Element Errors");
           
        }

        private void PopulateFeatureWs(IXLWorksheet ws, IEnumerable<FeatureError> errors, List<RouteError> routeErrors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            // Setup static content
            ws.Cell("A1").Value = $"Report Date: {reportDate}";
            ws.Cell(2, 1).Value = title;

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            // Populate data starting from the fifth row
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.FeatureDetails.BF01 ?? "",
                    error.FeatureDetails.BF02 ?? "",
                    string.Empty,
                    error.ErrorDetails.ItemId ?? "",
                    error.ErrorDetails.ItemName ?? "",
                    error.ErrorDetails.SubmittedValue ?? "",
                    error.ErrorDetails.ErrorType,
                    error.ErrorDetails.Description ?? ""
                ]);
            }

            foreach (var error in routeErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.RouteDetails.BF01 ?? "",
                    error.RouteDetails.BF02 ?? "",
                    error.RouteDetails.BRT01 ?? "",
                    error.ErrorDetails.ItemId ?? "",
                    error.ErrorDetails.ItemName ?? "",
                    error.ErrorDetails.SubmittedValue ?? "",
                    error.ErrorDetails.ErrorType,
                    error.ErrorDetails.Description ?? ""
                ]);
            }
            ws.Cell(5, 1).InsertData(rowData);

            // Apply styles for entire columns outside the loop
            ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //Item ID
            ws.Column(10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = 5 + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 12);

            CreateTable(tableRange, "Feature and Route Errors");
           
        }

        private void PopulateInspectionWs(IXLWorksheet ws, IEnumerable<InspectionError> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            // Populate data starting from the fifth row
            int row = 5;
            int idCounter = 1;
            var rowData = new List<object[]>();

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.InspectionDetails.BIE01 ?? "",
                    error.InspectionDetails.BIE02 ?? "",
                    error.ErrorDetails.ItemId ?? "",
                    error.ErrorDetails.ItemName ?? "",
                    error.ErrorDetails.SubmittedValue ?? "",
                    error.ErrorDetails.ErrorType ?? "",
                    error.ErrorDetails.Description ?? ""
                 ]);
            }

            ws.Cell(row, 1).InsertData(rowData);

            // Apply styles for entire columns outside the loop
            ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = row + rowData.Count - 1; // Calculate the last row with data
            var tableRange = ws.Range(4, 1, lastRow, 11);

            CreateTable(tableRange, "Inspection Errors");

        }

        private void PopulateSpanSetWs(IXLWorksheet ws, IEnumerable<SpanSetError> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            int row = 5;
            int idCounter = 1;
            var rowData = new List<object[]>();

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.SpanSetDetails.BSP01 ?? "",
                    error.ErrorDetails.ItemId ?? "",
                    error.ErrorDetails.ItemName ?? "",
                    error.ErrorDetails.SubmittedValue ?? "",
                    error.ErrorDetails.ErrorType ?? "",
                    error.ErrorDetails.Description ?? ""
                ]);
            }

            ws.Cell(row, 1).InsertData(rowData);

            ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 10);

            CreateTable(tableRange, "Span Set Errors");

        }

        private void PopulatePostingStatusWs(IXLWorksheet ws, IEnumerable<PostingStatusError> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.PostingStatusDetails.BPS01 ?? "",
                    error.PostingStatusDetails.BPS02,
                    error.ErrorDetails.ErrorType,
                    error.ErrorDetails.Description ?? ""
                ]);
            }

            ws.Cell(row, 1).InsertData(rowData);

            ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 7);

            CreateTable(tableRange, "Posting Status Errors");

        }

        private void PopulatePostingEvaluationsWs(IXLWorksheet ws, IEnumerable<PostingEvaluationsError> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01,
                    error.ErrorDetails.BCL01,
                    error.PostingEvaluationsDetails.BEP01,
                    error.PostingEvaluationsDetails.BEP02,
                    error.PostingEvaluationsDetails.BEP03,
                    error.PostingEvaluationsDetails.BEP04,
                    error.ErrorDetails.ErrorType,
                    error.ErrorDetails.Description
                ]);
            }

            ws.Cell(row, 1).InsertData(rowData);

            ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 9);

            CreateTable(tableRange, "Posting Evaluation Errors");

        }

        private void PopulateSubstructureSetWs(IXLWorksheet ws, IEnumerable<SubstructureSetError> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in limitedErrors)
            {
                rowData.Add(
                [
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.SubstructureSetDetails.BSB01 ?? "",
                    error.ErrorDetails.ItemId ?? "",
                    error.ErrorDetails.ItemName ?? "",
                    error.ErrorDetails.SubmittedValue ?? "",
                    error.ErrorDetails.ErrorType ?? "",
                    error.ErrorDetails.Description ?? ""
                ]);
            }

            ws.Cell(row, 1).InsertData(rowData);

            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Column(9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 10);

            CreateTable(tableRange, "Substructure Set Errors");
        }

        private void PopulateWorkWs(IXLWorksheet ws, IEnumerable<WorkError> errors, string title)
        {
            if (errors == null || !errors.Any())
            {
                ws.Delete();
                return;
            }

            // Setup static content
            ws.Cell("A1").Value = $"Report Date: {reportDate}";
            ws.Cell(2, 1).Value = title;

            // Apply styles for entire columns outside the loop
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Limit the number of errors to 1,000,000
            const int maxErrorLimit = 1_000_000;
            var limitedErrors = errors.Take(maxErrorLimit);

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var error in limitedErrors)
            {
                rowData.Add(new object[]
                {
                    idCounter++,
                    error.ErrorDetails.State,
                    error.ErrorDetails.BID01 ?? "",
                    error.ErrorDetails.BCL01 ?? "",
                    error.WorkDetails.BW01,
                    error.WorkDetails.BW02,
                    error.WorkDetails.BW03,
                    error.ErrorDetails.ErrorType.ToString(),
                    error.ErrorDetails.Description ?? ""
                });

            }

            ws.Cell(row, 1).InsertData(rowData);

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 9);

            CreateTable(tableRange, "Work Errors");
        }

        //Duplicates
        private void PopulateDuplicateWorkWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || !duplicates.Any())
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.Work))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.Work; // Safe cast to the appropriate type
                        if (record != null)
                        {
                            hasEntries = true;

                            rowData.Add(
                            [
                                idCounter++,
                                record.BL01,
                                record.BID01,
                                record.BCL01,
                                record.BW02,
                                record.BW03
                            ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);


            ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 6);

            CreateTable(tableRange, "Duplicate Work Records");

        }

        private void PopulateDuplicateSubSetsWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || !duplicates.Any())
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.SubstructureSet))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.SubstructureSet; // Safe cast to the appropriate type
                        if (record != null)
                        {
                            hasEntries = true;
                            rowData.Add(
                           [
                                idCounter++,
                                record.BL01,
                                record.BID01 ?? "",
                                record.BCL01 ?? "",
                                record.BSB01 ?? ""
                                //record.BSB02,
                                //record.BSB03 ?? "",
                                //record.BSB04 ?? "",
                                //record.BSB05 ?? "",
                                //record.BSB06 ?? "",
                                //record.BSB07 ?? ""
                           ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);
            var lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 5);

            CreateTable(tableRange, "Duplicate Substructure Sets");

        }

        private void PopulateDuplicateSpanSetsWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.SpanSet))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.SpanSet; // Safe cast to the appropriate type
                        if (record != null)
                        {
                            hasEntries = true;
                            rowData.Add(
                            [
                                idCounter++,
                                record.BL01,
                                record.BID01 ?? "",
                                record.BCL01 ?? "",
                                record.BSP01 ?? "",
                                record.BSP02,
                                record.BSP03,
                                record.BSP04 ?? "",
                                record.BSP05 ?? "",
                                record.BSP06 ?? ""
                            ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 10);
            CreateTable(tableRange, "Duplicate Span Sets");

        }

        private void PopulateDuplicatePostStatusesWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.PostingStatus))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.PostingStatus; // Safe cast to the appropriate type
                        if (record != null)
                        {
                            hasEntries = true;

                            rowData.Add(
                            [
                                idCounter++,
                                record.BL01,
                                record.BID01 ?? "",
                                record.BCL01 ?? "",
                                record.BPS01 ?? "",
                                record.BPS02 ?? ""
                            ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 6);

            CreateTable(tableRange, "Duplicate Posting Statuses");

        }

        private void PopulateDuplicatePostEvaluationsWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.PostingEvaluation))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.PostingEvaluation; // Safe cast to the appropriate type
                        if (record != null)
                        {
                            hasEntries = true;

                            rowData.Add(
                        [
                            idCounter++,
                            record.BL01,
                            record.BID01 ?? "",
                            record.BCL01 ?? "",
                            record.BEP01,
                            record.BEP02,
                            record.BEP03,
                            record.BEP04
                        ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);

            var lastRow = row + rowData.Count - 1; // Calculate the last row of data
            var tableRange = ws.Range(4, 1, lastRow, 8);

            CreateTable(tableRange, "Duplicate Posting Evaluations");

        }

        private void PopulateDuplicateInspectionsWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.Inspection))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.Inspection; // Safe cast to the appropriate type
                        if (record != null)
                        {
                            hasEntries = true;

                            rowData.Add(
                        [
                            idCounter++,
                            record.BL01,
                            record.BID01 ?? "",
                            record.BCL01 ?? "",
                            record.BIE01 ?? "",
                            record.BIE02 ?? "",
                            record.BIE05
                        ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);
            var lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 7);

            CreateTable(tableRange, "Duplicate Inspections");

        }

        private void PopulateDuplicateRoutesWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            // Setup the header of the worksheet
            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            // Routes
            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.Route))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.Route;
                        if (record != null)
                        {
                            hasEntries = true;

                            rowData.Add(
                            [
                                idCounter++,
                                record.BL01,
                                record.BID01 ?? "",
                                record.BCL01 ?? "",
                                record.BF01 ?? "",
                                record.BRT01 ?? "",
                                record.BRT02 ?? "",
                                record.BRT03 ?? "",
                                record.BRT04 ?? "",
                                record.BRT05 ?? ""
                            ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);
            var lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 10);

            CreateTable(tableRange, "Duplicate Routes");

        }

        private void PopulateDuplicateFeatureWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            // Setup the header of the worksheet
            SetupWorksheetHeader(ws, title);
            //Add Date
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            // Features - Iterate through all KeyValuePair entries
            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.Feature))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.Feature; // Safe cast to the appropriate type
                        if (record != null)
                        {
                            hasEntries = true;

                            rowData.Add(
                            [
                                idCounter++,
                                record.BL01,
                                record.BID01 ?? "",
                                record.BCL01 ?? "",
                                record.BF01 ?? "",
                                record.BF02 ?? "",
                                record.BF03 ?? ""
                            ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);
            var lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 7);

            CreateTable(tableRange, "Duplicate Features");

        }

        private void PopulateDuplicateElementWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            // Setup the header of the worksheet
            SetupWorksheetHeader(ws, title);
            //Add Date
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            // Iterate through all KeyValuePair entries
            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(SNBIRecord.Element))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var record = item as SNBIRecord.Element;
                        if (record != null)
                        {
                            hasEntries = true;

                            rowData.Add(
                            [
                                idCounter++,
                                record.BL01,
                                record.BID01 ?? "",
                                record.BCL01 ?? "",
                                record.BE01 ?? "",
                                record.BE02 ?? "",
                                record.BE03,
                                record.BCS01,
                                record.BCS02,
                                record.BCS03,
                                record.BCS04
                            ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);
            var lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 11);

            CreateTable(tableRange, "Duplicate Elements");
        }

        private void PopulateDuplicatePrimaryWs(IXLWorksheet ws, List<KeyValuePair<Type, IList>>? duplicates, string title)
        {
            // Early exit if there are no duplicates to process
            if (duplicates == null || duplicates.Count == 0)
            {
                ws.Delete();
                return;
            }

            bool hasEntries = false;

            // Setup the header of the worksheet
            SetupWorksheetHeader(ws, title);
            ws.Cell("A1").Value = $"Report Date: {reportDate}";

            int row = 5;
            var rowData = new List<object[]>();
            int idCounter = 1;

            // Iterate through all KeyValuePair entries
            foreach (var kvp in duplicates)
            {
                if (kvp.Key == typeof(ProcessingReport.Primary))
                {
                    IList duplicateList = kvp.Value;
                    foreach (var item in duplicateList)
                    {
                        var primary = item as ProcessingReport.Primary; // Safe cast to the appropriate type
                        if (primary != null)
                        {
                            hasEntries = true; // Set flag true if at least one Report.Primary exists
                           
                            rowData.Add(
                            [
                                idCounter++,
                                primary.BL01,
                                primary.BID01 ?? "",
                                primary.BCL01 ?? "",
                                primary.BL11 ?? ""
                             ]);
                        }
                    }
                }
            }

            if (!hasEntries)
            {
                ws.Delete();
                return;
            }

            ws.Cell(row, 1).InsertData(rowData);
            var lastRow = row + rowData.Count - 1;
            var tableRange = ws.Range(4, 1, lastRow, 5);

            CreateTable(tableRange, "Duplicate Bridge Records");

        }

        private void SetupWorksheetHeader(IXLWorksheet ws, string title)
        {
            // Adding Title
            ws.Cell(2, 1).Value = title;

            //if (isFedAgency)
            //{
            //ws.Cell(4, 1).Value = "State";
            //ws.Column(1).Width = 8;
            //ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //}
        }



    }
}
