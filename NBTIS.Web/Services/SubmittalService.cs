using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NBTIS.Data.Models;
using NBTIS.Web.ViewModels;
using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using NBTIS.Core.Enums;
using NBTIS.Core.Extensions;
using NBTIS.Core.DTOs;
using Telerik.DataSource.Extensions;
using System.Data;
using ErrorSummary = NBTIS.Web.ViewModels.ErrorSummary;

using File = System.IO.File;

using NBTIS.Core.Utilities;
using EmailService.Services;
using EmailService.Models;
using NBTIS.Core.Services;
using System.Net;
using Telerik.SvgIcons;


namespace NBTIS.Web.Services
{
    public class SubmittalService
    {
        private readonly DataContext _context;
        private readonly IDbContextFactory<DataContext> _factory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SubmittalService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly long _maxAllowedSize;
        private readonly string _uploadTempPath;

        private readonly IMapper _mapper;
        private byte[] ReportContent;
        private readonly ICurrentUserService _currentUserService;
        private readonly FileValidationService _fileValidationService;
        private readonly IEmailNotificationService _notify;

        private const string ReplaceAction = "Replace";
        private const string UpdateAction = "Update";

        // Static dictionary to hold SemaphoreSlim objects per uploadId for thread-safety
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _uploadSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        // Helper to get or create a semaphore for a given uploadId
        private SemaphoreSlim GetSemaphore(string uploadId)
        {
            return _uploadSemaphores.GetOrAdd(uploadId, key => new SemaphoreSlim(1, 1));
        }

        public SubmittalService(
            DataContext dataContext,
            IDbContextFactory<DataContext> factory,
            IHttpContextAccessor httpContextAccessor,
            HttpClientService httpClientService,
            ILogger<SubmittalService> logger,
            IConfiguration configuration,
            IWebHostEnvironment env,
            JsonSerializerOptions jsonOptions,
            IMapper mapper,
            IEmailManagerService emailService,
            ICurrentUserService currentUserService,
            FileValidationService fileValidationService,
            IEmailNotificationService notify

            )
        {
            _context = dataContext;
            _factory = factory;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientService.Client;
            _logger = logger;
            _jsonOptions = jsonOptions;
            _maxAllowedSize = configuration.GetValue<long>("FileUploadSettings:MaxAllowedSize");
            // Use a custom temp directory within the application root
            _uploadTempPath = System.IO.Path.Combine(env.ContentRootPath, "temp");
            _context = dataContext;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _fileValidationService = fileValidationService;
            _notify = notify;
        }


        /// <summary>
        /// Processes an uploaded file chunk.
        /// Throws exceptions if any error occurs.
        /// </summary>
        public async Task UploadChunkAsync(IFormFile fileChunk, string fileName, string uploadId, int chunkNumber, CancellationToken cancellationToken = default)
        {
            if (fileChunk == null || fileChunk.Length == 0)
            {
                throw new ArgumentException("No file chunk received.");
            }
            if (string.IsNullOrEmpty(uploadId))
            {
                throw new ArgumentException("Upload ID is required.");
            }

            // Build a temporary file name using the uploadId for uniqueness
            var tempFileName = System.IO.Path.Combine(_uploadTempPath, $"{uploadId}_{fileName}.tmp");

            // Get or create a semaphore for this uploadId to ensure sequential writes
            var semaphore = GetSemaphore(uploadId);

            await semaphore.WaitAsync();
            try
            {
                // If this is the first chunk, create the file; otherwise, append
                using (var stream = new FileStream(
                    tempFileName,
                    chunkNumber == 0 ? FileMode.Create : FileMode.Append,
                    FileAccess.Write,
                    FileShare.None))
                {
                    await fileChunk.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadChunk");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<Int64> InsertSubmittalRecordAsync(
         string fileName,
         Guid uploadId,
         string submittedBy,
         string isPartial,
         string comments,
         CancellationToken cancellationToken = default
         )
        {
            DateTime uploadDate = DateTime.Now;
            var uploadedBy = _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";

            var submittalLogRecord = new SubmittalLog
            {
                UploadId = uploadId,
                SubmittedBy = submittedBy,
                IsPartial = isPartial == "full" ? false : true,
                StatusCode = (byte)SubmittalStatus.Pending,
                UploadDate = uploadDate,
                UploadedBy = uploadedBy
            };

            // Insert the submittal log record.
            await _context.SubmittalLogs.AddAsync(submittalLogRecord);
            await _context.SaveChangesAsync();

            // Insert into SubmittalFile if fileName is provided.
            if (!string.IsNullOrEmpty(fileName))
            {
                var tempFilePath = System.IO.Path.Combine(_uploadTempPath, $"{uploadId}_{fileName}.tmp");

                if (File.Exists(tempFilePath))
                {
                    byte[] fileContent = await File.ReadAllBytesAsync(tempFilePath);

                    var submittalFileRecord = new SubmittalFile
                    {
                        SubmitId = submittalLogRecord.SubmitId,  // Link to SubmittalLog
                        FileType = 1, // Set appropriate FileType (adjust as needed)
                        FileName = fileName,
                        FileContent = fileContent
                    };

                    await _context.SubmittalFiles.AddAsync(submittalFileRecord);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new FileNotFoundException($"The file {tempFilePath} was not found.");
                }
            }

            // Create a submittal comment record using the generated SubmitId.
            if (comments != null)
            {
                var submittalCommentRecord = new SubmittalComment
                {
                    SubmitId = submittalLogRecord.SubmitId,
                    CommentText = comments,
                    CreatedBy = uploadedBy,
                    CreatedDate = uploadDate
                };

                // Insert the submittal comment record.
                await _context.SubmittalComments.AddAsync(submittalCommentRecord);
                await _context.SaveChangesAsync();
            }

            return submittalLogRecord.SubmitId; // Return the upload date for use in subsequent steps
        }

        public void CleanupTempFiles(string fileName, string uploadId)
        {
            var tempFileName = System.IO.Path.Combine(_uploadTempPath, $"{uploadId}_{fileName}.tmp");

            if (System.IO.File.Exists(tempFileName))
            {
                System.IO.File.Delete(tempFileName);
            }

            if (_uploadSemaphores.TryRemove(uploadId, out var semaphore))
            {
                semaphore.Dispose();
            }
        }

        public string SetExcelFileName(string reportDate, string stateAgencyCode, string fullPartial)
        {
            string fileName = "ProcessingReport.xlsx";
            try
            {
                string dateTime = reportDate.Replace("/", "").Replace(":", "");
                var dt = dateTime.Split(" ");
                fileName = $"ProcessingReport_{stateAgencyCode}_{fullPartial}_{dt[0]}_{dt[1]}.xlsx";

                return fileName;
            }
            catch (Exception ex)
            {
                return fileName;
            }
        }

        internal async Task<List<SubmittalItem>> GetSubmittalListAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                
                var result = await GetSubmittalLogDtosAsync(cancellationToken);
                var dtos = result.Where(s => s.StatusCode != (byte)SubmittalStatus.Accepted && 
                                             s.StatusCode != (byte)SubmittalStatus.Rejected && 
                                             s.StatusCode != (byte)SubmittalStatus.Canceled && 
                                             s.StatusCode != (byte)SubmittalStatus.Deleted &&
                                             s.StatusCode != (byte)SubmittalStatus.Merged);

                // Map DB models to view models
                var submittals = _mapper.Map<List<SubmittalItem>>(dtos);
                return submittals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching submittals list from the SubmittalLog table");
                return new List<SubmittalItem>();
            }
        }

        public async Task<List<SubmittalLogDTO>> GetSubmittalLogDtosAsync(CancellationToken ct = default)
        {
            try
            {
                await using var ctx = await _factory.CreateDbContextAsync(ct);

                var submittalLogs = await ctx.SubmittalLogs
                                        .AsNoTracking()
                                        .OrderByDescending(log => log.UploadDate)
                                        .Select(log => new SubmittalLogDTO
                                        {
                                            SubmitId = log.SubmitId,
                                            SubmittedBy = log.SubmittedBy,
                                            UploadDate = log.UploadDate,
                                            UploadedBy = log.UploadedBy,
                                            StatusCode = log.StatusCode,
                                            IsPartial = log.IsPartial,
                                            ReportContent = log.ReportContent != null ? new byte[] { 88, 88 } : null,
                                            SubmittalComments = log.SubmittalComments
                                                .Where(c => c.IsActive) // && c.CommentType != Constants.CommentType_ACC_REJ
                                                .Select(c => new SubmittalCommentDTO
                                                {
                                                    Id = c.Id,
                                                    IsActive = c.IsActive,
                                                    SubmitId = c.SubmitId,
                                                    CommentText = c.CommentText,
                                                    CreatedBy = c.CreatedBy,
                                                    CreatedDate = c.CreatedDate,
                                                    UpdatedBy = c.UpdatedBy,
                                                    UpdatedDate = c.UpdatedDate
                                                }).ToList()
                                        })
                                        .ToListAsync(ct);

                // Get lookup data for states and agencies
                var (lookupStates, lookupAgencies) = await GetLookupDataAsync(ct);

                var dtos = submittalLogs.Select(log =>
                {
                    var state = lookupStates.FirstOrDefault(s => s.Code.ToString() == log.SubmittedBy);
                    string? description = state != null
                        ? state.Description
                        : lookupAgencies.FirstOrDefault(a => a.Code == log.SubmittedBy)?.Description
                          ?? log.SubmittedBy;

                    return new SubmittalLogDTO
                    {
                        SubmitId = log.SubmitId,
                        SubmittedBy = log.SubmittedBy,
                        //FileContent = log.SubmittalFiles.Select(f => f.FileContent).FirstOrDefault(),
                        UploadDate = log.UploadDate,
                        UploadedBy = log.UploadedBy,
                        StatusCode = log.StatusCode,
                        IsPartial = log.IsPartial,
                        ReportContent = log.ReportContent,
                        SubmittedByDescription = description,
                        SubmittalComments = log.SubmittalComments.Where(e => e.IsActive == true)
                        .Select(c => new SubmittalCommentDTO
                        {
                            Id = c.Id,
                            IsActive = c.IsActive,
                            SubmitId = c.SubmitId,
                            CommentText = c.CommentText,
                            CreatedBy = c.CreatedBy,
                            CreatedDate = c.CreatedDate,
                            UpdatedBy = c.UpdatedBy,
                            UpdatedDate = c.UpdatedDate
                        })
                        .ToList()


                    };
                }).ToList();

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching submittal log DTOs");
                return new List<SubmittalLogDTO>();
            }
        }


        public async Task<(List<Lookup_State>, List<LookupValue>)> GetLookupDataAsync(CancellationToken cancellationToken)
        {
            // Get lookup data for states
            var lookupStates = await _context.Lookup_States.ToListAsync(cancellationToken);

            // Get lookup data for agencies with specific filtering criteria
            var lookupAgencies = await _context.LookupValues
                .Where(s => s.FieldName == Constants.FedAgencyFieldName &&
                            s.IsActive == true &&
                            !s.Code.StartsWith("S") &&
                            !s.Code.StartsWith("L") &&
                            !s.Code.StartsWith("P"))
                .ToListAsync(cancellationToken);

            return (lookupStates, lookupAgencies);
        }

        #region SUBMIT BRIDGE INVENTORY TO DIVISION 

        //public async Task<bool> SubmitToDivisionAsync(long submitId, long prevSubmitId, string action, string stateAgencyName, CancellationToken token = default)
        //{
        //    var logEntry = await _context.SubmittalLogs.FirstOrDefaultAsync(s => s.SubmitId == submitId, token);
        //    if (logEntry is null)
        //        return false;

        //    var submittalType = logEntry.IsPartial
        //           ? SubmittalType.Partial
        //           : SubmittalType.Full;

        //    var attachments = BuildProcessingReportAttachments(logEntry, submittalType);

        //    if (action == "Replace" && prevSubmitId != 0)
        //    {
        //        //Cancel previous submission
        //        await CancelSubmittalAsync(prevSubmitId);

        //        //Set new submission status as Division Review
        //        await _fileValidationService.SetSubmittalStatusAsync(
        //            submitId,
        //            SubmittalStatus.DivisionReview,
        //            submitDateOverride: DateTime.Now,
        //            token: token
        //        );

        //        /////***** Send Notification Email *****/////                                 
        //        await _notify.NotifySubmissionAsync(
        //            logEntry,
        //            submittalType: submittalType,
        //            stateAgencyName: stateAgencyName,
        //            notificationType: SubmissionNotificationType.Submitted,
        //            attachments: attachments
        //        );
        //    }
        //    else if (action == "Update" && prevSubmitId != 0)
        //    {
        //        // ❌ DELETE PREVIOUS ERRORS
        //        var oldErrors = _context.SubmittalErrors.Where(e => e.SubmitId == prevSubmitId || e.SubmitId == submitId);
        //        _context.SubmittalErrors.RemoveRange(oldErrors);
        //        await _context.SaveChangesAsync(token);

        //        // ✅ MERGE + REVALIDATE
        //        await MergeSubmittalAsync(submitId, prevSubmitId, token);
        //        await ReValidateSubmittalAsync(prevSubmitId, token);

        //        /////***** Send Notification Email *****/////                              
        //        await _notify.NotifySubmissionAsync(
        //            logEntry,
        //            submittalType: submittalType,
        //            stateAgencyName: stateAgencyName,
        //            notificationType: SubmissionNotificationType.Merged,
        //            attachments: attachments
        //        );
        //    }
        //    else
        //    {
        //        await _fileValidationService.SetSubmittalStatusAsync(
        //            submitId,
        //            SubmittalStatus.DivisionReview,
        //            submitDateOverride: DateTime.Now,
        //            token: token
        //        );

        //       /////***** Send Notification Email *****/////                                 
        //            await _notify.NotifySubmissionAsync(
        //                logEntry,
        //                submittalType: submittalType,
        //                stateAgencyName: stateAgencyName,
        //                notificationType: SubmissionNotificationType.Submitted,
        //                attachments: attachments
        //            );

        //    }

        //    return true;
        //}

        public async Task<bool> SubmitToDivisionAsync(
       long submitId,
       long prevSubmitId,
       string action,
       string stateAgencyName,
       CancellationToken cancellationToken = default)
        {
            // 1) Load the log entry
            var logEntry = await _context.SubmittalLogs
                .FirstOrDefaultAsync(x => x.SubmitId == submitId, cancellationToken);
            if (logEntry == null)
                return false;

            // 2) Prepare common data
            var submittalType = logEntry.IsPartial
                ? SubmittalType.Partial
                : SubmittalType.Full;

            var attachments = BuildProcessingReportAttachments(logEntry, submittalType);

            // 3) Branch on action
            SubmissionNotificationType notificationType;
            if (action.Equals(ReplaceAction, StringComparison.OrdinalIgnoreCase)
                && prevSubmitId != 0)
            {
                await HandleReplaceAsync(prevSubmitId, submitId, cancellationToken);
                notificationType = SubmissionNotificationType.Submitted;
            }
            else if (action.Equals(UpdateAction, StringComparison.OrdinalIgnoreCase)
                     && prevSubmitId != 0)
            {
                await HandleUpdateAsync(submitId, prevSubmitId, cancellationToken);
                notificationType = SubmissionNotificationType.Merged;
            }
            else
            {
                await SetDivisionReviewStatusAsync(submitId, cancellationToken);
                notificationType = SubmissionNotificationType.Submitted;
            }

            // 4) Send notification
            await _notify.NotifySubmissionAsync(
                logEntry,
                submittalType: submittalType,
                stateAgencyName: stateAgencyName,
                notificationType: notificationType,
                attachments: attachments
            );

            return true;
        }

        private async Task HandleReplaceAsync(
            long submitIdToCancel,
            long currentSubmitId,
            CancellationToken cancellationToken)
        {
            await CancelSubmittalAsync(submitIdToCancel);
            await SetDivisionReviewStatusAsync(currentSubmitId, cancellationToken);
        }

        private async Task HandleUpdateAsync(
            long submitId,
            long prevSubmitId,
            CancellationToken cancellationToken)
        {
            await RemovePreviousErrorsAsync(prevSubmitId, submitId, cancellationToken);
            await MergeSubmittalAsync(submitId, prevSubmitId, cancellationToken);
            await ReValidateSubmittalAsync(prevSubmitId, cancellationToken);
        }

        private async Task RemovePreviousErrorsAsync(
            long prevSubmitId,
            long submitId,
            CancellationToken cancellationToken)
        {
            var errors = _context.SubmittalErrors
                .Where(e => e.SubmitId == prevSubmitId || e.SubmitId == submitId);

            if (await errors.AnyAsync(cancellationToken))
            {
                _context.SubmittalErrors.RemoveRange(errors);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task SetDivisionReviewStatusAsync(
            long submitId,
            CancellationToken cancellationToken)
        {
            await _fileValidationService.SetSubmittalStatusAsync(
                submitId,
                SubmittalStatus.DivisionReview,
                submitDateOverride: DateTime.UtcNow,
                token: cancellationToken
            );
        }


        /// <summary>
        /// If the log entry has ReportContent, wraps it in a MemoryStream/FormFile and
        /// returns a single-element List&lt;IFormFile&gt;; otherwise null.
        /// </summary>
        private List<IFormFile>? BuildProcessingReportAttachments(SubmittalLog logEntry, SubmittalType submittalType)
        {
            var bytes = logEntry.ReportContent;
            if (bytes == null || bytes.Length == 0)
                return null;

            var stream = new MemoryStream(bytes)
            {
                Position = 0
            };

            var timestamp = logEntry.UploadDate
                              .ToString("yyyyMMddHHmmss");
            var fileName = $"ProcessingReport_"
                         + $"{logEntry.SubmittedBy}_"
                         + $"{submittalType}_"
                         + $"{timestamp}.xlsx";

            var formFile = new FormFile(stream, 0, bytes.Length,
                                        name: "ProcessingReport",
                                        fileName: fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            return new List<IFormFile> { formFile };
        }
        

        private async Task ReValidateSubmittalAsync(long submitId, CancellationToken token)
        {
            DateTime reportDate = DateTime.Now;

            var submittalLog = _context.SubmittalLogs.Where(x => x.SubmitId == submitId).FirstOrDefault();
            
            var processResult = await _fileValidationService.ValidateFileAsync(
           tempFileName: null,
           uploadId: null,
           uploadDate: reportDate,
           submitId,
           submittedBy: submittalLog?.SubmittedBy ?? string.Empty,
           connectionId: null,
           revalidate: true,
           token
       );
            
            // Generate Excel report

            string tempExcelFileName = await _fileValidationService.GenerateExcelReportAsync(
                reportDate,
                processResult,
                token
            );
           
            // Write report back to DB
            token.ThrowIfCancellationRequested();
            byte[] excelContent = await System.IO.File.ReadAllBytesAsync(tempExcelFileName, token);
            await UpdateProcessingReportContentAsync(
                submitId,
                excelContent,
                token
            );
      
        }

        public async Task MergeSubmittalAsync(long sourceId /*101*/, long targetId /* Already in DivisionReview 100*/, CancellationToken ct = default)
        {
            var strategy = _factory.CreateDbContext().Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var ctx = await _factory.CreateDbContextAsync(ct);
                await using var tx = await ctx.Database.BeginTransactionAsync(ct);

                var detect = ctx.ChangeTracker.AutoDetectChangesEnabled;
                ctx.ChangeTracker.AutoDetectChangesEnabled = false;

                try
                {
                    var sourceRows = await ctx.Stage_BridgePrimaries
                        .AsNoTracking()
                        .Where(r => r.SubmitId == sourceId)
                        .ToListAsync(ct);

                    var targetRows = await ctx.Stage_BridgePrimaries
                        .Where(r => r.SubmitId == targetId)
                        .ToListAsync(ct);

                    var lookup = targetRows.ToDictionary(
                         r => (r.StateCode_BL01, r.BridgeNo_BID01, r.SubmittedBy), r => r);

                    foreach (var src in sourceRows)
                    {
                        var key = (src.StateCode_BL01, src.BridgeNo_BID01, src.SubmittedBy);

                        if (lookup.TryGetValue(key, out var existing))
                        {
                            // Copy non-primary-key values from source to existing
                            var srcVals = ctx.Entry(src).CurrentValues;

                            foreach (var prop in ctx.Entry(existing).Properties)
                            {
                                if (!prop.Metadata.IsPrimaryKey())
                                {
                                    prop.CurrentValue = srcVals[prop.Metadata.Name];
                                }
                            }
                        }
                        else
                        {
                            var clone = ctx.Entry(src).CurrentValues.ToObject() as Stage_BridgePrimary;
                            clone!.SubmitId = targetId;
                            ctx.Stage_BridgePrimaries.Add(clone);
                        }
                    }

                    /// TODO: Delete rows from Stage* tables for sourceId (The one that we are merging)
                    await BulkDeleteStage(sourceId);

                    await ctx.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                }
                finally
                {
                    ctx.ChangeTracker.AutoDetectChangesEnabled = detect;
                }
            });

            // Centralized status update after successful merge
            await _fileValidationService.SetSubmittalStatusAsync(
                submitId: sourceId,
                newStatus: SubmittalStatus.Merged,
                mergedInto: targetId,
                submitDateOverride: DateTime.Now,
                token: ct
            );
        }

        public async Task<(bool canSubmit, string action, long? submitId, string msg)> EvaluateSubmissionAsync(string submittedBy, string submittedByDescription, SubmittalType type, CancellationToken ct = default)
        {
            await using var ctx = await _factory.CreateDbContextAsync(ct);

            // Pull the single most‑recent active row for this submitter
            var active = await ctx.SubmittalLogs
                .AsNoTracking()
                .Where(l => l.SubmittedBy == submittedBy &&
                           (l.StatusCode == (byte)SubmittalStatus.DivisionReview ||
                            l.StatusCode == (byte)SubmittalStatus.HQReview))
                .OrderByDescending(l => l.UploadDate)
                .Select(l => new { l.SubmitId, l.StatusCode, l.IsPartial })
                .FirstOrDefaultAsync(ct);

            // HQ Review blocks everything
            if (active is { StatusCode: (byte)SubmittalStatus.HQReview })
                return (false, "HQReview", null, null);

            // No active Division‑Review → add new
            if (active is null)
                return (true, "Add", null, null);

            // One record in Division Review → decide next step
            string msg;
            string action = type == SubmittalType.Full ? "Replace" : "Update";

            if (type == SubmittalType.Full)
                msg = $"A {(active.IsPartial ? "PARTIAL" : "FULL")} submittal for {submittedByDescription} " +
                      "is already in Division Review.\nDo you want to overwrite it?";
            else
                msg = $"A {(active.IsPartial ? "PARTIAL" : "FULL")} submittal for {submittedByDescription} " +
                      "is already in Division Review.\nDo you want to update it with your PARTIAL data?";

            return (true, action, active.SubmitId, msg);
        }

        public async Task UpdateProcessingReportContentAsync(long submitId, byte[] excelContent, CancellationToken cancellationToken = default)
        {
            var record = await _context.SubmittalLogs.FirstOrDefaultAsync(r => r.SubmitId == submitId);

            if (record != null)
            {
                record.ReportContent = excelContent;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning($"No submittal logEntry found with SubmitId: {submitId}");
            }
        }

        #endregion


        internal async Task<List<SubmittalItem>> GetSubmittalListByStateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the DTOs including description
                var dtos = await GetSubmittalLogDtosAsync(cancellationToken);

                // Map DB models to view models
                var submittals = _mapper.Map<List<SubmittalItem>>(dtos);

                // Filter for In Division Review only
                submittals = submittals.Where(s => s.StatusCode == (byte)SubmittalStatus.DivisionReview).ToList();

                return submittals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching submittals list from the SubmittalLog table");
                return new List<SubmittalItem>();
            }
        }

        public async Task<DateTime> UpdateSubmittalRecordAsync(
             string fileName,
             Guid uploadId,
             string submittedBy,
             string isPartial,
             string comments,
             string commentType)
            {
                DateTime uploadDate = DateTime.Now;
                var uploadedBy = _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";

                var submittalLogRecord = new SubmittalLog
                {
                    UploadId = uploadId,
                    SubmittedBy = submittedBy,
                    IsPartial = isPartial == "full" ? false : true,
                    StatusCode = (byte)SubmittalStatus.New,
                    UploadDate = uploadDate,
                    UploadedBy = uploadedBy
                };

                // Insert the submittal log record.
                await _context.SubmittalLogs.AddAsync(submittalLogRecord);
                await _context.SaveChangesAsync();

                // Create a submittal comment record using the generated SubmitId.
                if (comments != null)
                {
                    await AddSubmittalCommentsAsync(comments, uploadDate, uploadedBy, submittalLogRecord.SubmitId, commentType);
                }

                return uploadDate; // Return the upload date for use in subsequent steps
            }

        private async Task<SubmittalComment> AddSubmittalCommentsAsync(string comments, DateTime uploadDate, string uploadedBy, long SubmitId, string commentType)
        {
            var submittalCommentRecord = new SubmittalComment
            {
                SubmitId = SubmitId,
                CommentText = comments,
                CreatedBy = uploadedBy,
                CreatedDate = uploadDate,
                CommentType = commentType,
                IsActive=true
            };

            // Insert the submittal comment record.
            await _context.SubmittalComments.AddAsync(submittalCommentRecord);
            await _context.SaveChangesAsync();
            return submittalCommentRecord;
        }
        private async Task UpdateSubmittalCommentsAsync(string comments,long Id)
        {
            var record = await _context.SubmittalComments
                            .Where(tbl => tbl.Id == Id)
                            .FirstOrDefaultAsync();
            if (record != null)
            {
                record.CommentText = comments;
                record.UpdatedDate = DateTime.Now;
                record.UpdatedBy= _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";
            }
            await _context.SaveChangesAsync();      
        }

        public async Task<List<ErrorSummary>> GetErrorSummaryBySubmittal(long submitId)
        {
            var semaphore = GetSemaphore(submitId.ToString());
            await semaphore.WaitAsync();

            try
            {
                var errorSummaries = await (from error in _context.SubmittalErrors
                                            join lookup in _context.Lookup_DataItems
                                            on error.ItemId equals lookup.NBI_Id into lookupGroup
                                            from lookup in lookupGroup.DefaultIfEmpty()
                                            where error.SubmitId == submitId
                                            select new ErrorSummary
                                            {
                                                ErrorId = error.ErrorId,
                                                State = error.StateCode == null ? null : error.StateCode,
                                                BID01 = error.BridgeNo ?? string.Empty,
                                                BCL01 = error.Owner ?? string.Empty,
                                                ItemId = error.ItemId ?? string.Empty,
                                                ItemName = lookup != null ? lookup.ItemName : string.Empty,
                                                SubmittedValue = error.SubmittedValue ?? string.Empty,
                                                ErrorType = error.ErrorType ?? string.Empty,
                                                Description = error.ErrorDescription ?? string.Empty,
                                                Reviewed = error.Reviewed,
                                                Ignore = error.Ignore,
                                                Comment = error.Comments ?? string.Empty
                                            })
                                        .ToListAsync();

                return errorSummaries;
            }
            finally
            {
                semaphore.Release();
            }
        }


        //internal async Task<string> UpdateStatusToHQReviewAsync(long submitId, string comment)
        //{
        //    DateTime approvedDate = DateTime.Now;
        //    var approvedBy = _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";


        //    var submittalLogRecord = await _context.SubmittalLogs.FirstOrDefaultAsync(x => x.SubmitId == submitId);

        //    if (submittalLogRecord == null)
        //    {
        //        return "Submittal record not found.";
        //    }

        //    submittalLogRecord.StatusCode = (byte)SubmittalStatus.HQReview;
        //    submittalLogRecord.Reviewer = approvedBy;
        //    submittalLogRecord.ReviewDate = approvedDate;

        //    var newComment = new SubmittalCommentDTO
        //    {
        //        CommentText = comment,
        //        SubmitId = submitId,
        //        CommentType = "HQ"
        //    };

        //    await SaveNewCommentAsync(newComment);

        //    await _context.SaveChangesAsync();

        //    //send email
        //    var submitterEmail = submittalLogRecord.UploadedBy; //uploader email?

        //    var emailViewModel = new EmailModel
        //    {
        //        To = submitterEmail, //HQ and Submitter email?
        //        From = "NBTIS-no-reply@dot.gov",
        //        Subject = "Submission Approved",
        //        Body = $"Submittal {submitId} has been updated to HQ Review.",
        //        EmailType = "Approval"
        //    };

        //    _ = SendEmailAsync(emailViewModel);


        //    return "Successfully updated to HQ Review status.";
        //}

        //internal async Task<string> UpdateStatusToReturnedByDivisionAsync(long submitId, string comment)
        //{
        //    DateTime approvedDate = DateTime.Now;
        //    var approvedBy = _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";
        //    var submittalLogRecord = await _context.SubmittalLogs.FirstOrDefaultAsync(x => x.SubmitId == submitId);

        //    if (submittalLogRecord == null)
        //    {
        //        return "Submittal record not found.";
        //    }

        //    submittalLogRecord.StatusCode = (byte)SubmittalStatus.ReturnedByDivision;
        //    submittalLogRecord.Approver = approvedBy;
        //    submittalLogRecord.ApproveRejectDate = approvedDate;

        //    var newComment = new SubmittalCommentDTO
        //    {
        //        CommentText = comment,
        //        SubmitId = submitId,
        //        CommentType = "HQ"
        //    };

        //    await SaveNewCommentAsync(newComment);

        //    await _context.SaveChangesAsync();

        //    //send email
        //    var submitterEmail = submittalLogRecord.UploadedBy; //uploader email?

        //    var emailViewModel = new EmailModel
        //    {
        //        To = submitterEmail, //submitter email?
        //        From = "NBTIS-no-reply@dot.gov",
        //        Subject = "Returned Submission",
        //        Body = $"Submittal {submitId} has been Returned.",
        //        EmailType = "Returned"
        //    };

        //    _ = SendEmailAsync(emailViewModel);

        //    return "Successfully returned submittal.";
        //}

        internal async Task<SubmittalComment> SaveNewCommentAsync(SubmittalCommentDTO newComment)
        {
            var uploadedBy = _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";
            DateTime uploadDate = DateTime.Now;
           return await AddSubmittalCommentsAsync(newComment.CommentText, uploadDate, uploadedBy, newComment.SubmitId, newComment.CommentType);           
        }

        internal async Task UpdateCommentAsync(SubmittalCommentDTO newComment)
        {
            await UpdateSubmittalCommentsAsync(newComment.CommentText, newComment.Id);
        }
        internal async Task DeleteCommentAsync(SubmittalCommentDTO newComment)
        {
            await DeleteSubmittalCommentsAsync( newComment.Id);
        }

        private async Task DeleteSubmittalCommentsAsync(long Id)
        {
            // Find the record by ID
            var record = await _context.SubmittalComments
                                       .FirstOrDefaultAsync(tbl => tbl.Id == Id);
            if (record != null)
            {
                record.IsActive = false;
                record.UpdatedDate = DateTime.Now;
                record.UpdatedBy = _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";

            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateErrorSummariesAsync(long submitId, long errorId, bool reviewed, bool ignore)
        {
            var errorSummaries = await _context.SubmittalErrors
                .Where(x => x.SubmitId == submitId && x.ErrorId == errorId)
                .ToListAsync();

            var currentUser = _httpContextAccessor.HttpContext?.User.GetOktaClaimEmail()?.Value ?? "unknown@example.com";

            foreach (var errorSummary in errorSummaries)
            {
                if (errorSummary.Reviewed != reviewed)
                {
                    errorSummary.Reviewed = reviewed;
                    errorSummary.ReviewedBy = currentUser;
                    errorSummary.ReviewedDate = DateTime.Now;
                }

                if (errorSummary.Ignore != ignore)
                {
                    errorSummary.Ignore = ignore;
                    errorSummary.IgnoredBy = currentUser;
                    errorSummary.IgnoredDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<SubmissioniStatusItemViewModel>> Load_SubmissionStatus()
        {
            try
            {
                var tbl_results = await _context.VSubmittalLogs.AsNoTracking().OrderBy(e=>e.StateOrder).ThenBy(e=>e.Lookup_States_Abbreviation).ToListAsync();
                var mappedResults = _mapper.Map<List<SubmissioniStatusItemViewModel>>(tbl_results);
                return mappedResults;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new ApplicationException("An error occurred while loading submission status.", ex);
            }
        }

        public async Task<VSubmittalLog?> Load_VsubmittalLog(long SubmitId)
        {
            try
            {
                return await _context.VSubmittalLogs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.SubmitId == SubmitId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while loading submission status.", ex);
            }
        }
        public async Task<SubmittalFile?> Load_SubmittalFile(long submitId)
        {
            try
            {
                var tbl_results = await _context.SubmittalFiles.Where(e => e.SubmitId == submitId).FirstOrDefaultAsync();

                return tbl_results;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new ApplicationException("An error occurred while loading submission status.", ex);
            }
        }

        public async Task<SubmittalLog?> Load_SubmittalReport(long submitId, CancellationToken ct = default)
        {
            try
            {
                await using var ctx = await _factory.CreateDbContextAsync(ct);
                return await ctx.SubmittalLogs
                            .AsNoTracking()
                            .SingleOrDefaultAsync(l => l.SubmitId == submitId, ct);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw new ApplicationException("An error occurred while loading submission status.", ex);
            }
        }

        internal int GetBridgeInventoryCount(string? submittedBy)
        {
            var count = _context.BridgePrimaries.Where(x => x.SubmittedBy == submittedBy).Count();
            return count;
        }

        internal int GetStageCount(string? submittedBy)
        {
            var count = _context.Stage_BridgePrimaries.Where(x => x.SubmittedBy == submittedBy).Count();
            return count;
        }

        public async Task<bool> DeleteSubmittalRecordAsync(long submitId)
        {
            //-------- Remove Submittal Comments -----------
            var records = await _context.SubmittalComments.Where(s => s.SubmitId == submitId).ToListAsync();
            if (records.Any())
            {
                _context.SubmittalComments.RemoveRange(records);
                await _context.SaveChangesAsync();
            }

            //-------- Remove Submittal Files -----------
            var fileRecords = await _context.SubmittalFiles.Where(s => s.SubmitId == submitId).ToListAsync();
            if (fileRecords != null)
            {
                _context.SubmittalFiles.RemoveRange(fileRecords);
                await _context.SaveChangesAsync();
            }

            //-------- Remove Submittal Errors -----------
            var errorRecords = await _context.SubmittalErrors.Where(s => s.SubmitId == submitId).ToListAsync();
            if (errorRecords.Any())
            {
                _context.SubmittalErrors.RemoveRange(errorRecords);
                await _context.SaveChangesAsync();
            }


            // Use EF Core's ExecuteSqlRawAsync to perform bulk deletes
            await BulkDeleteStage(submitId);

            // Find the submittal record with the specified submitId
            var record = await _context.SubmittalLogs.FirstOrDefaultAsync(s => s.SubmitId == submitId);

            if (record == null)
            {
                // Record not found, nothing to delete.
                return true;
            }

            // Change Status to Deleted = 10          
             record.StatusCode = 10;
                      
            await _context.SaveChangesAsync();
            return true;
        }

        internal async Task<bool> CancelSubmittalAsync(long submitId)
        {
            //-------- Leave Submittal Comments -----------

            await BulkDeleteAllSubmittalFiles(submitId);

            // Find the submittal record with the specified submitId
            var record = await _context.SubmittalLogs.FirstOrDefaultAsync(s => s.SubmitId == submitId);

            if (record != null)
            {
                record.StatusCode = (byte)SubmittalStatus.Canceled;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        private async Task BulkDeleteAllSubmittalFiles(long submitId)
        {
            //-------- Remove Submittal Files -----------
            var fileRecords = await _context.SubmittalFiles.Where(s => s.SubmitId == submitId).ToListAsync();
            if (fileRecords != null)
            {
                _context.SubmittalFiles.RemoveRange(fileRecords);
                await _context.SaveChangesAsync();
            }

            //-------- Remove Submittal Errors -----------
            var errorRecords = await _context.SubmittalErrors.Where(s => s.SubmitId == submitId).ToListAsync();
            if (errorRecords.Any())
            {
                _context.SubmittalErrors.RemoveRange(errorRecords);
                await _context.SaveChangesAsync();
            }

            // Use EF Core's ExecuteSqlRawAsync to perform bulk deletes
            await BulkDeleteStage(submitId);
        }
            
        private async Task BulkDeleteStage(long submitId)
        {
           
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgeElements] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
               "DELETE FROM [NBI].[Stage_BridgeRoutes] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgeFeatures] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgeInspections] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgePostingEvaluations] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgePostingStatuses] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgeSpanSets] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgeSubstructureSets] WHERE SubmitId = {0}", submitId);

            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgeWorks] WHERE SubmitId = {0}", submitId);

            // Finally, delete the parent records
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [NBI].[Stage_BridgePrimary] WHERE SubmitId = {0}", submitId);
        }

        private async Task BulkDeleteBySubmittedByFromStage(long submitId)
        {
                            // Delete from BridgeElements
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgeElements] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgeElements]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgeFeatures
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgeFeatures] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgeFeatures]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgeInspections
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgeInspections] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgeInspections]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgePostingEvaluations
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgePostingEvaluations] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgePostingEvaluations]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgePostingStatuses
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgePostingStatuses] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgePostingStatuses]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgePrimary
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgePrimary] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgePrimary]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgeRoutes
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgeRoutes] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgeRoutes]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgeSpanSets
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgeSpanSets] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgeSpanSets]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgeSubstructureSets
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgeSubstructureSets] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgeSubstructureSets]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);

                            // Delete from BridgeWorks
                            await _context.Database.ExecuteSqlRawAsync(
                            @"DELETE T
                      FROM [NBI].[BridgeWorks] T
                      INNER JOIN (
                          SELECT DISTINCT SubmittedBy
                          FROM [NBI].[Stage_BridgeWorks]
                          WHERE SubmitId = {0}
                      ) S
                          ON T.SubmittedBy = S.SubmittedBy", submitId);
        }

        public async Task<List<SubmittalCommentDTO>> GetSubmittalCommentsSSAsync(long submitId)
        {
            List<SubmittalCommentDTO> EditItem_SubmittalComments = await _context.SubmittalComments
                .Where(e => e.SubmitId == submitId && e.IsActive==true) //&& e.CommentType == Constants.CommentType_ACC_REJ
                .Select(c => new SubmittalCommentDTO
                {
                    Id = c.Id,
                    IsActive = c.IsActive,
                    SubmitId = c.SubmitId,
                    CommentText = c.CommentText,
                    CreatedBy = c.CreatedBy,
                    CreatedDate = c.CreatedDate,
                    UpdatedBy = c.UpdatedBy,
                    UpdatedDate = c.UpdatedDate,
                    CommentType=c.CommentType
                })
                .ToListAsync(); // Important to await!

            return EditItem_SubmittalComments;
        }

        internal async Task<string> DivisionApproveReturnAsync(
     long submitId,
     byte statusCode,
     string comment,
     bool isPartial,
     string stateAgencyName)
        {
            const string CommentTypeHQ = "HQ";
            var userEmail = _currentUserService.UserId;
            var now = DateTime.Now;

            try
            {
                await using var ctx = await _factory.CreateDbContextAsync();

                var logEntry = await ctx.SubmittalLogs.FirstOrDefaultAsync(x => x.SubmitId == submitId);
                if (logEntry == null)
                    return "Submittal logEntry not found.";

                if (!Enum.IsDefined(typeof(SubmittalStatus), statusCode))
                    return "Invalid status code.";

                // Update submittal log
                logEntry.StatusCode = statusCode;
                logEntry.Reviewer = userEmail;
                logEntry.ReviewDate = now;

                // Add comment
                await AddSubmittalCommentsAsync(comment, now, userEmail, submitId, CommentTypeHQ);
                await ctx.SaveChangesAsync();

                SubmissionNotificationType? notificationType = statusCode switch
                {
                    (byte)SubmittalStatus.HQReview => SubmissionNotificationType.ApprovedByDivision,
                    (byte)SubmittalStatus.ReturnedByDivision => SubmissionNotificationType.ReturnedByDivision,
                    _ => null
                };

                if (notificationType.HasValue)
                {
                    var submittalType = isPartial
                        ? SubmittalType.Partial
                        : SubmittalType.Full;

                    await _notify.NotifySubmissionAsync(
                        logEntry,
                        submittalType: submittalType,
                        stateAgencyName: stateAgencyName,
                        notificationType: notificationType.Value
                    );
                }

                return statusCode switch
                {
                    (byte)SubmittalStatus.HQReview => "Successfully updated to HQ Review status.",
                    (byte)SubmittalStatus.ReturnedByDivision => "Successfully returned submittal.",
                    _ => "Status updated, no notification sent."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DivisionApproveReturn for SubmitId {submitId}", submitId);
                return $"An error occurred : {ex.Message}";
            }
        }



        public async Task AcceptRejectAsync(long submitId, byte statusCode, string comment, bool IsPartial, string stateAgencyName)
        {
            try
            {
                //await BulkDeleteAllSubmittalFiles(submitId);
                var logEntry = await _context.SubmittalLogs
                          .FirstOrDefaultAsync(e => e.SubmitId == submitId);

                //exec nbi.usp_AcceptBridgeBatch @submitid=53,@IsPartial=0
                if (statusCode == (byte)SubmittalStatus.Accepted)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                            "EXEC [NBI].[usp_AcceptBridgeBatch] @SubmitId = {0}, @IsPartial = {1}",
                            (long)submitId, // or long if the stored proc expects bigint
                            IsPartial ? 1 : 0); // Convert bool to int
                }

                if (logEntry != null)
                {
                    logEntry.StatusCode = statusCode;
                    logEntry.ApproveRejectDate = DateTime.Now;
                    logEntry.Approver = _currentUserService.UserId;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new KeyNotFoundException($"No SubmittalLog found for SubmitId: {submitId}");
                }

                var newComment = new SubmittalCommentDTO
                {
                    SubmitId = submitId,
                    CommentText = "Final Comment: " + comment,
                    CreatedBy = _currentUserService.UserId,
                    CreatedDate = DateTime.Now,
                    CommentType = NBTIS.Core.Utilities.Constants.CommentType_ACC_REJ,
                };
                await SaveNewCommentAsync(newComment);

                /////------- Send Notification Email ------///// 

                if (statusCode == (byte)SubmittalStatus.Accepted)
                {

                    await _notify.NotifySubmissionAsync(
                        logEntry,
                        submittalType: IsPartial ? SubmittalType.Partial : SubmittalType.Full,
                        stateAgencyName: stateAgencyName ?? string.Empty,
                        notificationType: SubmissionNotificationType.Accepted
);
                }
                else
                {
                    await _notify.NotifySubmissionAsync(
                                           logEntry,
                                           submittalType: IsPartial ? SubmittalType.Partial : SubmittalType.Full,
                                           stateAgencyName: stateAgencyName ?? string.Empty,
                                           notificationType: SubmissionNotificationType.Rejected
                   );

                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while updating submission status.", ex);
            }
        }


    }
}
