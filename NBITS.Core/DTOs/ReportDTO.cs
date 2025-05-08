using NBTIS.Core.DTOs;
using System.Collections;
using System.Net.Sockets;
using static NBTIS.Core.DTOs.SNBIRecord;

namespace NBTIS.Core.DTOs
{
    public class ProcessingReport
    {
        public long SubmitId { get; set; } = 0;
        public string SubmittedBy { get; set; } = string.Empty;
        public int TotalRecordsUploaded { get; set; } = 0;
        public int TotalRecordsOmitted { get; set; } = 0;
        public int TotalSafetyErrors { get; set; } = 0;
        public int TotalCriticalErrors { get; set; } = 0;
        public int TotalGeneralErrors { get; set; } = 0;
        public int TotalFlags { get; set; } = 0;

        public int TotalDuplicateBridges { get; set; } = 0;
        public List<KeyValuePair<Type, IList>>? Duplicates { get; set; }
        public List<NonNBIBridge> NonNBIBridges { get; set; } = new List<NonNBIBridge>();


        public class Error
        {           
            //Location
            public int? State { get; set; }
            public string County { get; set; } = string.Empty;
            //Bridge Number
            public string? BID01 { get; set; }
            //Owner
            public string? BCL01 { get; set; }

            public string? ItemId { get; set; }
            public string? ItemName { get; set; }
            public string? DataSet { get; set; }
            public string? SubmittedValue { get; set; }
            public string? ErrorType { get; set; }
            public string? ValidationType { get; set; } //For Safety errors
            public string? ErrorCode { get; set; }
            public string? Description { get; set; }
            public int SortOrder { get; set; }
        }

        public class ElementError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.Element ElementDetails { get; set; }

            public ElementError()
            {
                ErrorDetails = new Error();
                ElementDetails = new SNBIRecord.Element();
            }
        }

        public class FeatureError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.Feature FeatureDetails { get; set; }

            public FeatureError()
            {
                ErrorDetails = new Error();
                FeatureDetails = new SNBIRecord.Feature();
            }
        }

        public class RouteError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.Route RouteDetails { get; set; }

            public RouteError()
            {
                ErrorDetails = new Error();
                RouteDetails = new SNBIRecord.Route();
            }
        }

        public class InspectionError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.Inspection InspectionDetails { get; set; }

            public InspectionError()
            {
                ErrorDetails = new Error();
                InspectionDetails = new SNBIRecord.Inspection();
            }
        }

        public class SpanSetError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.SpanSet SpanSetDetails { get; set; }

            public SpanSetError()
            {
                ErrorDetails = new Error();
                SpanSetDetails = new SNBIRecord.SpanSet();
            }
        }

        public class PostingStatusError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.PostingStatus PostingStatusDetails { get; set; }

            public PostingStatusError()
            {
                ErrorDetails = new Error();
                PostingStatusDetails = new SNBIRecord.PostingStatus();
            }
        }

        public class PostingEvaluationsError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.PostingEvaluation PostingEvaluationsDetails { get; set; }

            public PostingEvaluationsError()
            {
                ErrorDetails = new Error();
                PostingEvaluationsDetails = new SNBIRecord.PostingEvaluation();
            }
        }

        public class SubstructureSetError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.SubstructureSet SubstructureSetDetails { get; set; }

            public SubstructureSetError()
            {
                ErrorDetails = new Error();
                SubstructureSetDetails = new SNBIRecord.SubstructureSet();
            }
        }

        public class WorkError
        {
            public Error ErrorDetails { get; set; }
            public SNBIRecord.Work WorkDetails { get; set; }

            public WorkError()
            {
                ErrorDetails = new Error();
                WorkDetails = new SNBIRecord.Work();
            }
        }

        public class Primary
        {
            //State Code
            public int? BL01 { get; set; }
            //County Code
            public int? BL02 { get; set; }
            public string State { get; set; } = string.Empty;
            public string County { get; set; } = string.Empty;
            //Bridge Number
            public string? BID01 { get; set; }
            //Bridge Name
            public string? BID02 { get; set; }
            //Owner
            public string? BCL01 { get; set; }
            //Bridge Location
            public string? BL11 { get; set;}
           
        }


      
     
    }
}
