using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NBTIS.Web.Services;
using NBTIS.Web.ViewModels;

namespace NBTIS.Web.Components.PageComponents
{
    public partial class DataCorrectionEdit_PopUp : ComponentBase
    {
        [Inject]
        private DataCorrectionService _dataCorrectionService { get; set; }

        [Parameter]
        public ErrorSummary? EditItem { get; set; }

        [Parameter]
        [SupplyParameterFromQuery]
        public long SubmitId { get; set; }

        [Parameter]
        public string? CurrentUser { get; set; } // Identifier for the current user

        [Parameter]
        public EventCallback OnValidSubmit { get; set; }

        [Parameter]
        public EventCallback OnCancel { get; set; }

        [Parameter]
        public EventCallback OnSaveSuccess { get; set; }

        [Parameter]
        public EventCallback<string> OnSaveFailure { get; set; }


        [Parameter]
        public EventCallback<bool> OnClose { get; set; }

        private string errorMessage;
        private EditContext editContext;
        private ValidationMessageStore messageStore;



    }
}
