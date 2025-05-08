using NBTIS.Core.DTOs;
using NBTIS.Data.Models;

namespace NBTIS.Web.ViewModels
{
    // Simple model class for Grid rows
    public class SubmittalItem
    {
        public long SubmitId { get; set; }
        public string? SubmittedBy { get; set; }
        public string? SubmittedByDescription { get; set; }
        public string? UploadType { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }

        public byte? StatusCode { get; set; }
        public string? Status { get; set; }
        public string? ReportTitle { get; set; }
        public byte[]? ReportContent { get; set; }
        public string? Comments { get; set; }
        public bool SubmitAllowed { get; set; }
        public bool CorrectAllowed { get; set; }
        public bool DeleteAllowed { get; set; }
        public bool CancelAllowed { get; set; }
        public bool DisplayRecord { get; set; }
        public bool Approve { get; set; }
        public bool Return { get; set; }

        public byte[]? FileContent { get; set; }

        public List<SubmittalCommentDTO> SubmittalComments { get; set; }
      



        //public bool IsPartial { get; set; }

        //public byte StatusCode { get; set; }

        //public string? Comments { get; set; }

        //public byte[]? ReportContent { get; set; }

        //public DateTime? SubmitDate { get; set; }

        //public string? Submitter { get; set; }

        //public DateTime? ReviewDate { get; set; }

        //public string? Reviewer { get; set; }

    }

  

    }
