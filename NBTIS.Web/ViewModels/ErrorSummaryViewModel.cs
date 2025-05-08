using System.ComponentModel.DataAnnotations;

namespace NBTIS.Web.ViewModels
{
    public class ErrorSummary
    {
        [Required(ErrorMessage = "Item ID is required")]
        public long ErrorId { get; set; }
        public int? State { get; set; }
        //Bridge Number
        public string? BID01 { get; set; }
        //Owner
        public string? BCL01 { get; set; }
        public string ItemId { get; set; }
        public string? ItemName { get; set; }

        [Required(ErrorMessage = "Submitted Value is required")]
        public string? SubmittedValue { get; set; }
        public string ErrorType { get; set; }
        public string Description { get; set; }
        public int Frequency { get; set; }

        public int SortOrder { get; set; }

        public bool Reviewed { get; set; }
        public bool Ignore { get; set; }

        public string? Comment { get; set; }
        public string? ReviewedBy { get; set; }
        public string? IgnoredBy { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? IgnoredDate { get; set; }
        public string? DataSet { get; set; }
        public bool IsCorrected { get; set; }
        public string? FieldType { get; set; }

    }
}
