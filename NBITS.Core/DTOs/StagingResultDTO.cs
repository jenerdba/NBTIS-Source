using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBTIS.Core.DTOs
{
    public class StagingResultDTO
    {
        public int BridgesInserted { get; set; }
        public int ElementsInserted { get; set; }
        public int FeaturesInserted { get; set; }
        public int RoutesInserted { get; set; }
        public int InspectionsInserted { get; set; }
        public int PostingEvaluationsInserted { get; set; }
        public int PostingStatusesInserted { get; set; }
        public int SpanSetsInserted { get; set; }
        public int SubstructureSetsInserted { get; set; }
        public int WorksInserted { get; set; }

        public TimeSpan ProcessingTime { get; set; }
    }
}
