using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models;
using Microsoft.JSInterop;
using NBTIS.Core.DTOs;
using NBTIS.Core.Enums;
using NBTIS.Core.Exceptions;
using NBTIS.Core.Services;
using NBTIS.Core.Settings;
using NBTIS.Web.Services;
using NBTIS.Web.ViewModels;
using RulesEngine.Models;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Blazor;
using Telerik.SvgIcons;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;
using static NBTIS.Web.Services.SubmittalService;
using static System.Net.WebRequestMethods;

public class SubmittalsBase : ComponentBase
{
    [Inject]
    public IWebHostEnvironment Env { get; set; } = null!;
    [Inject]
    private IConfiguration _config { get; set; } = null!;
    [Inject]
    public ICurrentUserService CurrentUserService { get; set; } = null!;
    [Inject]
    private LocationService _locationService { get; set; } = null!;
    [Inject]
    private SubmittalService _submittalService { get; set; } = null!;
    [Inject]
    private FileValidationService _validationService { get; set; } = null!;
    [Inject]
    private BridgeStagingLoaderService _bridgeStagingLoaderService { get; set;} = null!;
    [Inject]
    private IJSRuntime JS { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    // Telerik Dialogs are injected as a cascading parameter.
    [CascadingParameter]
    private DialogFactory Dialogs { get; set; } = null!;

    // Flag to control the visibility of the loading indicator
    public bool IsDataLoadComplete { get; set; } = false;

    public string? SelectedState { get; set; }
    public string? SelectedYear { get; set; }

    public string? UploadedJsonContent { get; set; }
    public string? UploadErrorMessage { get; set; }

    protected EditContext? editContext;
    protected UploadFileInputForm? input;
    protected string _inputFileId = Guid.NewGuid().ToString();
    protected string _uploadTempPath;
    protected string successMessage = string.Empty;
    protected string infoMessage = string.Empty;
    protected string errorMessage = string.Empty;
    protected bool hasJsonFormatError = false;
    protected DateTime uploadDate = DateTime.Now;
    protected IList<string> fileNames = new List<string>();
    protected List<LocationDTO> locationList = new List<LocationDTO>();
    
    protected CancellationTokenSource cancelationTokenSource;
    
    protected int progressPercent;  
    protected bool isLoading;
    protected bool displayProgress;

    private HubConnection _hubConnection;
    private string _connectionId;

    protected bool _isUploadInProgress;

    // Kendo Grid data source
    public List<SubmittalItem> SubmittalsList { get; set; } = new List<SubmittalItem>();



    protected override async Task OnInitializedAsync()
    {
        base.OnInitialized();
        cancelationTokenSource = new CancellationTokenSource();
        input = new UploadFileInputForm();
        editContext = new EditContext(input);
        _uploadTempPath = Path.Combine(Env.ContentRootPath, "temp");

        await StartHubConnection();
        SetRefreshDataListener();

        await SetLocationDropdown("state");

        await RefreshGridAsync();

        // Data is loaded—hide the loading indicator
        IsDataLoadComplete = true;
    }

    private async Task StartHubConnection()
    {
        var hubUrl = NavigationManager.ToAbsoluteUri("/progressHub"); // Converts relative URL to full one

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();

        _hubConnection.Closed += async (error) =>
        {
            Console.WriteLine("SignalR connection closed. Reconnecting...");
            await Task.Delay(2000);
            await _hubConnection.StartAsync();
        };

        await _hubConnection.StartAsync();

        _connectionId = _hubConnection.ConnectionId!;
        Console.WriteLine($"SignalR Connected. ID: {_connectionId}");
    }

    private void SetRefreshDataListener()
    {
        _hubConnection.On<int>("GetPercentComplete", (percent) =>
        {
            progressPercent = percent;
            displayProgress = true;
            //isLoading = true;
            Console.WriteLine($"[SignalR] Progress: {percent}%");
            InvokeAsync(StateHasChanged);
        });
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

    protected async Task OnUpload()
    {
        /* If an upload is still winding down (success or cancel) – stop */
        if (_isUploadInProgress)
        {
            infoMessage = "An upload is already in progress. Please wait for it to finish or cancel.";
            StateHasChanged();
            return;
        }

        _isUploadInProgress = true;
        cancelationTokenSource?.Cancel();
        cancelationTokenSource?.Dispose();
        cancelationTokenSource = new CancellationTokenSource();

        ClearMessages();
        ResetUploadState();

        if (input?.SubmittedBy == null)
        {
            errorMessage = "No submittedBy selected.";
            CleanupUpload();
            return;
        }
        if (input?.LoadedFiles == null || !input.LoadedFiles.Any())
        {
            errorMessage = "No file selected.";
            CleanupUpload();
            return;
        }

        var file = input.LoadedFiles.First();
        try
        {
            UpdateProgress(5);
            using var response = await ProcessSubmittalAsync(
                file,
                input.SubmittedBy!,
                uploadDate,
                input.Comments,
                _connectionId,
                cancelationTokenSource.Token
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorContent);
            }

            await RefreshGridAsync();
            successMessage = "File uploaded and validated successfully.";
            UpdateProgress(100);
        }
        catch (OperationCanceledException)
        {
            // The user hit Cancel
            infoMessage = "Upload was cancelled.";
        }
        catch (NoNbiBridgesException ex) { errorMessage = ex.Message; }
        catch (StateMismatchException ex) { errorMessage = ex.Message; }
        catch (Exception ex) { errorMessage = $"Unexpected error: {ex.Message}"; }
        finally
        {
            CleanupUpload();
            _isUploadInProgress = false;
            StateHasChanged();                // refresh UI (button re‑enabled)
        }
    }

    private async Task<HttpResponseMessage> ProcessSubmittalAsync(
        IBrowserFile file,
        string selectedState,
        DateTime uploadDate,
        string? comments,
        string connectionId,
        CancellationToken token)
    {
        const int chunkSize = 20 * 1024 * 1024; // 20 MB
        int chunkNumber = 0;
        string fileName = file.Name;
        string fullPartial = input.FullPartialOption ?? "Partial";

        // generate a unique ID for this session
        var uploadGuid = Guid.NewGuid();
        string uploadId = uploadGuid.ToString();

        long maxAllowedSize = _config.GetValue<long>("FileUploadSettings:MaxAllowedSize");
        // pass the token here
        using var stream = file.OpenReadStream(maxAllowedSize, token);

        var buffer = new byte[chunkSize];
        int bytesRead;

        // 1️. upload in chunks
        while (true)
        {
            token.ThrowIfCancellationRequested();

            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            if (bytesRead == 0)
                break;

            using var chunkStream = new MemoryStream(buffer, 0, bytesRead);
            var formFile = new FormFile(chunkStream, 0, bytesRead, fileName, fileName);

            // ← propagate token
            await _submittalService.UploadChunkAsync(
                formFile,
                fileName,
                uploadId,
                chunkNumber,
                token
            );

            chunkNumber++;
        }
        UpdateProgress(10);

        // 2️. insert the log row
        token.ThrowIfCancellationRequested();
        long submitId = await _submittalService.InsertSubmittalRecordAsync(
            fileName,
            uploadGuid,
            selectedState,
            fullPartial,
            comments,
            token
        );
        UpdateProgress(20);

        // 3️. validate
        token.ThrowIfCancellationRequested();
        var tempFileName = Path.Combine(_uploadTempPath, $"{uploadId}_{fileName}.tmp");
        var processResult = await _validationService.ValidateFileAsync(
            tempFileName,
            uploadId,
            uploadDate,
            submitId,
            selectedState,
            connectionId,
            revalidate: false,
            token
        );
        UpdateProgress(80);

        // 4️. staging
        token.ThrowIfCancellationRequested();
        if (processResult.StagingData?.Any() == true)
        {
            await _bridgeStagingLoaderService.PopulateStageAsync(
                submitId,
                selectedState,
                processResult.StagingData,
                token
            );
        }
        UpdateProgress(85);

        // 5️. generate Excel report
        token.ThrowIfCancellationRequested();
        string tempExcelFileName = await _validationService.GenerateExcelReportAsync(
            uploadDate,
            processResult,
            token
        );
        UpdateProgress(95);

        // 6️. write report back to DB
        token.ThrowIfCancellationRequested();
        byte[] excelContent = await System.IO.File.ReadAllBytesAsync(tempExcelFileName, token);
        await _submittalService.UpdateProcessingReportContentAsync(
            submitId,
            excelContent,
            token
        );
        UpdateProgress(99);

        // 7️. cleanup
        _submittalService.CleanupTempFiles(fileName, uploadId);

        // 8️. return the Excel file as an HTTP response
        var excelStream = new FileStream(
            tempExcelFileName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(excelStream)
        };
        response.Content.Headers.ContentType =
            new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        response.Content.Headers.ContentDisposition =
            new ContentDispositionHeaderValue("attachment")
            {
                FileName = _submittalService.SetExcelFileName(
                    uploadDate.ToString("yyyyMMdd"),
                    selectedState,
                    fullPartial

                )
            };

        return response;
    }

    private void UpdateProgress(int percent)
    {
        progressPercent = percent;
        StateHasChanged();
    }


    protected void CancelUpload()
    {
        fileNames.Clear();
        progressPercent = 0;
        displayProgress = false;
        input.SubmittedBy = null;
        editContext = new EditContext(input);
        ClearMessages();
        if (cancelationTokenSource?.IsCancellationRequested == false)
            cancelationTokenSource.Cancel();
    }

    protected void ClearSuccessMessage()
    {
        successMessage = string.Empty;
    }

    protected void ClearErrorMessage()
    {
        errorMessage = string.Empty;
    }

    protected void ClearInfoMessage()
    {
        infoMessage = string.Empty;
    }

    private void ClearMessages()
    {
        successMessage = string.Empty;
        errorMessage = string.Empty;
        infoMessage = string.Empty;
        hasJsonFormatError = false;
    }

    private void ResetUploadState()
    {
        isLoading = true;
        displayProgress = true;
        UpdateProgress(0);
    }

    private void CleanupUpload()
    {
        isLoading = false;
        displayProgress = false;
        // Any additional cleanup can go here
    }

   

    // **** Grid-related actions
    private async Task RefreshGridAsync()
    {
        SubmittalsList = await _submittalService.GetSubmittalListAsync(cancelationTokenSource.Token);
        StateHasChanged();
    }

    protected async Task DownloadFile(SubmittalItem item)
    {
        if (item.FileContent != null && item.FileContent.Length > 0)
        {
            var fileName = $"BridgeInventory_{item.SubmittedBy}_{item.UploadType}_{item.UploadDate:yyyyMMddHHmmss}.json";
            var contentType = "application/octet-stream";
            var base64Data = System.Convert.ToBase64String(item.FileContent);

            using var stream = new MemoryStream(item.FileContent);
            await using var fileRef = await JS.InvokeAsync<IJSObjectReference>("import", "./js/fileDownloader.js");
            await fileRef.InvokeVoidAsync("downloadFile", fileName, contentType, stream.ToArray());
        }
        else
        {
            await JS.InvokeVoidAsync("alert", "The requested file was not found.");
        }
    }

    protected async Task DownloadReport(SubmittalItem itemVM)
    {
        var item = await _submittalService.Load_SubmittalReport(itemVM.SubmitId, cancelationTokenSource.Token);
        var fullPartial = item.IsPartial ? "Partial" : "Full";

        if (item?.ReportContent is { Length: > 0 })
        {
            var fileName = $"ProcessingReport_{item.SubmittedBy}_{fullPartial}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            using var stream = new MemoryStream(item.ReportContent);
            await using var fileRef = await JS.InvokeAsync<IJSObjectReference>("import", "./js/fileDownloader.js");
            await fileRef.InvokeVoidAsync("downloadFile", fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream.ToArray());
        }
    }

    protected async Task ConfirmAndSubmit(SubmittalItem item)
    {
        ClearMessages();

        // Step 1: Confirm user intent
        if (!await JS.InvokeAsync<bool>("confirm", "Do you want to submit this file?"))
            return;

        // Step 2: Check if submission type is partial and no data exists
        if (item.UploadType.Equals("Partial", StringComparison.OrdinalIgnoreCase) &&
            _submittalService.GetBridgeInventoryCount(item.SubmittedBy) == 0 &&
            _submittalService.GetStageCount(item.SubmittedBy) == 0)
        {
            infoMessage = $"This is an initial submission for {item.SubmittedByDescription}. " +
                          "Please submit a FULL bridge inventory first.";
            StateHasChanged();
            return;
        }

        // Step 3: Parse UploadType to enum
        if (!Enum.TryParse<SubmittalType>(item.UploadType, true, out var submittalType))
        {
            errorMessage = $"Unknown UploadType '{item.UploadType}'.";
            StateHasChanged();
            return;
        }

        // Step 4: Evaluate submission eligibility
        (bool canSubmit, string action, long? prevId, string confirmMsg) evalResult;
        try
        {
            evalResult = await _submittalService.EvaluateSubmissionAsync(
                item.SubmittedBy, item.SubmittedByDescription, submittalType, cancelationTokenSource.Token);
        }
        catch (Exception ex)
        {
            errorMessage = $"Evaluation failed: {ex.Message}";
            StateHasChanged();
            return;
        }

        if (!evalResult.canSubmit)
        {
            infoMessage = evalResult.action == "HQReview"
                ? "Cannot submit because another submission for the same state or agency is in HQ Review."
                : "Submission not allowed.";
            StateHasChanged();
            return;
        }

        // Step 5: Show custom confirmation message, if provided
        if (!string.IsNullOrEmpty(evalResult.confirmMsg))
        {
            if (!await PromptUser(evalResult.confirmMsg))
                return;
        }

        // Step 6: Submit to Division
        IsDataLoadComplete = false;
        StateHasChanged();

        try
        {
            await _submittalService.SubmitToDivisionAsync(item.SubmitId, evalResult.prevId ?? 0, evalResult.action, item.SubmittedByDescription);
            await RefreshGridAsync();

            successMessage = $"{item.SubmittedByDescription} submission completed successfully.";
        }
        catch (OperationCanceledException)
        {
            infoMessage = "Submission was cancelled.";
        }
        catch (Exception ex)
        {
            errorMessage = $"Submission failed: {ex.Message}";
        }
        finally
        {
            IsDataLoadComplete = true;
            StateHasChanged();
        }
    }

    protected async Task DeleteItem(long submitId, string submittedByDescription)
    {
        bool result = await _submittalService.DeleteSubmittalRecordAsync(submitId);
        successMessage = $"{submittedByDescription} submission deleted successfully.";
        await RefreshGridAsync();
    }

    protected async Task ConfirmAndDelete(long submitId, string submitedByDescription)
    {
        ClearMessages();

        bool confirmed = await JS.InvokeAsync<bool>("confirm", new object[] { "Do you want to delete this submission?" });
        if (confirmed)
        {
            // Set flag to show the loader
            IsDataLoadComplete = false;
            StateHasChanged(); 

            // Optionally, yield to the UI thread so the change can render
            await Task.Yield();

            // Perform deletion and grid refresh
            await DeleteItem(submitId, submitedByDescription);

            // Hide the loader after operation completes
            IsDataLoadComplete = true;
            StateHasChanged(); 
        }
    }

    protected async Task ConfirmAndCancel(long submitId, string submitedByDescription)
    {
        ClearMessages();

        bool confirmed = await JS.InvokeAsync<bool>("confirm", new object[] { "Do you want to cancel this submission?" });
        if (confirmed)
        {
            // Set flag to show the loader
            IsDataLoadComplete = false;
            StateHasChanged();

            // Optionally, yield to the UI thread so the change can render
            await Task.Yield();

            // Perform deletion and grid refresh
            await CancelSubmittal(submitId, submitedByDescription);

            // Hide the loader after operation completes
            IsDataLoadComplete = true;
            StateHasChanged();
        }
    }

    public async Task CancelSubmittal(long submitId, string submittedByDescription)
    {
        bool result = await _submittalService.CancelSubmittalAsync(submitId);
        successMessage = $"{submittedByDescription} submission canceled successfully.";
        await RefreshGridAsync();
    }

    protected Task OpenCorrectionPage(long submitId, string submittedBy, string submittedByDescription)
    {
        // Construct the URL for the RelatedSites page. Use Uri.EscapeDataString for safe query string encoding.
        var url = $"/datacorrection?submitId={submitId}&submittedBy={Uri.EscapeDataString(submittedBy)}&submittedByDescription={Uri.EscapeDataString(submittedByDescription)}";
        NavigationManager.NavigateTo(url);
        return Task.CompletedTask;
    }

   
    // Updated PromptUser method using Telerik Dialog's ConfirmAsync.
    public async Task<bool> PromptUser(string message)
    {
        // Customize the title and button texts as appropriate.
        bool isConfirmed = await Dialogs.ConfirmAsync(
            message,
            "Please Confirm",
            "Yes, Proceed",
            "Cancel");

        return isConfirmed;
    }

}

