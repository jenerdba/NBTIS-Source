using NBTIS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBTIS.Core.DTOs
{
    public class SubmittalLogDTO
    {
        public long SubmitId { get; set; }
        public string? SubmittedBy { get; set; }
        public bool IsPartial { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
        public byte StatusCode { get; set; }
        public string? ReportTitle { get; set; }
        public byte[]? ReportContent { get; set; }
        public string? Comments { get; set; }
        public string SubmittedByDescription { get; set; } = string.Empty;
        public bool SubmitAllowed { get; set; }
        public bool DeleteAllowed { get; set; }

        public byte[]? FileContent { get; set; }

        public List<SubmittalCommentDTO> SubmittalComments { get; set; }


        //public DateTime? SubmitDate { get; set; }

        //public string? Submitter { get; set; }

        //public DateTime? ReviewDate { get; set; }

        //public string? Reviewer { get; set; }
    }
}
