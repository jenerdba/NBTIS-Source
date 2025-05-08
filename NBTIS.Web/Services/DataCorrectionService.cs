using AutoMapper;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NBTIS.Core.Hub;
using NBTIS.Data.Models;
using NBTIS.Web.ViewModels;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IO;
using static Telerik.Blazor.ThemeConstants;
using Microsoft.Identity.Client;
using NBTIS.Core.Enums;
using NBTIS.Core.Extensions;
using NBTIS.Core.DTOs;
using NBTIS.Core.Services;
using System.Collections;
using Telerik.DataSource.Extensions;
using static NBTIS.Web.Services.SubmittalService;
using RulesEngine.Models;
using Microsoft.Data.SqlClient;
using NBTIS.Web.ViewModels;
using System.Data;
using ErrorSummary = NBTIS.Web.ViewModels.ErrorSummary;
using Telerik.SvgIcons;
using File = System.IO.File;
using System.Reflection.Metadata.Ecma335;
using NBTIS.Core.Utilities;
using DocumentFormat.OpenXml.InkML;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using NBTIS.Web.Components.Pages;
using EmailService.Models;
using System;
using Microsoft.Graph.Models.Partners.Billing;

namespace NBTIS.Web.Services
{
    public class DataCorrectionService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<DataCorrectionService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<DataContext> _contextFactory;

        public DataCorrectionService(
            IHttpContextAccessor httpContextAccessor,
            HttpClientService httpClientService,
            IHubContext<MessageHub> hubContext,
            ILogger<DataCorrectionService> logger,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IDbContextFactory<DataContext> contextFactory,
            IMapper mapper
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientService.Client;
            _hubContext = hubContext;
            _logger = logger;
            _contextFactory = contextFactory;
            _mapper = mapper;
        }


        internal async Task<List<StageBridgeItem>> GetStagedBridgeListByStateAsync(string submittedBy, CancellationToken cancellationToken = default)
        {
            try
            {
                var dtos = await GetStageBridgeDtosAsync(submittedBy, cancellationToken);
                var stagedBridges = _mapper.Map<List<StageBridgeItem>>(dtos);

                return stagedBridges;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching staged bridge list from the Stage Bridges table");
                return new List<StageBridgeItem>();
            }
        }

        public async Task<List<Stage_BridgePrimary>> GetStageBridgeDtosAsync(string submittedBy, CancellationToken cancellationToken = default)
        {
            try
            {
                var stagedBridges = await _context.Stage_BridgePrimaries
                    .Where(bridge => bridge.SubmittedBy == submittedBy)
                    .OrderByDescending(bridge => bridge.SubmitId)
                    .AsSplitQuery() // <--- This tells EF Core to split the query into separate SQL commands
                    .ToListAsync(cancellationToken);

                var dtos = stagedBridges.Select(bridge => new Stage_BridgePrimary
                {
                    SubmitId = bridge.SubmitId,
                    SubmittedBy = bridge.SubmittedBy,
                    StateCode_BL01 = bridge.StateCode_BL01,
                    BridgeNo_BID01 = bridge.BridgeNo_BID01,
                    BridgeName_BID02 = bridge.BridgeName_BID02,
                    Owner_BCL01 = bridge.Owner_BCL01,
                    CountyCode_BL02 = bridge.CountyCode_BL02,
                    BridgeLocation_BL11 = bridge.BridgeLocation_BL11
                }).ToList();

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching staged bridge DTOs");
                return new List<Stage_BridgePrimary>();
            }
        }


        internal async Task<List<ErrorSummary>> GetSubmittalErrorListByStateAsync(long submitId, CancellationToken cancellationToken = default)
        {
            try
            {
                var dtos = await GetErrorSummaryBySubmittal(submitId, cancellationToken);
                var submittalErrors = _mapper.Map<List<ErrorSummary>>(dtos);

                return submittalErrors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception fetching error summary list from the SubmittalErrors table");
                return new List<ErrorSummary>();
            }
        }

        public async Task<List<ErrorSummary>> GetErrorSummaryBySubmittal(long submitId, CancellationToken cancellationToken = default)
        {
            using var context = _contextFactory.CreateDbContext();

            var errorSummaries = await (from error in context.SubmittalErrors
                                        join lookup in context.Lookup_DataItems
                                        on error.ItemId equals lookup.NBI_Id into lookupGroup
                                        from lookup in lookupGroup.DefaultIfEmpty()
                                        where error.SubmitId == submitId && error.IsCorrected == false
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
                                            DataSet = error.DataSet,
                                            Comment = error.Comments ?? string.Empty
                                        })
                                    .ToListAsync(cancellationToken);

            return errorSummaries;
        }


        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingPrimaryAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgePrimaries
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("Primary", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);
            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }

        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingFeaturesAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgeFeatures
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("Feature", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }


        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingElementsAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgeElements
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("Element", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
               .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }


        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingInspectionsAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgeInspections
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("Inspection", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
             .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }


        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingPostingEvaluationsAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgePostingEvaluations
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("PostingEvaluation", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
                      .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);
            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }


        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingPostingStatusesAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgePostingStatuses
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("PostingStatus", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
                      .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }


        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingSpanSetsAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgeSpanSets
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("SpanSet", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
                      .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }


        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingWorksAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgeWorks
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("Work", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
                    .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }

        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingSubstructureSetsAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgeSubstructureSets
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("SubstructureSet", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
                      .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }

        internal async Task<(bool isSuccess, string errorMessage)> UpdateBridgeStagingRoutesAsync(long submitId, string itemId, string bridgeNumber, string submittedValue)
        {
            DateTime approvedDate = DateTime.Now;

            await using var ctx = await _contextFactory.CreateDbContextAsync();

            // Look up the correct column identifier
            var dataItem = await ctx.Lookup_DataItems
                .FirstOrDefaultAsync(x => x.NBI_Id == itemId);

            if (dataItem == null || string.IsNullOrEmpty(dataItem.Identifier))
            {
                return (false, "Data item not found.");
            }

            var identifier = dataItem.Identifier.Trim();

            // Get the staging record
            var stagingRecord = await ctx.Stage_BridgeRoutes
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo_BID01 == bridgeNumber);

            if (stagingRecord == null)
            {
                return (false, "Staging record not found.");
            }

            // Get the expected data type from the FieldTypeRegistry
            var fieldType = FieldTypeRegistry.GetFieldType("Route", dataItem.Identifier.Trim());

            if (fieldType == null)
            {
                throw new InvalidOperationException($"No field type found for the identifier {identifier}.");
            }

            var (isValid, errorMessage) = ValidateSubmittedValue(submittedValue, fieldType);

            if (!isValid)
            {
                return (false, errorMessage);
            }

            // Get column name and convert value
            var property = stagingRecord.GetType().GetProperties()
                      .FirstOrDefault(p => p.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (property == null)
            {
                return (false, $"Field '{identifier}' not found or is not writable on the staging record.");
            }

            if (!TrySetConvertedPropertyValue(stagingRecord, identifier, submittedValue, out var conversionError))
            {
                return (false, conversionError);
            }

            // Update Submittal Error to Is Corrected
            await MarkSubmittalErrorAsCorrectedAsync(submitId, bridgeNumber, itemId);

            await ctx.SaveChangesAsync();

            return (true, string.Empty);
        }


        // Helpers
        public async Task MarkSubmittalErrorAsCorrectedAsync(long submitId, string bridgeNumber, string itemId)
        {
            await using var ctx = await _contextFactory.CreateDbContextAsync();

            var submittalError = await ctx.SubmittalErrors
                .FirstOrDefaultAsync(x => x.SubmitId == submitId && x.BridgeNo == bridgeNumber && x.ItemId == itemId);

            if (submittalError != null)
            {
                submittalError.IsCorrected = true;
                await ctx.SaveChangesAsync();
            }
        }

        private bool IsValidDataType(string submittedValue, Type expectedType)
        {
            if (string.IsNullOrEmpty(submittedValue))
                return true; 

            try
            {
                if (expectedType == typeof(int))
                {
                    return int.TryParse(submittedValue, out _);
                }
                if (expectedType == typeof(double))
                {
                    return double.TryParse(submittedValue, out _);
                }
                if (expectedType == typeof(decimal))
                {
                    return decimal.TryParse(submittedValue, out _);
                }
                if (expectedType == typeof(DateOnly))
                {
                    return DateOnly.TryParse(submittedValue, out _);
                } 
                if (expectedType == typeof(DateTime))
                {
                    return DateTime.TryParse(submittedValue, out _);
                }
                if (expectedType == typeof(long))
                {
                    return long.TryParse(submittedValue, out _);
                }
                if (expectedType == typeof(string))
                {
                    return true; // Always valid for strings.
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private (bool isValid, string errorMessage) ValidateSubmittedValue(string submittedValue, Type fieldType)
        {
            var expectedType = Nullable.GetUnderlyingType(fieldType) ?? fieldType;
            bool isValid = IsValidDataType(submittedValue, expectedType);

            if (isValid)
            {
                return (true, string.Empty);
            }

            string baseMessage = $"The submitted value must be of type {expectedType.Name}. Please provide a valid value.";

            if (expectedType == typeof(DateTime) || expectedType == typeof(DateOnly))
            {
                baseMessage += " Expected format: MM/dd/yyyy.";
            }

            return (false, baseMessage);
        }

        private bool TrySetConvertedPropertyValue(object stagingRecord, string identifier, string submittedValue, out string errorMessage)
        {
            errorMessage = string.Empty;

            var property = stagingRecord.GetType().GetProperty(identifier);
            if (property == null || !property.CanWrite)
            {
                errorMessage = $"Property '{identifier}' not found or is not writable on the staging record.";
                return false;
            }

            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            try
            {
                if (propertyType == typeof(DateOnly))
                {
                    if (DateOnly.TryParse(submittedValue, out var dateOnlyValue))
                    {
                        property.SetValue(stagingRecord, dateOnlyValue);
                        return true;
                    }
                    else
                    {
                        errorMessage = $"Invalid date format for DateOnly. Expected format: MM/dd/yyyy.";
                        return false;
                    }
                }
                else if (propertyType == typeof(DateTime))
                {
                    if (DateTime.TryParse(submittedValue, out var dateTimeValue))
                    {
                        property.SetValue(stagingRecord, dateTimeValue);
                        return true;
                    }
                    else
                    {
                        errorMessage = $"Invalid date format for DateTime. Expected format: MM/dd/yyyy.";
                        return false;
                    }
                }
                else
                {
                    // General conversion for other types
                    var convertedValue = Convert.ChangeType(submittedValue, propertyType);
                    property.SetValue(stagingRecord, convertedValue);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error setting property '{identifier}': {ex.Message}";
                return false;
            }
        }

    }

}