using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NBTIS.Web.ViewModels;
using Telerik.Blazor.Components;
using Telerik.SvgIcons;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using NBTIS.Core.DTOs;
using NBTIS.Core.Services;
using NBTIS.Core.Settings;
using NBTIS.Web.Services;
using RulesEngine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using Telerik.Blazor.Components.Grid;
using System.Threading;
using ErrorSummary = NBTIS.Web.ViewModels.ErrorSummary;

namespace NBTIS.Web.Components.Pages
{
    public class DataCorrectionBase : ComponentBase
    {
        [Inject]
        public ICurrentUserService CurrentUserService { get; set; }
        protected string successMessage = string.Empty;
        protected string errorMessage = string.Empty;

        public List<ErrorSummary> SubmittalsList { get; set; } = new List<ErrorSummary>();
        public bool IsInitialDataLoadComplete { get; set; } = false;
        private IJSRuntime JS { get; set; }
        protected CancellationTokenSource cancelationTokenSource;
        [Inject]
        private DataCorrectionService _dataCorrectionService { get; set; }

        protected EditContext? editContext;
        protected UploadFileInputForm? input;
        protected TelerikGrid<ErrorSummary> GridRef { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public long submitId { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public string submittedByDescription { get; set; } = string.Empty;

        [Parameter, SupplyParameterFromQuery]
        public string submittedBy { get; set; } = string.Empty;

        protected void ClearSuccessMessage()
        {
            successMessage = string.Empty;
        }
        protected void ClearErrorMessage()
        {
            errorMessage = string.Empty;
        }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            cancelationTokenSource = new CancellationTokenSource();
            input = new UploadFileInputForm();
            editContext = new EditContext(input);

            await RefreshGridAsync();
            IsInitialDataLoadComplete = true;
        }

        public async Task RefreshGridAsync()
        {
            var updatedList = await _dataCorrectionService.GetSubmittalErrorListByStateAsync(submitId, cancelationTokenSource.Token);
            SubmittalsList = updatedList ?? new List<ErrorSummary>();

            if (GridRef != null)
            {
                GridRef?.Rebind();
            }

            StateHasChanged();
        }

    }
}
