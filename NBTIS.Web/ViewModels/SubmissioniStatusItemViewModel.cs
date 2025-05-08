namespace NBTIS.Web.ViewModels
{

    public class SubmissioniStatusItemViewModel
    {
        public long? SubmittalLogsViewID { get; set; }

        public string? Lookup_States_Description { get; set; }

        public string? Lookup_States_Abbreviation { get; set; }

        public string? Lookup_Statuses_StatusDescription { get; set; }

        public int StateOrder { get; set; }

        public long? SubmitId { get; set; }

        public string? SubmittedBy { get; set; }

        public bool? IsPartial { get; set; }

        public byte? StatusCode { get; set; }

        public string? ReportContent { get; set; }

        public DateTime? UploadDate { get; set; }

        public string? UploadedBy { get; set; }

        public DateTime? SubmitDate { get; set; }

        public string? Submitter { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? Reviewer { get; set; }

        public DateTime? ApproveRejectDate { get; set; }

        public string? Approver { get; set; }

    }
}
