using Microsoft.Data.SqlClient;
using static NBTIS.Core.Utilities.Constants;

namespace NBTIS.Core.DTOs
{
    public class ErrorSummary
    {
        public long ErrorID { get; set; }
        public byte? State { get; set; }
        //Bridge Number
        public string? BID01 { get; set; }
        //Owner
        public string? BCL01 { get; set; }
        public string ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? SubmittedValue { get; set; }
        public string ErrorType { get; set; }
        public string Description { get; set; }
        public int Frequency { get; set; }

        public int SortOrder { get; set; }
    }
}
