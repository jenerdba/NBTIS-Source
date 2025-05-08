namespace NBTIS.Core.DTOs
{
    public static class TemporaryCounts
    {
        //Historic Significance BCL04
        public static int BCL04Count { get; set; } = 0;
        //Span Material BSP04
        public static int BSP04Count { get; set; } = 0;
        //Span Continuity BSP05
        public static int BSP05Count { get; set; } = 0;
        //Span Type BSP06
        public static int BSP06Count { get; set; } = 0;
        //Deck Material and Type BSP09
        public static int BSP09Count { get; set; } = 0;
        //Wearing Surface BSP10
        public static int BSP10Count { get; set; } = 0;
        //Deck Protective System BSP11
        public static int BSP11Count { get; set; } = 0;
        //Deck Reinforcing Protective System BSP12
        public static int BSP12Count { get; set; } = 0;
        //BRH01 - Bridge Railings
        public static int BRH01Count { get; set; } = 0;
        //BRH02 - Transitions
        public static int BRH02Count { get; set; } = 0;
        //BRT03 - Route Direction
        public static int BRT03Count { get; set; } = 0;
        //BH01 - Functional Classification
        public static int BH01Count { get; set; } = 0;
        //BH02 - Urban Code
        public static int BH02Count { get; set; } = 0;
        //BH04 - National Highway Freight Network
        public static int BH04Count { get; set; } = 0;
        //BN06 - Substructure Navigation Protection
        public static int BN06Count { get; set; } = 0;
        //BPS01 - Load Posting Status
        public static int BPS01Count { get; set; } = 0;
        //BC11 - Scour Condition Rating
        public static int BC11Count { get; set; } = 0;
        //BAP02 - Overtopping Likelihood
        public static int BAP02Count { get; set; } = 0;
        //BAP03 - Scour Vulnerability BAP 03
        public static int BAP03Count { get; set; } = 0;
        //BIE12 - Inspection Equipment
        public static int BIE12Count { get; set; } = 0;

        //BSB01 - Substructure Configuration Designation
        public static int BSB03Count { get; set; } = 0;
        //BSB04 - Substructure Type
        public static int BSB04Count { get; set; } = 0;
        //BSB05 - Substructure Protective System
        public static int BSB05Count { get; set; } = 0;
        //BSB06 - Foundation Type
        public static int BSB06Count { get; set; } = 0;
        //BSB06 - Foundation Protective System
        public static int BSB07Count { get; set; } = 0;
        //BW03 - Work Performed
        public static int BW03Count { get; set; } = 0;
        //BEP03 - Posting Type
       // public static int BEP03Count { get; set; } = 0;

        public static bool AllTemporaryCountsZero()
        {
            var countValues = new[]
            {
            BCL04Count,
            BSP04Count,
            BSP05Count,
            BSP06Count,
            BSP09Count,
            BSP10Count,
            BSP11Count,
            BSP12Count,
            BRH01Count,
            BRH02Count,
            BRT03Count,
            BH01Count,
            BH02Count,
            BH04Count,
            BN06Count,
            BPS01Count,
            BC11Count,
            BAP02Count,
            BAP03Count,
            BIE12Count,
            BSB03Count,
            BSB04Count,
            BSB05Count,
            BSB06Count,
            BSB07Count,
            BW03Count
        };

            return countValues.All(count => count == 0);
        }

        // Method to reset all counts to zero
        public static void Reset()
        {
            BCL04Count = 0;
            BSP04Count = 0;
            BSP05Count = 0;
            BSP06Count = 0;
            BSP09Count = 0;
            BSP10Count = 0;
            BSP11Count = 0;
            BSP12Count = 0;
            BRH01Count = 0;
            BRH02Count = 0;
            BRT03Count = 0;
            BH01Count = 0;
            BH02Count = 0;
            BH04Count = 0;
            BN06Count = 0;
            BPS01Count = 0;
            BC11Count = 0;
            BAP02Count = 0;
            BAP03Count = 0;
            BIE12Count = 0;
            BSB03Count = 0;
            BSB04Count = 0;
            BSB05Count = 0;
            BSB06Count = 0;
            BSB07Count = 0;
            BW03Count = 0;
        }
    }


}
