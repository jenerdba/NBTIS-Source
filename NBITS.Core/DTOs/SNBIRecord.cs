using System.Text.Json;
using System.Collections;
using NBTIS.Data.Models;

namespace NBTIS.Core.DTOs
{
    public class SNBIRecord
    {
        public long SubmitId { get; set; }
        public string SubmittedBy { get; set; } = null!;
        public int? BL01 { get; set; }
        public string? BID01 { get; set; }
        public string? BID02 { get; set; }
        public string? BID03 { get; set; }
        public int? BL02 { get; set; }
        public int? BL03 { get; set; }
        public string? BL04 { get; set; }
        public double? BL05 { get; set; }
        public double? BL06 { get; set; }
        public string? BL07 { get; set; }
        public string? BL08 { get; set; }
        public string? BL09 { get; set; }
        public string? BL10 { get; set; }
        public string? BL11 { get; set; }
        public string? BL12 { get; set; }
        public string? BCL01 { get; set; }
        public string? BCL02 { get; set; }
        public string? BCL03 { get; set; }
        public string? BCL04 { get; set; }
        public string? BCL05 { get; set; }
        public string? BCL06 { get; set; }
        public string? BRH01 { get; set; }
        public string? BRH02 { get; set; }
        public double? BG01 { get; set; }
        public double? BG02 { get; set; }
        public double? BG03 { get; set; }
        public double? BG04 { get; set; }
        public double? BG05 { get; set; }
        public double? BG06 { get; set; }
        public double? BG07 { get; set; }
        public double? BG08 { get; set; }
        public double? BG09 { get; set; }
        public string? BG10 { get; set; }
        public double? BG11 { get; set; }
        public string? BG12 { get; set; }
        public double? BG13 { get; set; }
        public string? BG14 { get; set; }
        public double? BG15 { get; set; }
        public double? BG16 { get; set; }
        public string? BLR01 { get; set; }
        public string? BLR02 { get; set; }
        public string? BLR03 { get; set; }
        public string? BLR04 { get; set; }
        public double? BLR05 { get; set; }
        public double? BLR06 { get; set; }
        public double? BLR07 { get; set; }
        public string? BLR08 { get; set; }
        public string? BIR01 { get; set; }
        public string? BIR02 { get; set; }
        public string? BIR03 { get; set; }
        public string? BIR04 { get; set; }
        public string? BC01 { get; set; }
        public string? BC02 { get; set; }
        public string? BC03 { get; set; }
        public string? BC04 { get; set; }
        public string? BC05 { get; set; }
        public string? BC06 { get; set; }
        public string? BC07 { get; set; }
        public string? BC08 { get; set; }
        public string? BC09 { get; set; }
        public string? BC10 { get; set; }
        public string? BC11 { get; set; }
        public string? BC12 { get; set; }
        public string? BC13 { get; set; }
        public string? BC14 { get; set; }
        public string? BC15 { get; set; }
        public string? BAP01 { get; set; }
        public string? BAP02 { get; set; }
        public string? BAP03 { get; set; }
        public string? BAP04 { get; set; }
        public string? BAP05 { get; set; }
        public int? BW01 { get; set; }
        public ICollection<Element>? Elements { get; set; }
        public ICollection<Feature>? Features { get; set; }
        public ICollection<Inspection>? Inspections { get; set; }
        public ICollection<PostingEvaluation>? PostingEvaluations { get; set; }
        public ICollection<PostingStatus>? PostingStatuses { get; set; }
        public ICollection<SpanSet>? SpanSets { get; set; }
        public ICollection<SubstructureSet>? SubstructureSets { get; set; }
        public ICollection<Work>? Works { get; set; }

        public virtual ICollection<Route> Routes { get; set; } = new List<Route>();

        public class Element
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }

            public string? BE01 { get; set; }
            public string? BE02 { get; set; }
            public int? BE03 { get; set; }
            public double? BCS01 { get; set; }
            public double? BCS02 { get; set; }
            public double? BCS03 { get; set; }
            public double? BCS04 { get; set; }
        }

        public class Feature
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }


            public string? BF01 { get; set; }
            public string? BF02 { get; set; }
            public string? BF03 { get; set; }
            public string? BH01 { get; set; }
            public string? BH02 { get; set; }
            public string? BH03 { get; set; }
            public string? BH04 { get; set; }
            public string? BH05 { get; set; }
            public string? BH06 { get; set; }
            public double? BH07 { get; set; }
            public double? BH08 { get; set; }
            public double? BH09 { get; set; }
            public double? BH10 { get; set; }
            public int? BH11 { get; set; }
            public double? BH12 { get; set; }
            public double? BH13 { get; set; }
            public double? BH14 { get; set; }
            public double? BH15 { get; set; }
            public double? BH16 { get; set; }
            public int? BH17 { get; set; }
            public string? BH18 { get; set; }
            public string? BRR01 { get; set; }
            public double? BRR02 { get; set; }
            public double? BRR03 { get; set; }
            public string? BN01 { get; set; }
            public double? BN02 { get; set; }
            public double? BN03 { get; set; }
            public double? BN04 { get; set; }
            public double? BN05 { get; set; }
            public string? BN06 { get; set; }
            public ICollection<Route>? Routes { get; set; }
        }

        public class Route
        {
            public long Id { get; set; }
            public long FeatureId { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }

            public string? BF01 { get; set; }
            public string? BF02 { get; set; }
            public string? BF03 { get; set; }

            public string? BRT01 { get; set; }
            public string? BRT02 { get; set; }
            public string? BRT03 { get; set; }
            public string? BRT04 { get; set; }
            public string? BRT05 { get; set; }
        }

        public class Inspection
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }

            public string? BIE01 { get; set; }
            public string? BIE02 { get; set; }
            public string? BIE03 { get; set; }
            public string? BIE04 { get; set; }
            public int? BIE05 { get; set; }
            public string? BIE06 { get; set; }
            public string? BIE07 { get; set; }
            public string? BIE08 { get; set; }
            public string? BIE09 { get; set; }
            public string? BIE10 { get; set; }
            public string? BIE11 { get; set; }
            public string? BIE12 { get; set; }
        }

        public class PostingEvaluation
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }
            public string? BEP01 { get; set; }
            public double? BEP02 { get; set; }
            public string? BEP03 { get; set; }
            public string? BEP04 { get; set; }
        }

        public class PostingStatus
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }

            public string? BPS02 { get; set; }
            public string? BPS01 { get; set; }
        }

        public class SpanSet
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }

            public string? BSP01 { get; set; }
            public double? BSP02 { get; set; }
            public double? BSP03 { get; set; }
            public string? BSP04 { get; set; }
            public string? BSP05 { get; set; }
            public string? BSP06 { get; set; }
            public string? BSP07 { get; set; }
            public string? BSP08 { get; set; }
            public string? BSP09 { get; set; }
            public string? BSP10 { get; set; }
            public string? BSP11 { get; set; }
            public string? BSP12 { get; set; }
            public string? BSP13 { get; set; }
        }

        public class SubstructureSet
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }
            public string? BSB01 { get; set; }
            public double? BSB02 { get; set; }
            public string? BSB03 { get; set; }
            public string? BSB04 { get; set; }
            public string? BSB05 { get; set; }
            public string? BSB06 { get; set; }
            public string? BSB07 { get; set; }
        }

        public class Work
        {
            public long Id { get; set; }
            public long SubmitId { get; set; }
            public string SubmittedBy { get; set; } = null!;
            public string? RecordStatus { get; set; }
            public int? BL01 { get; set; }
            public int? BL02 { get; set; }
            public string BID01 { get; set; }
            public string BCL01 { get; set; }
            public int? BW01 { get; set; }
            public int? BW02 { get; set; }
            public string? BW03 { get; set; }
        }

        public Dictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    }

}
