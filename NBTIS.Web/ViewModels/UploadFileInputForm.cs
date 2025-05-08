using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace NBTIS.Web.ViewModels
{
    public class UploadFileInputForm
    {
        public string? StateAgencyOption { get; set; } = "state";

        [Required(ErrorMessage = "Please select a type of submittal.")]
        public string? FullPartialOption { get; set; }  

        [Required(ErrorMessage = "Please select a state or agency.")]
        public string? SubmittedBy { get; set; }

        [Required(ErrorMessage = "Input file is required.")]
        public IBrowserFile[]? LoadedFiles { get; set; }

        public string? Comments { get; set; }
    }
}
