using NBTIS.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NBTIS.Core.DTOs.SNBIRecord;
using static NBTIS.Core.Services.DataProcessor;

namespace NBTIS.Core.Services
{
    public class SNBISanitizer
    {
        private string? SanitizeAN(string? input, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"{fieldName}: Input is empty or whitespace; setting to null.");
                return null;
            }
            if (input.Length > maxLength)
            {
                Console.WriteLine($"{fieldName}: Length {input.Length} exceeds maximum {maxLength}; setting to null.");
                return null;
            }
            return input;
        }

        // Helper for AN fields that need truncation.
        private string? TruncateAN(string? input, int maxLength, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"{fieldName}: Input is empty or whitespace; setting to null.");
                return null;
            }
            if (input.Length > maxLength)
            {
                Console.WriteLine($"{fieldName}: Length {input.Length} exceeds maximum {maxLength}; truncating.");
                return input.Substring(0, maxLength);
            }
            return input;
        }

        // Helper for N(x,0) fields (ints). Returns 0 if input is null/invalid.
        private int SanitizeNInt(int? input, int maxDigits, string fieldName)
        {
            if (!input.HasValue)
            {
                Console.WriteLine($"{fieldName}: Input is null; defaulting to 0.");
                return 0;
            }
            int value = input.Value;
            int digitCount = Math.Abs(value).ToString().Length;
            if (digitCount > maxDigits)
            {
                Console.WriteLine($"{fieldName}: {value} has {digitCount} digits, exceeds maximum {maxDigits}; setting to 0.");
                return 0;
            }
            return value;
        }

        // Helper for N(x,y) fields (doubles). Returns null if input is null or invalid;
        // if too many decimal places, truncates (rounds down) to allowed precision.
        // Overload for double? input
        private double? SanitizeNDouble(double? input, int totalDigits, int decimalPlaces, string fieldName)
        {
            if (!input.HasValue)
            {
                Console.WriteLine($"{fieldName}: Input is null.");
                return null;
            }

            return SanitizeNDouble(input.Value, totalDigits, decimalPlaces, fieldName);
        }

        // Overload for string input
        private double? SanitizeNDouble(string? input, int totalDigits, int decimalPlaces, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"{fieldName}: Input is null or empty.");
                return null;
            }

            if (!double.TryParse(input, out double parsedValue))
            {
                Console.WriteLine($"{fieldName}: Unable to parse '{input}' to a double.");
                return null;
            }

            return SanitizeNDouble(parsedValue, totalDigits, decimalPlaces, fieldName);
        }

        // Helper method for non-nullable double input
        private double? SanitizeNDouble(double input, int totalDigits, int decimalPlaces, string fieldName)
        {
            double absValue = Math.Abs(input);
            int allowedIntegerDigits = totalDigits - decimalPlaces;
            int integerDigits = ((long)Math.Floor(absValue)).ToString().Length;

            if (integerDigits > allowedIntegerDigits)
            {
                Console.WriteLine($"{fieldName}: {input} has {integerDigits} integer digits (allowed: {allowedIntegerDigits}); setting to null.");
                return null;
            }

            // Truncate extra decimal places.
            double factor = Math.Pow(10, decimalPlaces);
            double truncated = Math.Truncate(input * factor) / factor;

            if (Math.Abs(input - truncated) > 0)
            {
                Console.WriteLine($"{fieldName}: {input} truncated to {truncated}.");
            }

            return truncated;
        }


        /// <summary>
        /// Sanitizes element quantity fields (N(8,0)) by:
        /// - Replacing null with 0.
        /// - Replacing negative values with 0.
        /// - Rounding down (floor) any decimal portion for positive values.
        /// - Ensuring the total digit count does not exceed 8; otherwise returns 0.
        /// </summary>
        private int SanitizeElementQuantity(double? input, int totalDigits, string fieldName)
        {
            // If null, default to 0.
            if (!input.HasValue)
            {
                Console.WriteLine($"{fieldName}: Input is null; setting to 0.");
                return 0;
            }

            // If negative, set to 0.
            if (input.Value < 0)
            {
                Console.WriteLine($"{fieldName}: Value is negative ({input.Value}); setting to 0.");
                return 0;
            }

            // Floor the value to round down any decimals.
            double flooredValue = Math.Floor(input.Value);

            // Convert to string (no decimals) to count digits.
            // "F0" means format as an integer string (no decimals).
            string flooredString = flooredValue.ToString("F0");

            // If more than 8 digits, return 0.
            if (flooredString.Length > totalDigits)
            {
                Console.WriteLine($"{fieldName}: Value {flooredValue} has more than 8 digits; setting to 0.");
                return 0;
            }

            // Safe to cast to int because we've floored any decimals.
            int finalValue = (int)flooredValue;
            Console.WriteLine($"{fieldName}: Final sanitized value is {finalValue}.");

            return finalValue;
        }


        // Helper for DATE fields (DateTime).
        private string? SanitizeDate(string? input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine($"{fieldName}: Input is empty or whitespace; setting to null.");
                return null;
            }

            if (input.Length != 8)
            {
                Console.WriteLine($"{fieldName}: Input '{input}' does not have exactly 8 characters; setting to null.");
                return null;
            }

            if (DateTime.TryParseExact(input, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate.ToString("yyyyMMdd");
            }
            else
            {
                Console.WriteLine($"{fieldName}: Input '{input}' is not a valid date in 'yyyyMMdd' format; setting to null.");
                return null;
            }
        }

        // Main method to populate the sanitized record.
        public void PopulateSanitizedRecord(SNBIRecord original, SNBIRecord sanitized, Dictionary<RecordType, HashSet<string>> fatalKeys)
        {
            sanitized.BID01 = SanitizeAN(original.BID01, 15, "BID01");
            sanitized.BID02 = TruncateAN(original.BID02, 300, "BID02");
            sanitized.BID03 = SanitizeAN(original.BID03, 15, "BID03");

            sanitized.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
            sanitized.BL02 = SanitizeNInt(original.BL02, 3, "BL02");
            sanitized.BL03 = SanitizeNInt(original.BL03, 5, "BL03");

            sanitized.BL04 = SanitizeAN(original.BL04, 2, "BL04");
            sanitized.BL05 = SanitizeNDouble(original.BL05, 9, 6, "BL05"); // Latitude (N(9,6))
            sanitized.BL06 = SanitizeNDouble(original.BL06, 10, 6, "BL06"); // Longitude (N(10,6))
            sanitized.BL07 = SanitizeAN(original.BL07, 15, "BL07");
            sanitized.BL08 = SanitizeAN(original.BL08, 2, "BL08");
            sanitized.BL09 = SanitizeAN(original.BL09, 1, "BL09");
            sanitized.BL10 = SanitizeAN(original.BL10, 2, "BL10");
            sanitized.BL11 = TruncateAN(original.BL11, 300, "BL11");
            sanitized.BL12 = TruncateAN(original.BL12, 300, "BL12");

            sanitized.BCL01 = SanitizeAN(original.BCL01, 4, "BCL01");
            sanitized.BCL02 = SanitizeAN(original.BCL02, 4, "BCL02");
            sanitized.BCL03 = TruncateAN(original.BCL03, 30, "BCL03");
            sanitized.BCL04 = SanitizeAN(original.BCL04, 1, "BCL04");
            sanitized.BCL05 = SanitizeAN(original.BCL05, 1, "BCL05");
            sanitized.BCL06 = SanitizeAN(original.BCL06, 1, "BCL06");

            sanitized.BRH01 = SanitizeAN(original.BRH01, 4, "BRH01");
            sanitized.BRH02 = SanitizeAN(original.BRH02, 4, "BRH02");

            sanitized.BG01 = SanitizeNDouble(original.BG01, 7, 1, "BG01");
            sanitized.BG02 = SanitizeNDouble(original.BG02, 7, 1, "BG02");
            sanitized.BG03 = SanitizeNDouble(original.BG03, 5, 1, "BG03");
            sanitized.BG04 = SanitizeNDouble(original.BG04, 5, 1, "BG04");
            sanitized.BG05 = SanitizeNDouble(original.BG05, 4, 1, "BG05");
            sanitized.BG06 = SanitizeNDouble(original.BG06, 4, 1, "BG06");
            sanitized.BG07 = SanitizeNDouble(original.BG07, 3, 1, "BG07");
            sanitized.BG08 = SanitizeNDouble(original.BG08, 3, 1, "BG08");
            sanitized.BG09 = SanitizeNDouble(original.BG09, 4, 1, "BG09");
            sanitized.BG10 = SanitizeAN(original.BG10, 1, "BG10");
            sanitized.BG11 = SanitizeNDouble(original.BG11, 2, 0, "BG11");
            sanitized.BG12 = SanitizeAN(original.BG12, 2, "BG12");
            sanitized.BG13 = SanitizeNDouble(original.BG13, 4, 0, "BG13");
            sanitized.BG14 = SanitizeAN(original.BG14, 1, "BG14");
            sanitized.BG15 = SanitizeNDouble(original.BG15, 10, 1, "BG15");
            sanitized.BG16 = SanitizeNDouble(original.BG16, 10, 1, "BG16");

            sanitized.BLR01 = SanitizeAN(original.BLR01, 8, "BLR01");
            sanitized.BLR02 = SanitizeAN(original.BLR02, 4, "BLR02");
            sanitized.BLR03 = SanitizeDate(original.BLR03, "BLR03");
            sanitized.BLR04 = SanitizeAN(original.BLR04, 4, "BLR04");
            sanitized.BLR05 = SanitizeNDouble(original.BLR05, 4, 2, "BLR05");
            sanitized.BLR06 = SanitizeNDouble(original.BLR06, 4, 2, "BLR06");
            sanitized.BLR07 = SanitizeNDouble(original.BLR07, 4, 2, "BLR07");
            sanitized.BLR08 = SanitizeAN(original.BLR08, 1, "BLR08");

            sanitized.BIR01 = SanitizeAN(original.BIR01, 1, "BIR01");
            sanitized.BIR02 = SanitizeAN(original.BIR02, 1, "BIR02");
            sanitized.BIR03 = SanitizeAN(original.BIR03, 1, "BIR03");
            sanitized.BIR04 = SanitizeAN(original.BIR04, 1, "BIR04");

            sanitized.BC01 = SanitizeAN(original.BC01, 1, "BC01");
            sanitized.BC02 = SanitizeAN(original.BC02, 1, "BC02");
            sanitized.BC03 = SanitizeAN(original.BC03, 1, "BC03");
            sanitized.BC04 = SanitizeAN(original.BC04, 1, "BC04");
            sanitized.BC05 = SanitizeAN(original.BC05, 1, "BC05");
            sanitized.BC06 = SanitizeAN(original.BC06, 1, "BC06");
            sanitized.BC07 = SanitizeAN(original.BC07, 1, "BC07");
            sanitized.BC08 = SanitizeAN(original.BC08, 1, "BC08");
            sanitized.BC09 = SanitizeAN(original.BC09, 1, "BC09");
            sanitized.BC10 = SanitizeAN(original.BC10, 1, "BC10");
            sanitized.BC11 = SanitizeAN(original.BC11, 1, "BC11");
            sanitized.BC12 = SanitizeAN(original.BC12, 1, "BC12");
            sanitized.BC13 = SanitizeAN(original.BC13, 1, "BC13");
            sanitized.BC14 = SanitizeAN(original.BC14, 1, "BC14");
            sanitized.BC15 = SanitizeAN(original.BC15, 1, "BC15");

            sanitized.BAP01 = SanitizeAN(original.BAP01, 1, "BAP01");
            sanitized.BAP02 = SanitizeAN(original.BAP02, 1, "BAP02");
            sanitized.BAP03 = SanitizeAN(original.BAP03, 1, "BAP03");
            sanitized.BAP04 = SanitizeAN(original.BAP04, 1, "BAP04");
            sanitized.BAP05 = SanitizeAN(original.BAP05, 1, "BAP05");

            sanitized.BW01 = SanitizeNInt(original.BW01, 4, "BW01");

            PopulateFeatures(original, sanitized, fatalKeys[RecordType.Feature], fatalKeys[RecordType.Route]);
            PopulateSpanSets(original, sanitized, fatalKeys[RecordType.SpanSet]);
            PopulateSubstructureSets(original, sanitized, fatalKeys[RecordType.SubstructureSet]);
            PopulatePostingStatuses(original, sanitized, fatalKeys[RecordType.PostingStatus]);
            PopulatePostingEvaluations(original, sanitized, fatalKeys[RecordType.PostingEvaluation]);
            PopulateInspections(original, sanitized);
            PopulateElements(original, sanitized, fatalKeys[RecordType.Element]);
            PopulateWorks(original, sanitized);

        }

        private void PopulateSubstructureSets(SNBIRecord original, SNBIRecord sanitized, HashSet<string> fatalSubstructureKeys)
        {
            if (original.SubstructureSets == null)
                return;

            sanitized.SubstructureSets ??= new List<SNBIRecord.SubstructureSet>();

            foreach (var originalSub in original.SubstructureSets)
            {
                var sanitizedSub = new SNBIRecord.SubstructureSet();

                string key = originalSub.BSB01 ?? string.Empty;
                sanitizedSub.RecordStatus = fatalSubstructureKeys.Contains(key) ? "Removed" : "Active";

                // Populate the fields.
                sanitizedSub.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedSub.BL01 = SanitizeNInt(original.BL01, 2, "BL01");

                sanitizedSub.BSB01 = SanitizeAN(originalSub.BSB01, 3, "BSB01"); // Substructure Configuration Designation
                sanitizedSub.BSB02 = SanitizeNDouble(originalSub.BSB02, 3, 0, "BSB02"); // Number of Substructure Units
                sanitizedSub.BSB03 = SanitizeAN(originalSub.BSB03, 3, "BSB03"); // Substructure Material
                sanitizedSub.BSB04 = SanitizeAN(originalSub.BSB04, 3, "BSB04"); // Substructure Type
                sanitizedSub.BSB05 = SanitizeAN(originalSub.BSB05, 3, "BSB05"); // Substructure Protective System
                sanitizedSub.BSB06 = SanitizeAN(originalSub.BSB06, 3, "BSB06"); // Foundation Type
                sanitizedSub.BSB07 = SanitizeAN(originalSub.BSB07, 3, "BSB07"); // Foundation Protective System

                sanitized.SubstructureSets.Add(sanitizedSub);
            }
        }

        private void PopulateFeatures(SNBIRecord original, SNBIRecord sanitized, HashSet<string> fatalFeatureKeys, HashSet<string> fatalRouteKeys)
        {
            if (original.Features == null) return;

            sanitized.Features ??= new List<SNBIRecord.Feature>();
            foreach (var feature in original.Features)
            {
                var sanitizedFeature = new SNBIRecord.Feature();
                string featureKey = feature.BF01 ?? string.Empty;
                sanitizedFeature.RecordStatus = fatalFeatureKeys.Contains(featureKey) ? "Removed" : "Active";

                sanitizedFeature.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedFeature.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                sanitizedFeature.BF01 = SanitizeAN(feature.BF01, 3, "BF01");
                sanitizedFeature.BF02 = SanitizeAN(feature.BF02, 1, "BF02");
                sanitizedFeature.BF03 = TruncateAN(feature.BF03, 300, "BF03");

                sanitizedFeature.BH01 = SanitizeAN(feature.BH01, 1, "BH01");
                sanitizedFeature.BH02 = SanitizeAN(feature.BH02, 5, "BH02");
                sanitizedFeature.BH03 = SanitizeAN(feature.BH03, 1, "BH03");
                sanitizedFeature.BH04 = SanitizeAN(feature.BH04, 1, "BH04");
                sanitizedFeature.BH05 = SanitizeAN(feature.BH05, 1, "BH05");
                sanitizedFeature.BH06 = TruncateAN(feature.BH06, 120, "BH06");
                sanitizedFeature.BH07 = SanitizeNDouble(feature.BH07, 8, 3, "BH07");
                sanitizedFeature.BH08 = SanitizeNDouble(feature.BH08, 2, 0, "BH08");
                sanitizedFeature.BH09 = SanitizeNDouble(feature.BH09, 8, 0, "BH09");
                sanitizedFeature.BH10 = SanitizeNDouble(feature.BH10, 8, 0, "BH10");
                sanitizedFeature.BH11 = SanitizeNInt(feature.BH11, 4, "BH11");
                sanitizedFeature.BH12 = SanitizeNDouble(feature.BH12, 3, 1, "BH12");
                sanitizedFeature.BH13 = SanitizeNDouble(feature.BH13, 3, 1, "BH13");
                sanitizedFeature.BH14 = SanitizeNDouble(feature.BH14, 3, 1, "BH14");
                sanitizedFeature.BH15 = SanitizeNDouble(feature.BH15, 3, 1, "BH15");
                sanitizedFeature.BH16 = SanitizeNDouble(feature.BH16, 3, 1, "BH16");
                sanitizedFeature.BH17 = SanitizeNInt(feature.BH17, 3, "BH17");
                sanitizedFeature.BH18 = SanitizeAN(feature.BH18, 15, "BH18");

                sanitizedFeature.BRR01 = SanitizeAN(feature.BRR01, 2, "BRR01");
                sanitizedFeature.BRR02 = SanitizeNDouble(feature.BRR02, 3, 1, "BRR02");
                sanitizedFeature.BRR03 = SanitizeNDouble(feature.BRR03, 3, 1, "BRR03");

                sanitizedFeature.BN01 = SanitizeAN(feature.BN01, 1, "BN01");
                sanitizedFeature.BN02 = SanitizeNDouble(feature.BN02, 4, 1, "BN02");
                sanitizedFeature.BN03 = SanitizeNDouble(feature.BN03, 4, 1, "BN03");
                sanitizedFeature.BN04 = SanitizeNDouble(feature.BN04, 5, 1, "BN04");
                sanitizedFeature.BN05 = SanitizeNDouble(feature.BN05, 5, 1, "BN05");
                sanitizedFeature.BN06 = SanitizeAN(feature.BN06, 1, "BN06");

                // Process feature routes.
                sanitizedFeature.Routes = new List<SNBIRecord.Route>();
                if (feature.Routes != null)
                {
                    foreach (var route in feature.Routes)
                    {
                        var sanitizedRoute = new SNBIRecord.Route();
                        string routeKey = $"{feature.BF01 ?? string.Empty}-{route.BRT01 ?? string.Empty}";
                        sanitizedRoute.RecordStatus = fatalRouteKeys.Contains(routeKey) ? "Removed" : "Active";

                        sanitizedRoute.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                        sanitizedRoute.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                        sanitizedRoute.BRT01 = SanitizeAN(route.BRT01, 3, "BRT01");
                        sanitizedRoute.BRT02 = SanitizeAN(route.BRT02, 15, "BRT02");
                        sanitizedRoute.BRT03 = SanitizeAN(route.BRT03, 2, "BRT03");
                        sanitizedRoute.BRT04 = SanitizeAN(route.BRT04, 1, "BRT04");
                        sanitizedRoute.BRT05 = SanitizeAN(route.BRT05, 1, "BRT05");

                        sanitizedFeature.Routes.Add(sanitizedRoute);
                    }
                }
                sanitized.Features.Add(sanitizedFeature);
            }
        }

        private void PopulateSpanSets(SNBIRecord original, SNBIRecord sanitized, HashSet<string> fatalSpanKeys)
        {
            if (original.SpanSets == null) return;

            sanitized.SpanSets ??= new List<SNBIRecord.SpanSet>();
            foreach (var span in original.SpanSets)
            {
                var sanitizedSpan = new SNBIRecord.SpanSet();
                string spanKey = span.BSP01 ?? string.Empty;
                sanitizedSpan.RecordStatus = fatalSpanKeys.Contains(spanKey) ? "Removed" : "Active";

                sanitizedSpan.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedSpan.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                sanitizedSpan.BSP01 = SanitizeAN(span.BSP01, 3, "BSP01");
                sanitizedSpan.BSP02 = SanitizeNDouble(span.BSP02, 4, 0, "BSP02");
                sanitizedSpan.BSP03 = SanitizeNDouble(span.BSP03, 3, 0, "BSP03");
                sanitizedSpan.BSP04 = SanitizeAN(span.BSP04, 3, "BSP04");
                sanitizedSpan.BSP05 = SanitizeAN(span.BSP05, 3, "BSP05");
                sanitizedSpan.BSP06 = SanitizeAN(span.BSP06, 4, "BSP06");
                sanitizedSpan.BSP07 = SanitizeAN(span.BSP07, 3, "BSP07");
                sanitizedSpan.BSP08 = SanitizeAN(span.BSP08, 2, "BSP08");
                sanitizedSpan.BSP09 = SanitizeAN(span.BSP09, 4, "BSP09");
                sanitizedSpan.BSP10 = SanitizeAN(span.BSP10, 3, "BSP10");
                sanitizedSpan.BSP11 = SanitizeAN(span.BSP11, 4, "BSP11");
                sanitizedSpan.BSP12 = SanitizeAN(span.BSP12, 3, "BSP12");
                sanitizedSpan.BSP13 = SanitizeAN(span.BSP13, 3, "BSP13");

                sanitized.SpanSets.Add(sanitizedSpan);
            }
        }

        private void PopulatePostingStatuses(SNBIRecord original, SNBIRecord sanitized, HashSet<string> fatalPSKeys)
        {
            if (original.PostingStatuses == null) return;

            sanitized.PostingStatuses ??= new List<SNBIRecord.PostingStatus>();
            foreach (var status in original.PostingStatuses)
            {
                var sanitizedStatus = new SNBIRecord.PostingStatus();
                string psKey = status.BPS02 ?? string.Empty;
                sanitizedStatus.RecordStatus = fatalPSKeys.Contains(psKey) ? "Removed" : "Active";

                sanitizedStatus.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedStatus.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                sanitizedStatus.BPS01 = SanitizeAN(status.BPS01, 4, "BPS01");
                sanitizedStatus.BPS02 = SanitizeDate(status.BPS02, "BPS02"); // Returns "YYYYMMDD" string.

                sanitized.PostingStatuses.Add(sanitizedStatus);
            }
        }

        private void PopulatePostingEvaluations(SNBIRecord original, SNBIRecord sanitized, HashSet<string> fatalPEKeys)
        {
            if (original.PostingEvaluations == null) return;

            sanitized.PostingEvaluations ??= new List<SNBIRecord.PostingEvaluation>();
            foreach (var eval in original.PostingEvaluations)
            {
                var sanitizedEval = new SNBIRecord.PostingEvaluation();
                string peKey = eval.BEP01 ?? string.Empty;
                sanitizedEval.RecordStatus = fatalPEKeys.Contains(peKey) ? "Removed" : "Active";

                sanitizedEval.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedEval.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                sanitizedEval.BEP01 = SanitizeAN(eval.BEP01, 15, "BEP01");
                sanitizedEval.BEP02 = SanitizeNDouble(eval.BEP02, 4, 2, "BEP02");
                sanitizedEval.BEP03 = SanitizeAN(eval.BEP03, 17, "BEP03");
                sanitizedEval.BEP04 = SanitizeAN(eval.BEP04, 15, "BEP04");

                sanitized.PostingEvaluations.Add(sanitizedEval);
            }
        }

        private void PopulateInspections(SNBIRecord original, SNBIRecord sanitized)
        {
            if (original.Inspections == null) return;

            sanitized.Inspections ??= new List<SNBIRecord.Inspection>();
            foreach (var inspection in original.Inspections)
            {
                var sanitizedInspection = new SNBIRecord.Inspection();
                // Default inspections to "Active" as no fatal keys provided.
                sanitizedInspection.RecordStatus = "Active";

                sanitizedInspection.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedInspection.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                sanitizedInspection.BIE01 = SanitizeAN(inspection.BIE01, 1, "BIE01");
                sanitizedInspection.BIE02 = SanitizeAN(inspection.BIE02, 1, "BIE02");
                sanitizedInspection.BIE03 = SanitizeDate(inspection.BIE03, "BIE03");
                sanitizedInspection.BIE04 = TruncateAN(inspection.BIE04, 15, "BIE04");
                sanitizedInspection.BIE05 = SanitizeNInt(inspection.BIE05, 2, "BIE05");
                sanitizedInspection.BIE06 = SanitizeDate(inspection.BIE06, "BIE06");
                sanitizedInspection.BIE07 = SanitizeAN(inspection.BIE07, 1, "BIE07");
                sanitizedInspection.BIE08 = SanitizeDate(inspection.BIE08, "BIE08");
                sanitizedInspection.BIE09 = SanitizeDate(inspection.BIE09, "BIE09");
                sanitizedInspection.BIE10 = SanitizeDate(inspection.BIE10, "BIE10");
                sanitizedInspection.BIE11 = TruncateAN(inspection.BIE11, 300, "BIE11");
                sanitizedInspection.BIE12 = TruncateAN(inspection.BIE12, 120, "BIE12");

                sanitized.Inspections.Add(sanitizedInspection);
            }
        }

        private void PopulateElements(SNBIRecord original, SNBIRecord sanitized, HashSet<string> fatalElementKeys)
        {
            if (original.Elements == null) return;

            sanitized.Elements ??= new List<SNBIRecord.Element>();
            foreach (var element in original.Elements)
            {
                var sanitizedElement = new SNBIRecord.Element();
                string elementKey = $"{element.BE01 ?? string.Empty}-{element.BE02 ?? string.Empty}";
                sanitizedElement.RecordStatus = fatalElementKeys.Contains(elementKey) ? "Removed" : "Active";

                sanitizedElement.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedElement.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                sanitizedElement.BE01 = SanitizeAN(element.BE01, 4, "BE01");
                sanitizedElement.BE02 = SanitizeAN(element.BE02, 4, "BE02");
                sanitizedElement.BE03 = SanitizeElementQuantity(element.BE03, 8, "BE03");
                sanitizedElement.BCS01 = SanitizeElementQuantity(element.BCS01, 8, "BCS01");
                sanitizedElement.BCS02 = SanitizeElementQuantity(element.BCS02, 8, "BCS02");
                sanitizedElement.BCS03 = SanitizeElementQuantity(element.BCS03, 8, "BCS03");
                sanitizedElement.BCS04 = SanitizeElementQuantity(element.BCS04, 8, "BCS04");

                sanitized.Elements.Add(sanitizedElement);
            }
        }

        private void PopulateWorks(SNBIRecord original, SNBIRecord sanitized)
        {
            if (original.Works == null) return;

            sanitized.Works ??= new List<SNBIRecord.Work>();
            foreach (var work in original.Works)
            {
                var sanitizedWork = new SNBIRecord.Work();
                // Default to "Active" as no fatal keys are defined for Works.
                sanitizedWork.RecordStatus = "Active";

                sanitizedWork.BID01 = SanitizeAN(original.BID01, 15, "BID01");
                sanitizedWork.BL01 = SanitizeNInt(original.BL01, 2, "BL01");
                sanitizedWork.BW02 = SanitizeNInt(work.BW02, 4, "BW02");
                sanitizedWork.BW03 = TruncateAN(work.BW03, 120, "BW03");

                sanitized.Works.Add(sanitizedWork);
            }
        }
    }

}
