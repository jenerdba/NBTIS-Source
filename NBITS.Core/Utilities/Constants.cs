using System.Text.RegularExpressions;

namespace NBTIS.Core.Utilities
{
    public static class Constants
    {
        public const string FedAgencyFieldName = "BCL01";
        public enum DataSet
        {
            Primary = 1,
            Elements = 2,
            Features = 3,
            Routes = 4,
            Inspections = 5,
            SpanSets = 6,
            PostingStatuses = 7,
            PostingEvaluations = 8,
            SubstructureSets = 9,
            Works = 10
        }

        public static readonly Dictionary<string, (string LongitudePolarity, string LatitudePolarity)> PolarityCodes = new Dictionary<string, (string, string)>
    {
        { "60", ("-", "-") },
        { "64", ("+", "+") },
        { "66", ("+", "+") },
        { "68", ("+", "+") },
        { "69", ("+", "+") },
        { "70", ("+", "+") },
        { "74", ("+", "+") }
    };
        public const string CommentType_ACC_REJ = "ACC_REJ";
        // Regex pattern to match codes starting with S, L, or P (case-insensitive)
        public const string ExcludeNonFedCodesPattern = "^[SLP].*";

        // Precompiled regex for performance
        public static readonly Regex ExcludeNonFedRegex = new Regex(
            ExcludeNonFedCodesPattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Regex pattern to match 'W##' or 'F##'
        public const string WaterwayFeatureRegex = @"^[W]\d{2}$";
        public const string WaterwayFeatureReliefRegex = @"^[WF]\d{2}$";

        // Regex pattern to match "C##" or "V##" for culvert spans
        public const string CulvertSpanRegex = @"^[CV]\d{2}$";
        public const string NonCulvertSpanRegex = @"^[MAW]\d{2}$";

        //BSP04
        public static readonly HashSet<string> spanSteelCodes = new HashSet<string> { "S01", "S02", "S03", "S04", "S05", "SX", "S-T" };
        //BSP03
        public static readonly HashSet<string> substructureSteelCodes = new HashSet<string> { "S01", "S02", "S03", "S04", "S05", "S06", "SX", "S-T" };

        //BSP06 - Span Type - Slab 
        public static readonly string[] slabSuperstructure = { "S01", "S02", "S-T" };


        public static readonly string[] TemporaryCodes = new string[]
        { "VLM-T", "HVH-T", "AB-T", "BCE-T", "CD-T", "MA-T", "MI-T", "MO-T", "T", "I0-T", "Y-T", "X-T",
          "CR-T", "S-T", "CP-T", "T-T", "M-T", "AI-T", "C-T" , "7-T", "GB-T", "GT-T", "BM-T", "BS-T",
          "F-T", "T-T", "AD-T", "AT-T", "Z-T", "GC-T", "P-T", "MX-T", "PA-T", "PR-T", "PP-T", "T-U", "1-T", "2-T"
        };

        public static readonly string TempCodeBH02 = "T-U";

        public static readonly string[] array510 = { "12", "13", "15", "16", "28", "29", "30", "31", "38", "54", "60", "65", "240", "241", "242", "243", "244", "245" };
        public static readonly string[] array515 = { "28", "29", "30", "102", "107", "113", "120", "141", "142", "147", "148", "152", "161", "162", "202", "207", "219", "225", "231", "240", "300", "301", "302", "303", "304", "305", "306", "310", "311", "312", "313", "314", "315", "316", "330", "301A", "302A", "302B", "303A", "303B", "305A", "305B" };
        public static readonly string[] array521 = { "12", "13", "15", "16", "29", "38", "104", "105", "109", "110", "115", "116", "143", "144", "154", "155", "204", "205", "210", "215", "218", "220", "226", "227", "233", "234", "241", "245", "331", "510" };  //Added 218 per Wendy's element cross check email. Added 510.

        public static readonly string[] slabElements = { "38", "54", "65" };
        public static readonly string[] culvertElements = { "240", "241", "242", "243", "244", "245" };
        public static readonly string[] nonSteelSubstructures = { "203", "204", "205", "210", "211", "215", "216", "218", "220", "226", "227", "229", "233", "234", "236" };
        public static readonly string[] deckElements = { "12", "13", "15", "16", "28", "29", "30", "31", "38", "54", "60", "65" };

        public static readonly string[] superstructureElements =
        {
            "102", "104", "105", "106", "107", "109", "110", "111",
            "112", "113", "115", "116", "117", "118", "120", "135", "136", "141", "142", "143", "144", "145", "146", "147", "148", "149", "152",
            "154", "155", "156", "157", "161", "162"
        };

        public static readonly HashSet<string> coatingSystems = new HashSet<string> { "C01", "C02", "C03", "C04", "CX" };

        //BL01, BL02, and BH02 are not included because we do not use LOVValidator for State Code, County Code and Urban Code validations.
        public static readonly HashSet<string> lookupFiles = new HashSet<string>
            {
            "BAP01.json",
            "BAP02.json",
            "BAP03.json",
            "BAP04.json",
            "BAP05.json",
            "BC01_BC07.json",
            "BC08.json",
            "BC09.json",
            "BC10.json",
            "BC11.json",
            "BCL01.json",
            "BCL03.json",
            "BCL04.json",
            "BCL05.json",
            "BE01.json",
            "BEP03.json",
            "BF01.json",
            "BF02.json",
            "BG10.json",
            "BG12.json",
            "BH01.json",
            "BH04.json",
            "BIE01.json",
            "BIE07.json",
            "BIE12.json",
            "BIR01.json",
            "BLR01.json",
            "BLR02.json",
            "BLR04.json",
            "BLR08.json",
            "BN01.json",
            "BN06.json",
            "BPS01.json",
            "BRH01_BRH02.json",
            "BRR01.json",
            "BRT03.json",
            "BRT04.json",
            "BRT05.json",
            "BSB03.json",
            "BSB04.json",
            "BSB05.json",
            "BSB06.json",
            "BSB07.json",
            "BSP04.json",
            "BSP05.json",
            "BSP06.json",
            "BSP07.json",
            "BSP08.json",
            "BSP09.json",
            "BSP10.json",
            "BSP11.json",
            "BSP12.json",
            "BSP13.json",
            "BW03.json"
        };

        //public static readonly Dictionary<string, string> bridgeData = new Dictionary<string, string>
        //{
        //    {"BID01", "Bridge Number"},
        //    {"BID02", "Bridge Name"},
        //    {"BID03", "Previous Bridge Number"},
        //    {"BL01", "State Code"},
        //    {"BL02", "County Code"},
        //    {"BL03", "Place Code"},
        //    {"BL04", "Highway Agency District"},
        //    {"BL05", "Latitude"},
        //    {"BL06", "Longitude"},
        //    {"BL07", "Border Bridge Number"},
        //    {"BL08", "Border Bridge State or Country Code"},
        //    {"BL09", "Border Bridge Inspection Responsibility"},
        //    {"BL10", "Border Bridge Designated Lead State"},
        //    {"BL11", "Bridge Location"},
        //    {"BL12", "Metropolitan Planning Organization"},
        //    {"BCL01", "Owner"},
        //    {"BCL02", "Maintenance Responsibility"},
        //    {"BCL03", "Federal or Tribal Land Access"},
        //    {"BCL04", "Historic Significance"},
        //    {"BCL05", "Toll"},
        //    {"BCL06", "Emergency Evacuation Designation"},
        //    {"BSP01", "Span Configuration Designation"},
        //    {"BSP02", "Number of Spans"},
        //    {"BSP03", "Number of Beam Lines"},
        //    {"BSP04", "Span Material"},
        //    {"BSP05", "Span Continuity"},
        //    {"BSP06", "Span Type"},
        //    {"BSP07", "Span Protective System"},
        //    {"BSP08", "Deck Interaction"},
        //    {"BSP09", "Deck Material and Type"},
        //    {"BSP10", "Wearing Surface"},
        //    {"BSP11", "Deck Protective System"},
        //    {"BSP12", "Deck Reinforcing Protective System"},
        //    {"BSP13", "Deck Stay-In-Place Forms"},
        //    {"BSB01", "Substructure Configuration Designation"},
        //    {"BSB02", "Number of Substructure Units"},
        //    {"BSB03", "Substructure Material"},
        //    {"BSB04", "Substructure Type"},
        //    {"BSB05", "Substructure Protective System"},
        //    {"BSB06", "Foundation Type"},
        //    {"BSB07", "Foundation Protective System"},
        //    {"BRH01", "Bridge Railings"},
        //    {"BRH02", "Transitions"},
        //    {"BG01", "NBIS Bridge Length"},
        //    {"BG02", "Total Bridge Length"},
        //    {"BG03", "Maximum Span Length"},
        //    {"BG04", "Minimum Span Length"},
        //    {"BG05", "Bridge Width Out-To-Out"},
        //    {"BG06", "Bridge Width Curb-To-Curb"},
        //    {"BG07", "Left Curb or Sidewalk Width"},
        //    {"BG08", "Right Curb or Sidewalk Width"},
        //    {"BG09", "Approach Roadway Width"},
        //    {"BG10", "Bridge Median"},
        //    {"BG11", "Skew"},
        //    {"BG12", "Curved Bridge"},
        //    {"BG13", "Maximum Bridge Height"},
        //    {"BG14", "Sidehill Bridge"},
        //    {"BG15", "Irregular Deck Area"},
        //    {"BG16", "Calculated Deck Area"},
        //    {"BF01", "Feature Type"},
        //    {"BF02", "Feature Location"},
        //    {"BF03", "Feature Name"},
        //    {"BRT01", "Route Designation"},
        //    {"BRT02", "Route Number"},
        //    {"BRT03", "Route Direction"},
        //    {"BRT04", "Route Type"},
        //    {"BRT05", "Service Type"},
        //    {"BH01", "Functional Classification"},
        //    {"BH02", "Urban Code"},
        //    {"BH03", "NHS Designation"},
        //    {"BH04", "National Highway Freight Network"},
        //    {"BH05", "STRAHNET Designation"},
        //    {"BH06", "LRS Route ID"},
        //    {"BH07", "LRS Mile Point"},
        //    {"BH08", "Lanes On Highway"},
        //    {"BH09", "Annual Average Daily Traffic"},
        //    {"BH10", "Annual Average Daily Truck Traffic"},
        //    {"BH11", "Year of Annual Average Daily Traffic"},
        //    {"BH12", "Highway Maximum Usable Vertical Clearance"},
        //    {"BH13", "Highway Minimum Vertical Clearance"},
        //    {"BH14", "Highway Minimum Horizontal Clearance, Left"},
        //    {"BH15", "Highway Minimum Horizontal Clearance, Right"},
        //    {"BH16", "Highway Maximum Usable Surface Width"},
        //    {"BH17", "Bypass Detour Length"},
        //    {"BH18", "Crossing Bridge Number"},
        //    {"BRR01", "Railroad Service Type"},
        //    {"BRR02", "Railroad Minimum Vertical Clearance"},
        //    {"BRR03", "Railroad Minimum Horizontal Offset"},
        //    {"BN01", "Navigable Waterway"},
        //    {"BN02", "Navigation Minimum Vertical Clearance"},
        //    {"BN03", "Movable Bridge Maximum Navigation Vertical Clearance"},
        //    {"BN04", "Navigation Channel Width"},
        //    {"BN05", "Navigation Channel Minimum Horizontal Clearance"},
        //    {"BN06", "Substructure Navigation Protection"},
        //    {"BLR01", "Design Load"},
        //    {"BLR02", "Design Method"},
        //    {"BLR03", "Load Rating Date"},
        //    {"BLR04", "Load Rating Method"},
        //    {"BLR05", "Inventory Load Rating Factor"},
        //    {"BLR06", "Operating Load Rating Factor"},
        //    {"BLR07", "Controlling Legal Load Rating Factor"},
        //    {"BLR08", "Routine Permit Loads"},
        //    {"BPS01", "Load Posting Status"},
        //    {"BPS02", "Posting Status Change Date"},
        //    {"BEP01", "Legal Load Configuration"},
        //    {"BEP02", "Legal Load Rating Factor"},
        //    {"BEP03", "Posting Type"},
        //    {"BEP04", "Posting Value"},
        //    {"BIR01", "NSTM Inspection Required"},
        //    {"BIR02", "Fatigue Details"},
        //    {"BIR03", "Underwater Inspection Required"},
        //    {"BIR04", "Complex Feature"},
        //    {"BIE01", "Inspection Type"},
        //    {"BIE02", "Inspection Begin Date"},
        //    {"BIE03", "Inspection Completion Date"},
        //    {"BIE04", "Nationally Certified Bridge Inspector"},
        //    {"BIE05", "Inspection Interval"},
        //    {"BIE06", "Inspection Due Date"},
        //    {"BIE07", "Risk-Based Inspection Interval Method"},
        //    {"BIE08", "Inspection Quality Control Date"},
        //    {"BIE09", "Inspection Quality Assurance Date"},
        //    {"BIE10", "Inspection Data Update Date"},
        //    {"BIE11", "Inspection Note"},
        //    {"BIE12", "Inspection Equipment"},
        //    {"BC01", "Deck Condition Rating"},
        //    {"BC02", "Superstructure Condition Rating"},
        //    {"BC03", "Substructure Condition Rating"},
        //    {"BC04", "Culvert Condition Rating"},
        //    {"BC05", "Bridge Railing Condition Rating"},
        //    {"BC06", "Bridge Railing Transitions Condition Rating"},
        //    {"BC07", "Bridge Bearings Condition Rating"},
        //    {"BC08", "Bridge Joints Condition Rating"},
        //    {"BC09", "Channel Condition Rating"},
        //    {"BC10", "Channel Protection Condition Rating"},
        //    {"BC11", "Scour Condition Rating"},
        //    {"BC12", "Bridge Condition Classification"},
        //    {"BC13", "Lowest Condition Rating Code"},
        //    {"BC14", "NSTM Inspection Condition"},
        //    {"BC15", "Underwater Inspection Condition"},
        //    {"BE01", "Element Number"},
        //    {"BE02", "Element Parent Number"},
        //    {"BE03", "Element Total Quantity"},
        //    {"BCS01", "Element Quantity Condition State One"},
        //    {"BCS02", "Element Quantity Condition State Two"},
        //    {"BCS03", "Element Quantity Condition State Three"},
        //    {"BCS04", "Element Quantity Condition State Four"},
        //    {"BAP01", "Approach Roadway Alignment"},
        //    {"BAP02", "Overtopping Likelihood"},
        //    {"BAP03", "Scour Vulnerability"},
        //    {"BAP04", "Scour Plan of Action"},
        //    {"BAP05", "Seismic Vulnerability"},
        //    {"BW01", "Year Built"},
        //    {"BW02", "Year Work Performed"},
        //    {"BW03", "Work Performed"}
        //};


    }
}
