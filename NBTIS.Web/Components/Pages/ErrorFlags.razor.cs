using Microsoft.AspNetCore.Components;
using NBTIS.Core.DTOs;
using NBTIS.Web.Services;

namespace NBTIS.Web.Components.Pages
{
    public class ErrorFlagsBase: ComponentBase
    {
        [Inject]
        protected AdministrationService _administrationService { get; set; }

        [Inject]
        private LocationService _locationService { get; set; }
        protected List<LocationDTO> locationList = new List<LocationDTO>();

        protected string successMessage = string.Empty;


        protected override async Task OnInitializedAsync()
        {
            locationList = await SetLocationDropdown("state"); //state param only?
        }

        protected void ClearSuccessMessage()
        {
            successMessage = string.Empty;
        }

        protected async Task<List<LocationDTO>> SetLocationDropdown(string? option)
        {
            var response = await _locationService.GetLocationListAsync(option);
            return response ?? new List<LocationDTO>();
        }



    }
}
