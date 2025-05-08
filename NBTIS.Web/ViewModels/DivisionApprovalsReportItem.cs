using NBTIS.Core.DTOs;

namespace NBTIS.Web.ViewModels
{
    public class DivisionApprovalsSubmittalItem
    {
        public bool Approve { get; set; }
        public bool Return {  get; set; }
        public string? Status { get; set; }

        public long SubmitId { get; set; }
        public string? SubmittedBy { get; set; }
        public string? SubmittedByDescription { get; set; }
        public string? UploadType { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
        public string? ReportTitle { get; set; }
        public byte[]? ReportContent { get; set; }
        public string? Comments { get; set; }
  
        public List<SubmittalCommentDTO> SubmittalComments { get; set; }

    }
}
