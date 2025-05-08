using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using NBTIS.Core.DTOs;
using NBTIS.Core.Enums;
using NBTIS.Core.Exceptions;
using NBTIS.Core.Services;
using NBTIS.Core.Settings;
using NBTIS.Data.Models;
using NBTIS.Web.Services;
using NBTIS.Web.ViewModels;
using RulesEngine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Blazor.Components;
using Telerik.SvgIcons;
using static System.Net.WebRequestMethods;


namespace NBTIS.Web.Components.Pages
{
    public class SubmissionStatusBase : ComponentBase
    {
        [Inject]
        private SubmittalService _submittalService { get; set; }

        [Inject]
        public IWebHostEnvironment Env { get; set; }
        [Inject]
        private IConfiguration _config { get; set; }
        [Inject]
        public ICurrentUserService CurrentUserService { get; set; }
        [Inject]
        private LocationService _locationService { get; set; }

        [Inject]
        private FileValidationService _validationService { get; set; }
        [Inject]
        private BridgeStagingLoaderService _bridgeStagingLoaderService { get; set; }
        [Inject]
        private IJSRuntime JS { get; set; }



        public List<SubmissioniStatusItemViewModel> OriginalSubmittalsList { get; set; }
        public List<SubmissioniStatusItemViewModel> SubmittalsList = new();
        public string successMessage = string.Empty;
        public string errorMessage = string.Empty;

        // New: Loading state management
        public bool IsInitialDataLoadComplete = false;

        public IEnumerable<object> checkedItems { get; set; } = new List<object>();


        // -----------------------------
        // 2) FLAT DATA FOR THE TREE
        // -----------------------------
        public List<TreeItem> FlatData { get; set; }
        public IEnumerable<object> ExpandedItems { get; set; } = new List<TreeItem>();

        // -----------------------------
        // 3) TREE CHECKED ITEMS
        // -----------------------------
        private IEnumerable<object> _checkedItems = new List<object>();

        #region ShowComments_AcceptREject

        //ToShowComments
        public bool ShowCommentDialog { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public string DialogMode { get; set; } = ""; // "Accept" or "Reject"
        public SubmissioniStatusItemViewModel SelectedItem { get; set; }
        public bool IsCommentRequired => DialogMode == "Reject";

        public TelerikDialog DialogRef;

        protected void OpenAcceptDialog(SubmissioniStatusItemViewModel item)
        {
            errorMessage = string.Empty;
            SelectedItem = item;
            DialogMode = "Accept";
            CommentText = string.Empty;
            ShowCommentDialog = true;
        }

        protected void OpenRejectDialog(SubmissioniStatusItemViewModel item)
        {
            errorMessage = string.Empty;
            SelectedItem = item;
            DialogMode = "Reject";
            CommentText = string.Empty;
            ShowCommentDialog = true;
        }


        protected async Task SubmitCommentAsync()
        {
            errorMessage = "";
            //if (IsCommentRequired && string.IsNullOrWhiteSpace(CommentText))
            //{
            //    errorMessage = "Comment is required for rejection.";
            //    DialogRef.Refresh();                           
            //    return;
            //}

            if (CommentText?.Length > 500)
            {
                errorMessage = "Comment must be 500 characters or fewer.";
                DialogRef.Refresh();
                return;
            }

            // Confirm before changing UI state
            bool confirmed = await JS.InvokeAsync<bool>("confirm", $"Are you sure you want to {DialogMode.ToUpper()} this submittal?");


            if (!confirmed)
                return;

            IsInitialDataLoadComplete = false;
            StateHasChanged();
            await Task.Yield();

            var status = DialogMode == "Accept" ? SubmittalStatus.Accepted : SubmittalStatus.Rejected;
            await _submittalService.AcceptRejectAsync((long)SelectedItem.SubmitId, (byte)status, CommentText, (bool)SelectedItem?.IsPartial, SelectedItem.Lookup_States_Description);

            UpdateSubmittalStatusInList((long)SelectedItem.SubmitId, DialogMode == "Accept" ? "Accepted" : "Rejected", (byte)status);
            SubmittalsList = SubmittalsList.ToList();

            successMessage = $"{SelectedItem.Lookup_States_Description} submission has been {DialogMode.ToUpper()}ED.";

            // Close after everything is done
            ShowCommentDialog = false;
            IsInitialDataLoadComplete = true;
        }



        //protected async Task ConfirmAndAccept(SubmissioniStatusItemViewModel item)
        //{
        //    ClearMessages();

        //    bool confirmed = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to ACCEPT?");
        //    if (!confirmed)
        //        return;

        //    IsInitialDataLoadComplete = false;
        //    StateHasChanged();

        //    // 2) Yield so the UI thread can actually process that change
        //    await Task.Yield();

        //    await _submittalService.Update_SubmissionStatus((long)item.SubmitId, (byte)SubmittalStatus.Accepted);
        //    UpdateSubmittalStatusInList((long)item.SubmitId, "Accepted", 6);

        //    SubmittalsList = SubmittalsList.ToList();

        //    IsInitialDataLoadComplete = true;
        //    successMessage = $"{item.LookupStatesDescription} submission has been ACCEPTED.";

        //    StateHasChanged();
        //}

        //protected async Task CheckBox_Click(long submitId)
        //{
        //    await JS.InvokeVoidAsync("alert", "Work in Progress.");
        //}


        //protected async Task ConfirmAndReject(SubmissioniStatusItemViewModel item)
        //{
        //    ClearMessages();

        //    bool confirmed = await JS.InvokeAsync<bool>("confirm", new object[] { "Are you sure you want to REJECT?" });

        //    if (confirmed)
        //    {
        //        IsInitialDataLoadComplete = false;
        //        StateHasChanged();
        //        await _submittalService.Update_SubmissionStatus((long)item.SubmitId, (byte)SubmittalStatus.Rejected);
        //        UpdateSubmittalStatusInList((long)item.SubmitId, "Rejected", 7);
        //        IsInitialDataLoadComplete = true;
        //        SubmittalsList = SubmittalsList.ToList();
        //        successMessage = $"{item.LookupStatesDescription} submission has been REJECTED.";
        //        StateHasChanged();

        //    }

        //}

        #endregion



        // -----------------------------
        // 6) APPLY FILTER BUTTON
        // -----------------------------
        public void ApplyFilter()
        {
            //Get only leaf nodes => those with a non-null ParentId
            var selectedFilters = checkedItems
          .OfType<TreeItem>()
          .Where(x => x.ParentId.HasValue)
          .Select(x => x.Text)
          .ToList();

            SubmittalsList = selectedFilters.Any()
                ? OriginalSubmittalsList
                    .Where(item => selectedFilters.Contains(item.Lookup_States_Description))
                    .ToList()
                : OriginalSubmittalsList;

            StateHasChanged();
        }

        protected void ClearSuccessMessage()
        {
            successMessage = string.Empty;
        }




        private void OpenCommentsPopup()
        {
            successMessage = "Comments button clicked.";
        }
        private void ClearMessages()
        {
            successMessage = string.Empty;
            errorMessage = string.Empty;
            //infoMessage = string.Empty;
            //hasJsonFormatError = false;
        }
       
        private void UpdateSubmittalStatusInList(long submitId, string newStatus, byte statusCode)
        {
            // Update in OriginalSubmittalsList
            var originalItem = OriginalSubmittalsList.FirstOrDefault(x => x.SubmitId == submitId);
            if (originalItem != null)
            {
                originalItem.ApproveRejectDate = DateTime.Now;
                originalItem.Lookup_Statuses_StatusDescription = newStatus;
                originalItem.StatusCode = statusCode;

            }

            // Update in filtered list
            var filteredItem = SubmittalsList.FirstOrDefault(x => x.SubmitId == submitId);
            if (filteredItem != null)
            {
                filteredItem.ApproveRejectDate = DateTime.Now;
                filteredItem.Lookup_Statuses_StatusDescription = newStatus;
                filteredItem.StatusCode = statusCode;
            }
               
        }
        protected async Task DownloadReport(SubmissioniStatusItemViewModel itemVM)
        {
            var item = await _submittalService.Load_SubmittalReport(Convert.ToInt64(itemVM.SubmitId));
            if (item.ReportContent != null && item.ReportContent.Length > 0)
            {
                var fileName = $"ProcessingReport_{item.SubmittedBy}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                using var stream = new MemoryStream(item.ReportContent);
                await using var fileRef = await JS.InvokeAsync<IJSObjectReference>("import", "./js/fileDownloader.js");
                await fileRef.InvokeVoidAsync("downloadFile", fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream.ToArray());
            }
        }
        protected async Task DownloadFile(SubmissioniStatusItemViewModel incomingModel)
        {
            var item = await _submittalService.Load_SubmittalFile(Convert.ToInt64(incomingModel.SubmitId));
            if (item !=null &&  item.FileContent != null && item.FileContent.Length > 0)
            {
                var fileName = $"BridgeInventory_{incomingModel.SubmittedBy}_{(incomingModel?.IsPartial==true ? "Partial" : "Full")}_{incomingModel?.UploadDate:yyyyMMddHHmmss}.json";
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

        private void RejectSubmission()
        {
            errorMessage = "Submission rejected.";
        }

        protected CancellationTokenSource cancelationTokenSource;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                cancelationTokenSource = new CancellationTokenSource();
                IsInitialDataLoadComplete = false; // Start loading
                OriginalSubmittalsList = await _submittalService.Load_SubmissionStatus();

                SubmittalsList = OriginalSubmittalsList;
                // Get lookup data for states and agencies
                var (lookupStates, lookupAgencies) = await _submittalService.GetLookupDataAsync(cancelationTokenSource.Token);

                // Call the async method to load the flat data
                await LoadFlatDataAsync(lookupStates, lookupAgencies);

                // Expand all nodes that have children
                ExpandedItems = FlatData.Where(x => x.HasChildren).ToList();
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading submission status: {ex.Message}";
            }
            finally
            {
                IsInitialDataLoadComplete = true; // End loading
            }
        }


        private async Task LoadFlatDataAsync(List<Lookup_State> lookupStates, List<LookupValue> lookupAgencies)
        {
            // Initialize the FlatData list
            FlatData = new List<TreeItem>();

            // Add the root nodes for States and Agencies
            FlatData.Add(new TreeItem { Id = 1, Text = "All States", ParentId = null, HasChildren = true });
            FlatData.Add(new TreeItem { Id = 2, Text = "All Agencies", ParentId = null, HasChildren = true });

            int stateId = 100; // Start ID for states
            int agencyId = 200; // Start ID for agencies

            try
            {
                // Order lookup states by Description (ascending) and add them as children of "All States"
                foreach (var state in lookupStates.OrderBy(s => s.Description))
                {
                    stateId++; // Increment the ID for each state
                    FlatData.Add(new TreeItem
                    {
                        Id = stateId,
                        Text = state.Description ?? "Unknown State",
                        ParentId = 1, // Parent is "All States"
                        HasChildren = false
                    });
                }

                // Order lookup agencies by Description (ascending) and add them as children of "All Agencies"
                foreach (var agency in lookupAgencies.OrderBy(a => a.Description))
                {
                    agencyId++; // Increment the ID for each agency
                    FlatData.Add(new TreeItem
                    {
                        Id = agencyId,
                        Text = agency.Description ?? "Unknown Agency",
                        ParentId = 2, // Parent is "All Agencies"
                        HasChildren = false
                    });
                }

                // Log for debugging
                Console.WriteLine($"Loaded {lookupStates.Count} states and {lookupAgencies.Count} agencies.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading flat data asynchronously: {ex.Message}");
            }
        }


        public async Task ExportToExcel()
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.AddWorksheet("Submittal Status");

                // Adding headers
                worksheet.Cell(1, 1).Value = "State or Agency";
                worksheet.Cell(1, 2).Value = "Status";
                worksheet.Cell(1, 3).Value = "Submitted Date";
                worksheet.Cell(1, 4).Value = "DBE Review Date";
                worksheet.Cell(1, 5).Value = "HQ Accept/Reject Date";

                // Populating data
                for (int i = 0; i < SubmittalsList.Count; i++)
                {
                    var item = SubmittalsList[i];
                    worksheet.Cell(i + 2, 1).Value = item.Lookup_States_Description;
                    worksheet.Cell(i + 2, 2).Value = item.Lookup_Statuses_StatusDescription;
                    worksheet.Cell(i + 2, 3).Value = item.SubmitDate?.ToString("MM/dd/yyyy");
                    worksheet.Cell(i + 2, 4).Value = item.ReviewDate?.ToString("MM/dd/yyyy");
                    worksheet.Cell(i + 2, 5).Value = item.ApproveRejectDate?.ToString("MM/dd/yyyy");
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                // Reset the stream position to the beginning before reading
                stream.Position = 0;

                var content = stream.ToArray();
                var base64Data = Convert.ToBase64String(content);
                var fileName = $"SubmittalStatus_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var contentType = "application/octet-stream";
                // Load the JS module and call the function
                await using var fileRef = await JS.InvokeAsync<IJSObjectReference>("import", "./js/fileDownloader.js");
                await fileRef.InvokeVoidAsync("downloadFile", fileName, contentType, content);
           
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during Excel export: {ex.Message}");
            }
        }

        #region commentsPopup

        // For Comments Popup
        public bool ShowCommentsPopup { get; set; } = false;
        public SubmissioniStatusItemViewModel SelectedCommentsItem { get; set; } = new SubmissioniStatusItemViewModel();

        protected void OpenCommentsPopup(SubmissioniStatusItemViewModel item)
        {
            SelectedCommentsItem = item;
            ShowCommentsPopup = true;
            //StateHasChanged();
        }

        public void VisibleChangedHandler(bool currVisible)
        {
            // After user adds/updates/deletes comment, you may refresh grid or close popup
            ShowCommentsPopup = false;
            StateHasChanged();
        }

        public async Task RefreshAfterComment()
        {
            // After user adds/updates/deletes comment, you may refresh grid or close popup
            ShowCommentsPopup = false;
            StateHasChanged();
        }
        #endregion





    }





}
