namespace NBTIS.Web.Components.Pages
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.Configuration;
    using Microsoft.JSInterop;
    using NBTIS.Core.DTOs;
    using NBTIS.Core.Enums;
    using NBTIS.Core.Services;
    using NBTIS.Core.Settings;
    using NBTIS.Web.Services;
    using NBTIS.Web.ViewModels;
    using RulesEngine.Models;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http.Headers;
    using Telerik.Blazor.Components;
    using Telerik.Blazor.Components.Grid;

    public class DivisionApprovalsBase : ComponentBase
    {
        [Inject]
        private HttpClientService _httpClient { get; set; } = default!;
        [Inject]
        private IConfiguration _config { get; set; }
        [Inject]
        public ICurrentUserService CurrentUserService { get; set; }
        [Inject]
        private LocationService _locationService { get; set; }
        [Inject]
        private SubmittalService _submittalService { get; set; }
        [Inject]
        private FileValidationService _validationService { get; set; }
        [Inject]
        private IJSRuntime JS { get; set; }

        // Flag to control the visibility of the loading indicator
        public bool IsInitialDataLoadComplete { get; set; } = false;

        public string? SelectedState { get; set; }
        public string? SelectedYear { get; set; }

        public string? UploadedJsonContent { get; set; }
        public string? UploadErrorMessage { get; set; }

        protected EditContext? editContext;
        protected UploadFileInputForm? input;
        protected string _inputFileId = Guid.NewGuid().ToString();
        protected string successMessage = string.Empty;
        protected string errorMessage = string.Empty;
        protected bool hasJsonFormatError = false;
        protected string uploadDate = DateTime.Now.ToString();
        protected IList<string> fileNames = new List<string>();
        protected List<LocationDTO> locationList = new List<LocationDTO>();

        protected CancellationTokenSource cancelationTokenSource;

        protected int progressPercent;
        protected bool isLoading;
        protected bool displayProgress;


        private HubConnection _hubConnection;
        private string _connectionId;
        private TelerikGrid<SubmittalItem> GridRef { get; set; }



        protected void ClearSuccessMessage()
        {
            successMessage = string.Empty;
        }


        protected void ClearErrorMessage()
        {
            errorMessage = string.Empty;
        }


        // Kendo Grid data source
        public List<SubmittalItem> SubmittalsList { get; set; } = new List<SubmittalItem>();
        protected bool IsActionLoading { get; set; } = false;



        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            cancelationTokenSource = new CancellationTokenSource();
            input = new UploadFileInputForm();
            editContext = new EditContext(input);

            //await StartHubConnection();
            //SetRefreshDataListener();

            await SetLocationDropdown("state");

            await RefreshGridAsync();

            // Data is loaded—hide the loading indicator
            IsInitialDataLoadComplete = true;
        }

        protected void UpdateSelectedState(ChangeEventArgs e)
        {
            var selectedValue = e.Value?.ToString();
            if (!string.IsNullOrEmpty(selectedValue))
            {
                input.SubmittedBy = selectedValue;
            }
            else
            {
                input.SubmittedBy = null;
            }

            editContext.NotifyFieldChanged(FieldIdentifier.Create(() => input.SubmittedBy));
        }

        protected async Task SetStateAgencyAsync(string? value)
        {
            input.StateAgencyOption = value;
            input.SubmittedBy = null;
            await SetLocationDropdown(value);

            StateHasChanged();
        }

        private async Task SetLocationDropdown(string? option)
        {
            var response = await _locationService.GetLocationListAsync(option);

            if (response != null)
            {
                locationList = response;
            }
        }


        protected async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            // Clear previous messages
            UploadErrorMessage = null;
            UploadedJsonContent = null;

            try
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input), "Input object cannot be null.");
                }

                if (editContext == null)
                {
                    throw new ArgumentNullException(nameof(editContext), "EditContext cannot be null.");
                }

                var files = e.GetMultipleFiles();
                input.LoadedFiles = files.ToArray();
                fileNames.Clear();

                foreach (var f in files)
                {
                    decimal sizeInKilobytes = Math.Round((decimal)f.Size / 1024, 2); // Calculate size in KB and round to two decimal places
                    var fileInfo = $"{f.Name} ({sizeInKilobytes} KB)";

                    fileNames.Add(fileInfo);
                }

                editContext.NotifyFieldChanged(FieldIdentifier.Create(() => input.LoadedFiles));
            }
            catch (Exception ex)
            {
                UploadErrorMessage = $"Error reading file: {ex.Message}.";
            }
        }

        public async Task RefreshGridAsync()
        {
            var updatedList = await _submittalService.GetSubmittalListByStateAsync(cancelationTokenSource.Token);

            //SubmittalsList = new List<SubmittalItem>(updatedList);
            SubmittalsList = updatedList ?? new List<SubmittalItem>();

            if (GridRef != null)
            {
                 GridRef?.Rebind();
            }

            StateHasChanged();
        }


        protected async Task DownloadReport(SubmittalItem item)
        {
            if (item.ReportContent != null && item.ReportContent.Length > 0)
            {
                var fileName = $"ProcessingReport_{item.SubmittedBy}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                //// Copy the ReportContent to a new byte array (if needed; if ReportContent is already a byte array, you might use it directly)
                //var fileContent = new byte[item.ReportContent.Length];
                //Buffer.BlockCopy(item.ReportContent, 0, fileContent, 0, item.ReportContent.Length);

                using var stream = new MemoryStream(item.ReportContent);
                await using var fileRef = await JS.InvokeAsync<IJSObjectReference>("import", "./js/fileDownloader.js");
                await fileRef.InvokeVoidAsync("downloadFile", fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream.ToArray());
            }
        }

        protected async Task ConfirmAndApprove(long submitId, string submittalType, string stateAgencyName)
        {
            var comment = await ShowCommentPopup("approve");
            if (comment != null)
            {
                IsActionLoading = true;

                await ApproveItem(submitId, comment, submittalType, stateAgencyName);
                IsActionLoading = false;
                StateHasChanged();
            }
        }
        protected async Task ApproveItem(long submitId, string comment, string submittalType, string stateAgencyName)
        {
            // -------- Update Submission Status to "HQ Review" ----------
            var isPartial = submittalType == "Partial" ? true : false;
            successMessage = await _submittalService.DivisionApproveReturnAsync(submitId, (byte)SubmittalStatus.HQReview, comment, isPartial, stateAgencyName);

            await RefreshGridAsync();
        }

        protected async Task ConfirmAndReturn(long submitId, string submittalType, string stateAgencyName)
        {
            var comment = await ShowCommentPopup("return");
            if (comment != null)
            {
                IsActionLoading = true;

                await ReturnItem(submitId, comment, submittalType, stateAgencyName);
                IsActionLoading = false;
                StateHasChanged();
            }
        }
        protected async Task ReturnItem(long submitId, string comment, string submittalType, string stateAgencyName)
        {
            // -------- Update Submission Status to "Returned By Division" ----------
            var isPartial = submittalType == "Partial" ? true : false;
            successMessage = await _submittalService.DivisionApproveReturnAsync(submitId, (byte)SubmittalStatus.ReturnedByDivision, comment, isPartial, stateAgencyName);

            await RefreshGridAsync();
        }

        private async Task<string?> ShowCommentPopup(string action)
        {
            bool confirmed = await JS.InvokeAsync<bool>("confirm", new object[] { $"Do you want to {action} this submission?" });

            if (confirmed)
            {
                string comment = await JS.InvokeAsync<string>("prompt", $"Please provide a comment for the {action} (optional). Click OK:");

                return comment;
            }

            return null;  
        }

        protected async Task DownloadFile(SubmittalItem item)
        {
            if (item.FileContent != null && item.FileContent.Length > 0)
            {
                var fileName = $"BridgeInventory_{item.SubmittedBy}_{item.UploadType}_{item.UploadDate:yyyyMMddHHmmss}.json";
                var contentType = "application/octet-stream";
                var base64Data = Convert.ToBase64String(item.FileContent);

                using var stream = new MemoryStream(item.FileContent);
                await using var fileRef = await JS.InvokeAsync<IJSObjectReference>("import", "./js/fileDownloader.js");
                await fileRef.InvokeVoidAsync("downloadFile", fileName, contentType, stream.ToArray());
            }
            else
            {
                await JS.InvokeVoidAsync("alert", "The requested file was not found.");
            }
        }
    }
}


