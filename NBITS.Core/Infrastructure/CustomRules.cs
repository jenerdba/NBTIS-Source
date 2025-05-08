using FluentValidation;
using NBTIS.Core.DTOs;
using NBTIS.Core.Services;
using NBTIS.Core.Utilities;
using NBTIS.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using static NBTIS.Core.DTOs.SNBIRecord;
using static NBTIS.Core.Utilities.Constants;

namespace NBTIS.Core.Infrastructure
{

    public class CustomRules
    {
        private static LOVValidatorFactory? validatorFactory;

        /// <summary>
        /// Initializes the CustomRules by creating a LOVValidatorFactory with the given DataContext.
        /// Must be called at application startup before using other CustomRules methods.
        /// </summary>
        public static void Initialize(DataContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (validatorFactory != null)
            {
                // Already initialized – optionally throw or simply return
                return;
            }
            // Instantiate the factory with the provided DataContext
           validatorFactory = new LOVValidatorFactory(context);
        }

        public static bool IsValidBorderBridgeItems(SNBIRecord snbiRecord)
        {
            if (snbiRecord.BL07 != "N")
            {
                bool isBorderBridge = false;

                if (int.TryParse(snbiRecord.BL10, out int bl10Value))
                {
                    if (bl10Value != snbiRecord.BL01)
                    {
                        isBorderBridge = true;
                    }
                }
                else if (snbiRecord.BL10 == "CA" || snbiRecord.BL10 == "MX")
                {
                    isBorderBridge = true;
                }

                if (isBorderBridge)
                {
                    bool hasInvalidItems =
                        snbiRecord.GetType().GetProperties()
                        .Where(prop => !new[] {
                    "BID01", "BID03", "BL01", "BL02", "BL03", "BL04", "BL07", "BL08", "BL09", "BL10",
                    "BL12", "BF01", "BF02", "BF03", "BRT01", "BRT02", "BRT03", "BRT04", "BRT05",
                    "BH03", "BH06", "BH07", "BH18"
                        }.Contains(prop.Name))
                        .Any(prop =>
                        {
                            var value = prop.GetValue(snbiRecord) as string;
                            return !string.IsNullOrEmpty(value);
                        });

                    return !hasInvalidItems;
                }
            }

            return true;
        }
        public static bool HasWaterwayFeature(SNBIRecord snbiRecord)
        {
            return snbiRecord.Features.Any(feature =>
                Regex.IsMatch(feature.BF01 ?? string.Empty, Constants.WaterwayFeatureRegex));
        }

        public static bool HasWaterwayFeatureAndRelief(SNBIRecord snbiRecord)
        {
            return snbiRecord.Features.Any(feature =>
                Regex.IsMatch(feature.BF01 ?? string.Empty, Constants.WaterwayFeatureReliefRegex));
        }

        public static bool IsValidBridgeNumber(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidStateCode(object value)
        {
            if (IsNullOrEmpty(value))
                return false;

            var validator = validatorFactory.Create("LookupStates");

            return validator.IsValidStateCode(value.ToString());
        }

        public static bool IsValidStateMatch(object value1, object value2)
        {
            if (IsNullOrEmpty(value1) || IsNullOrEmpty(value2))
            {
                return true;
            }

            if (value2 is string value2Str && int.TryParse(value2Str, out int value2Int))
            {
                return value1.Equals(value2Int);
            }

            return true;
        }

        public static bool IsValidCountyCode(object value1, object value2)
        {
            if (IsNullOrEmpty(value1) || IsNullOrEmpty(value2))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupCounties");

            return validator.IsValidCountyCode(value1.ToString(), value2.ToString());
        }

        public static bool IsValidBL03(object bl03)
        {
            if (!HasMaxLengthString(bl03, 5))
            {
                return true;  //Do not validate;
            }

            if (IsNullOrEmpty(bl03))
            {
                return false;
            }

            return IsPositiveIntegerOr0(bl03);
        }

        //BL04 Highway Agency District
        public static bool IsValidBL04(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            if (!HasMaxLengthString(value, 2))
            {
                return false;
            }
            return true;
        }

        //BL05 Latitude
        public static bool IsValidBL05(object value)
        {
            // Check if value is null or only white spaces
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            // Try parsing the input into a decimal
            if (!decimal.TryParse(value.ToString().Trim(), out decimal latitude))
            {
                return false;
            }

            // Check if the latitude is within the valid range. The m suffix on a number designates it as a decimal literal.
            if (latitude < -90m || latitude > 90m)
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBL05_GeospatialPolarity(object bl01, object bl05)
        {
            if (bl01 == null || bl05 == null)
            {
                return true;
            }

            string stateCodeStr = bl01?.ToString();
            string latitudeStr = bl05?.ToString();

            // Determine the actual polarity of the latitude
            string actualPolarity = latitudeStr.StartsWith("-") ? "-" : "+";

            // Get the expected polarity for this state code and propertyType
            string? expectedPolarity = GetExpectedPolarity(stateCodeStr, "latitude");

            if (expectedPolarity == null)
            {
                return true; //Cannot validate.
            }

            // Compare actual polarity with expected polarity
            bool isValid = actualPolarity == expectedPolarity;

            return isValid;
        }

        //BL06 Longitude
        public static bool IsValidBL06(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            // Try parsing the input into a decimal
            if (!decimal.TryParse(value.ToString().Trim(), out decimal longitude))
            {
                return false;
            }

            // Check if the longitude is within the valid range
            if (longitude < -180m || longitude > 180m)
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBL06_GeospatialPolarity(object bl01, object bl06)
        {
            if (bl01 == null || bl06 == null)
            {
                return true;
            }

            string stateCodeStr = bl01?.ToString();
            string longitudeStr = bl06?.ToString();

            // Determine the actual polarity of the longitude
            string actualPolarity = longitudeStr.StartsWith("-") ? "-" : "+";

            string? expectedPolarity = GetExpectedPolarity(stateCodeStr, "longitude");

            if (expectedPolarity == null)
            {
                return true; //Cannot validate.
            }

            // Compare actual polarity with expected polarity
            bool isValid = actualPolarity == expectedPolarity;

            return isValid;
        }

        private static string? GetExpectedPolarity(string stateCodeStr, string propertyType)
        {
            if (PolarityCodes.TryGetValue(stateCodeStr, out var polarities))
            {
                // Use the specific polarity for this state
                return propertyType == "longitude" ? polarities.LongitudePolarity : polarities.LatitudePolarity;
            }
            else
            {
                // Default polarities for other states
                if (propertyType == "longitude")
                {
                    return "-";
                }
                else if (propertyType == "latitude")
                {
                    return "+";
                }
                else
                {
                    // Invalid propertyType
                    return null;
                }
            }
        }

        //BL08 Border Bridge State or Country Code
        public static bool IsValidBL08(object bl07, object bl08)
        {
            // Trim bl07 and bl08 once and assign them to string variables
            string bl07str = bl07?.ToString();
            string bl08str = bl08?.ToString();

            // Check if BL07 is null or equals "N" after trimming
            if (string.IsNullOrEmpty(bl07str) || bl07str == "N")
            {
                return string.IsNullOrWhiteSpace(bl08str);
            }

            // Check if BL08 is null or whitespace after trimming
            if (string.IsNullOrWhiteSpace(bl08str))
            {
                return false;
            }

            // Check for valid Country Codes
            if (bl08str == "CA" || bl08str == "MX")
            {
                return true;
            }

            // Perform existing state code validation
            return IsValidStateCode(bl08str);
        }


        //BL08 shall not be equal to BL01
        public static bool CrossCheckBL08_BL01(object bl01, object bl07, object bl08)
        {
            // Convert inputs to strings for easier comparison
            string bl07str = bl07?.ToString();
            string bl01str = bl01?.ToString();
            string bl08str = bl08?.ToString();

            // If BL01 or BL08 is null or empty, return true
            if (string.IsNullOrWhiteSpace(bl01str) || string.IsNullOrWhiteSpace(bl08str))
            {
                return true;
            }

            // Check if BL07 is not "N" and not null or empty
            if (!string.IsNullOrWhiteSpace(bl07str) && bl07str != "N")
            {
                // Check if BL08 is not equal to BL01
                return bl08str != bl01str;
            }

            return true; // If BL07 is "N" or null or empty, return true
        }

        //BL09 Border Bridge Inspection Responsibility
        public static bool IsValidBL09(object bl07, object bl09)
        {
            // Trim BL07 and BL09 at the start
            string bl07str = bl07?.ToString();
            string bl09str = bl09?.ToString();

            // Check if BL07 is not null, not whitespace, and not "N" after trimming
            if (!string.IsNullOrEmpty(bl07str) && bl07str != "N")
            {
                // Return false if BL09 is null or whitespace after trimming
                if (string.IsNullOrWhiteSpace(bl09str))
                {
                    return false;
                }

                // Check if BL09 has valid values ("0", "1", or "2")
                return bl09str == "0" || bl09str == "1" || bl09str == "2";
            }

            // If BL07 is "N" or null or empty, return true
            return true;
        }


        public static bool CrossCheckBL07_BL09(object bl07, object bl09)
        {
            string bl07str = bl07?.ToString();
            string bl09str = bl09?.ToString();

            // If BL07 is 'N', then BL10 should not be reported (should be null or empty)
            if (bl07str == "N")
            {
                return string.IsNullOrWhiteSpace(bl09str);
            }

            return true;
        }

        //BL10 Border Bridge Designated Lead State
        public static bool IsValidBL10(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                string bl07str = snbiRecord.BL07?.ToString();
                string bl10str = snbiRecord.BL10?.ToString();
                string bl01str = snbiRecord.BL01?.ToString();
                string bl08str = snbiRecord.BL08?.ToString();

                // Return true if BL07 is "N" or null or empty since no further validation is needed
                if (string.IsNullOrWhiteSpace(bl07str) || bl07str == "N")
                {
                    return true;
                }

                // BL10 must be reported and non-empty if BL07 is not "N"
                if (string.IsNullOrWhiteSpace(bl10str))
                {
                    return false;
                }

                // For Border Bridges that cross international borders (BL08 is "CA" or "MX"), BL10 should be equal to BL01
                //if ((bl08str == "CA" || bl08str == "MX") && bl10str != bl01str)
                //{
                //    return false;
                //}

                // Otherwise, BL10 should be equal to either BL01 or BL08
                return bl10str == bl01str || bl10str == bl08str;
            }

            return true;
        }


        public static bool CrossCheckBL07_BL10(object bl07, object bl10)
        {
            string bl07str = bl07?.ToString();
            string bl10str = bl10?.ToString();

            // If BL07 is 'N', then BL10 should not be reported (should be null or empty)
            if (bl07str == "N")
            {
                return string.IsNullOrWhiteSpace(bl10str);
            }

            return true;
        }

        //B.L.12 - Metropolitan Planning Organization
        public static bool IsValidBL12(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                // Check each feature in the Features collection
                if (snbiRecord.Features != null)
                {
                    foreach (var feature in snbiRecord.Features)
                    {
                        // Check if BF01, BH03, and BL12 are not null before trimming or comparing
                        if ((feature.BF01?.Trim() == "H01") &&
                            (feature.BH03?.Trim() == "Y") &&
                            snbiRecord.BL12 == null)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool IsValidBCL01_BCL02(object value)
        {
            string? stringValue = value?.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            string trimmedValue = stringValue.Trim();

            // Create the validator for the given lookup
            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BCL01", trimmedValue);

        }

        public static bool IsValidBCL03(object value)
        {
            string? stringValue = value?.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            string trimmedValue = stringValue.Trim();

            // Create the validator for the given lookup
            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BCL03", trimmedValue);
        }

        public static bool IsValidBCL04(object value)
        {
            string str = value?.ToString().Trim();

            // Check if the trimmed string is null or empty
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (Constants.TemporaryCodes.Contains(str))
            {
                TemporaryCounts.BCL04Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BCL04", str);
        }


        public static bool IsValidBCL05(object value)
        {
            string str = value?.ToString().Trim();

            // Check if the trimmed string is null or whitespace
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BCL05", str);
        }


        public static bool IsValidBRH01(object value)
        {
            string str = value?.ToString().Trim() ?? string.Empty;

            if (IsNullOrEmpty(str) || str.Length > 4)
            {
                return false;
            }
            if (Constants.TemporaryCodes.Contains(str)) { TemporaryCounts.BRH01Count++; }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BRH01", value.ToString());
        }

        public static bool IsValidBRH02(object value)
        {
            string str = value?.ToString() ?? string.Empty;

            if (IsNullOrEmpty(str) || str.Length > 4)
            {
                return false;
            }
            if (Constants.TemporaryCodes.Contains(str)) { TemporaryCounts.BRH02Count++; }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BRH01", value.ToString()); //BRH01 and BRH02 have the same LOV.
        }


        /////******* BRIDGE GEOMETRY *******/////////

        //BG01 - NBIS Bridge Length
        //Error: NBIS bridge length is either null or not a numeric
        public static bool IsValidBG01_2(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 7))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            // Check if the number of decimal places matches the specified amount
            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool IsValidBG01_3(object value)
        {
            if (value != null)
            {
                string str = value.ToString();
                if (double.TryParse(str, out double length))
                {
                    return length > 20; // Returns true if length is greater than 20
                }
            }

            return true;
        }

        //BG02 - Total Bridge Length
        public static bool IsValidBG02(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 7))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            // Check if the number of decimal places matches the specified amount
            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool CrossCheckBG01_BG02(object bg01, object bg02)
        {
            if (double.TryParse(bg01?.ToString(), out double numBG01) & double.TryParse(bg02?.ToString(), out double numBG02))
            {
                // Check if BG01 is not greater than BG02
                if (numBG01 > numBG02)
                {
                    return false;
                }
            }

            return true;
        }

        //BG03 - Maximum Span Length
        public static bool IsValidBG03(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 5))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            if (numericValue <= 0)
                return false;

            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        //BG04 - Minimum Span Length
        public static bool IsValidBG04(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 5))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            // Check if the number of decimal places matches the specified amount
            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool IsValidBG05(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 4))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            // Check if the number of decimal places matches the specified amount
            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool IsValidBG06(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 4))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            //// Check if the number of decimal places matches the specified amount
            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool IsValidBG05_BG06(object bg05, object bg06, object bg14)
        {
            if (bg05 is double bg05Value && bg06 is double bg06Value)
            {
                if (bg06Value > bg05Value)
                {
                    if (bg14 is string bg14Value)
                    {
                        return bg14Value == "Y";
                    }
                    return false; // Error if Bridge Width Out-To-Out BG05 < Bridge Width Curb-to-Curb and BG06BG14 is null or not "Y"
                }
            }
            return true;


        }

        public static bool IsValidBG07(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 3))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue < 0)
                return false;

            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool IsValidBG08(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 3))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue < 0)
                return false;

            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool IsValidBG09(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 3))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        public static bool IsValidBG10(object value)
        {
            string str = value?.ToString().Trim();

            // Check if the trimmed string is null or empty
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BG10", str);
        }


        //BG11 - Skew
        public static bool IsValidBG11(object value)
        {
            string? strBLR05 = value?.ToString();

            if (string.IsNullOrWhiteSpace(strBLR05))
            {
                return false;
            }

            // Try parsing the string to a double and validate its range
            if (double.TryParse(strBLR05, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                return number >= 0 && number <= 99;
            }

            return false;

        }


        //BG12 - Curved Bridge
        public static bool IsValidBG12(object value)
        {
            string str = value?.ToString().Trim();

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BG12", str);
        }

        //BG13 - Maximum Bridge Height
        public static bool IsValidBG13(object value, int decimalPlaces)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue))
                return false;

            if (!HasMaxLengthDouble(value, 4))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            int actualDecimalPlaces = GetDecimalPlaces(strValue);

            if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
                return false;

            return true;
        }

        //BG15 - Irregular Deck Area - WA
        public static bool IsValidBG15(object value)
        {
            string strValue = value?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(strValue)) // null is a valid value for BG15
                return true;

            if (!HasMaxLengthDouble(value, 10))
                return true; //Skip validation

            // Attempt to parse the value as a double
            if (!double.TryParse(strValue, out double numericValue))
                return false;

            // Check if the value is positive
            if (numericValue <= 0)
                return false;

            //int actualDecimalPlaces = GetDecimalPlaces(strValue);

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }

        //Design Load
        public static bool IsValidBLR01(object value)
        {
            string str = value?.ToString().Trim();

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BLR01", str);

        }

        //Design Method
        public static bool IsValidBLR02(object value)
        {
            string str = value?.ToString().Trim();

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BLR02", str);
        }

        public static bool IsValidBLR03(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                string? strBLR03 = snbiRecord.BLR03?.Trim();
                var doubleBLR05 = snbiRecord.BLR05;
                var doubleBLR06 = snbiRecord.BLR06;

                // Check if either BLR05 or BLR06 is not null or empty
                if (!IsNullOrEmpty(doubleBLR05) || !IsNullOrEmpty(doubleBLR06))
                {
                    if (string.IsNullOrEmpty(strBLR03))
                    {
                        return false; // BLR03 must not be empty if BLR05 or BLR06 is not empty
                    }
                    // Check if BLR03 is a valid date in YYYYMMDD format
                    return IsValidYYYYMMDD(strBLR03);
                }

            }

            return true;
        }

        public static bool IsValidBLR04(object blr04, object blr05, object blr06)
        {
            string? strBLR04 = blr04?.ToString();
            string? strBLR05 = blr05?.ToString();
            string? strBLR06 = blr06?.ToString();

            // Check if either BLR05 or BLR06 is not null or empty
            if (!string.IsNullOrEmpty(strBLR05) || !string.IsNullOrEmpty(strBLR06))
            {
                if (string.IsNullOrEmpty(strBLR04))
                {
                    return false; // BLR04 must not be empty if BLR05 or BLR06 is not empty
                }

                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BLR04", strBLR04);
            }

            return true;
        }

        /// <summary>
        /// BLR05 - Inventory Load Rating Factor
        /// BLR06 - Operating Load Rating Factor
        /// BLR07 - Controlling Legal Load Rating Factor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static bool IsValidLoadRatingFactor(object value, int decimalPlaces)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; // Skip validation

            string strValue = value.ToString().Trim();

            if (!decimal.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number))
                return false; // Invalid number format

            if (number < 0 || number > 99.99m)
                return false; // Number is negative or exceeds maximum allowed value

            int actualDecimalPlaces = GetDecimalPlaces(strValue);
            if (actualDecimalPlaces > decimalPlaces)
                return false;

            return true;
        }

        private static int GetDecimalPlaces(string? strValue)
        {
            strValue = strValue?.Trim();

            int decimalPointIndex = strValue.IndexOf('.');
            if (decimalPointIndex < 0)
            {
                // No decimal point means zero decimal places
                return 0;
            }
            else
            {
                // Calculate the number of characters after the decimal point
                return strValue.Length - decimalPointIndex - 1;
            }
        }


        //public static bool IsReportedBLR06(object record)
        //{
        //    if (record is SNBIRecord snbiRecord)
        //    {
        //        string? strBLR06 = snbiRecord.BLR06?.ToString();
        //        string? strBLR03 = snbiRecord.BLR03?.ToString();
        //        string? strBLR04 = snbiRecord.BLR04?.ToString();
        //        string? strBLR05 = snbiRecord.BLR05?.ToString();

        //        bool isAnyOtherReported = !string.IsNullOrEmpty(strBLR03) ||
        //                                  !string.IsNullOrEmpty(strBLR04) ||
        //                                  !string.IsNullOrEmpty(strBLR05);

        //        if (isAnyOtherReported)
        //        {
        //            return !string.IsNullOrEmpty(strBLR06);
        //        }

        //        return true;
        //    }

        //    return true;
        //}

        //public static bool DoNotReportBLR06(object record)
        //{
        //    if (record is SNBIRecord snbiRecord)
        //    {
        //        // Ensure that all the other related fields are not reported (null or empty)
        //        bool othersNotReported = string.IsNullOrEmpty(snbiRecord.BLR03?.ToString()) &&
        //                                 string.IsNullOrEmpty(snbiRecord.BLR04?.ToString()) &&
        //                                 string.IsNullOrEmpty(snbiRecord.BLR05?.ToString());

        //        bool blr06Reported = !string.IsNullOrEmpty(snbiRecord.BLR06?.ToString());

        //        // BLR06 is reported incorrectly if it is reported while others are not reported
        //        return !(othersNotReported && blr06Reported);
        //    }

        //    return true;
        //}

        //public static bool IsReportedBLR07(object record)
        //{
        //    if (record is SNBIRecord snbiRecord)
        //    {
        //        string? strBLR07 = snbiRecord.BLR07?.ToString();
        //        string? strBLR03 = snbiRecord.BLR03?.ToString();
        //        string? strBLR04 = snbiRecord.BLR04?.ToString();
        //        string? strBLR05 = snbiRecord.BLR05?.ToString();
        //        string? strBLR06 = snbiRecord.BLR06?.ToString();

        //        bool isAnyOtherReported = !string.IsNullOrEmpty(strBLR03) ||
        //                                  !string.IsNullOrEmpty(strBLR04) ||
        //                                  !string.IsNullOrEmpty(strBLR05) ||
        //                                  !string.IsNullOrEmpty(strBLR06);

        //        if (isAnyOtherReported)
        //        {
        //            return !string.IsNullOrEmpty(strBLR07);
        //        }

        //        return true;
        //    }

        //    return true;
        //}

        //public static bool DoNotReportBLR07(object record)
        //{
        //    if (record is SNBIRecord snbiRecord)
        //    {
        //        bool othersNotReported = string.IsNullOrEmpty(snbiRecord.BLR03?.ToString()) &&
        //                                 string.IsNullOrEmpty(snbiRecord.BLR04?.ToString()) &&
        //                                 string.IsNullOrEmpty(snbiRecord.BLR05?.ToString()) &&
        //                                 string.IsNullOrEmpty(snbiRecord.BLR06?.ToString());

        //        bool blr07Reported = !string.IsNullOrEmpty(snbiRecord.BLR07?.ToString());

        //        return !(othersNotReported && blr07Reported);
        //    }

        //    return true;
        //}

        //BLR08 - Routing Permit Loads

        public static bool IsValidBLR08(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; // Skip validation

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BLR08", value.ToString());
        }

        //******************* Subsection 6.1 - Inpsection Requirements *************//

        //BIR01 - NSTM Inspection Required
        public static bool IsValidBIR01(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                var validator = validatorFactory.Create("LookupValues");
                bool isBIR01Valid = validator.IsValidCode("BIR01", snbiRecord.BIR01 ?? string.Empty);

                if (!isBIR01Valid && !string.IsNullOrWhiteSpace(snbiRecord.BIR01))
                {
                    return false;
                }

                //var spanSteelCodes = new HashSet<string> { "S01", "S02", "S03", "S04", "S05", "SX" };
                //var substructureSteelCodes = new HashSet<string> { "S01", "S02", "S03", "S04", "S05", "S06", "SX" };

                bool hasSteelCodes = HasSteelCodes(snbiRecord, Constants.spanSteelCodes, Constants.substructureSteelCodes);

                if (hasSteelCodes && string.IsNullOrWhiteSpace(snbiRecord.BIR01))
                {
                    return false;
                }

                return true;
            }
            return true;
        }

        private static bool HasSteelCodes(SNBIRecord snbiRecord, HashSet<string> spanCodes, HashSet<string> substructureCodes)
        {
            return (snbiRecord.SpanSets?.Any(s => spanCodes.Contains(s.BSP04)) ?? false) ||
                   (snbiRecord.SubstructureSets?.Any(s => substructureCodes.Contains(s.BSB03)) ?? false);
        }

        //temporarily removed as per US-43800
        //public static bool DoNotReportBIR01(object record)
        //{
        //    if (record is SNBIRecord snbiRecord)
        //    {
        //        bool hasSteelCodes = HasSteelCodes(snbiRecord, Constants.spanSteelCodes, Constants.substructureSteelCodes);

        //        // If there are no steel codes, BIR01 must be null or white space
        //        if (!hasSteelCodes)
        //        {
        //            return string.IsNullOrWhiteSpace(snbiRecord.BIR02);
        //        }

        //        return true;
        //    }
        //    return true;
        //}

        //BIR02 - Fatigue Details
        public static bool IsValidBIR02(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                bool hasSteelCodes = HasSteelCodes(snbiRecord, Constants.spanSteelCodes, Constants.substructureSteelCodes);

                // If steel codes are present, BIR02 must not be null or empty and must be "Y" or "N"
                if (hasSteelCodes)
                {
                    if (IsNullOrEmpty(snbiRecord.BIR02) || (snbiRecord.BIR02 != "Y" && snbiRecord.BIR02 != "N"))
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        public static bool DoNotReportBIR02(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                bool hasSteelCodes = HasSteelCodes(snbiRecord, Constants.spanSteelCodes, Constants.substructureSteelCodes);

                // If there are no steel codes, BIR02 must be null or white space
                if (!hasSteelCodes)
                {
                    return string.IsNullOrWhiteSpace(snbiRecord.BIR02);
                }

                return true;
            }
            return true;
        }


        //BIR03 - Underwater Inpection Required
        public static bool IsValidBIR03(object record)
        {
            if (record is SNBIRecord snbiRecord && HasWaterwayFeature(snbiRecord))
            {
                string bir03 = snbiRecord.BIR03?.Trim();

                if (IsNullOrEmpty(snbiRecord.BIR03) || bir03 != "Y" && bir03 != "N")
                {
                    return false;
                }
            }

            return true;
        }


        public static bool DoNotReportBIR03(object record)
        {
            if (record is SNBIRecord snbiRecord && !HasWaterwayFeatureAndRelief(snbiRecord))
            {
                return string.IsNullOrWhiteSpace(snbiRecord.BIR03);

            }
            return true;
        }

        //BIR04 - Complex Feature
        public static bool IsValidBIR04(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // Check if the value is either 'Y' or 'N'
            return value.Trim() == "Y" || value.Trim() == "N";
        }


        ///////*********************** BRIDGE CONDITION RULES *********************///////


        //BC01 - BC07  Condition Ratings; 
        public static bool IsValidBC01_BC07(object value)
        {
            string stringValue = value?.ToString().Trim();

            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BC01", stringValue);
            }

            return false;
        }

        public static bool IsValidBC01_1(object record)
        {
            //If its not null
            if (record is SNBIRecord snbiRecord)
            {
                if (!IsValidBC01_BC07(snbiRecord.BC01))
                {
                    return true;  //Do not validate
                }

                var validator = validatorFactory.Create("LookupValues");

                foreach (var span in snbiRecord.SpanSets)
                {
                    if (span.BSP09?.Trim() != "0")
                    {
                        // Validate using the validator 
                        if (!validator.IsValidCode("BC01", snbiRecord.BC01))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        public static bool IsValidBC01_2(object record)
        {
            //if its not null
            if (record is SNBIRecord snbiRecord)
            {
                if (!IsValidBC01_BC07(snbiRecord.BC01))
                {
                    return true;  //Do not validate
                }

                // Return true if there are no spans to validate
                if (snbiRecord.SpanSets == null || snbiRecord.SpanSets.Count == 0)
                {
                    return true;
                }

                // Check if all spans have BSP01 equal to "0"
                bool allBSP01AreZero = snbiRecord.SpanSets.All(span => span.BSP01?.Trim() == "0");

                if (allBSP01AreZero)
                {
                    // BC01 must be "N" if all BSP01 are "0"
                    return snbiRecord.BC01?.Trim() == "N";
                }

                return true;
            }
            return true;
        }

        //BC02 - Superstructure Condition Rating
        public static bool CrossCheckBC02_BSP01(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (!IsValidBC01_BC07(snbiRecord.BC02))
                {
                    return true;  //Do not validate
                }

                var validator = validatorFactory.Create("LookupValues");

                foreach (var span in snbiRecord.SpanSets)
                {
                    // Check if Span Configuration Designation is not C## or V##
                    bool isCulvertOrWideningSpan = !string.IsNullOrEmpty(span.BSP01) && Regex.IsMatch(span.BSP01, Constants.CulvertSpanRegex);

                    if (!isCulvertOrWideningSpan)
                    {
                        if (!validator.IsValidCode("BC02", snbiRecord.BC02))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Additional logic: Check if any span is C## or V## and BSP05 <> 7
                        if (span.BSP05?.Trim() != "7")
                        {
                            if (!validator.IsValidCode("BC02", snbiRecord.BC02))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return true;
        }

        public static bool CrossCheckBC02_BSP01_BSP05(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (!IsValidBC01_BC07(snbiRecord.BC02))
                {
                    return true;  //Do not validate
                }

                // Check all spans for BSP01 being "C##" or "V##" and BSP05 being 7
                bool allSpansMatch = snbiRecord.SpanSets.All(span =>
                    (Regex.IsMatch(span.BSP01 ?? string.Empty, Constants.CulvertSpanRegex) && span.BSP05?.Trim() == "7"));

                // If all spans match the criteria and BC02 is not "N", return false (condition fails)
                if (allSpansMatch && snbiRecord.BC02?.Trim() != "N")
                {
                    return false;
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        //BC03 - Substructure condition rating is not in the valid value range of '0-9':
        //If (BSP01 <> C## or V## in ANY Spans dataset) AND (BC03 <> RANGE (0 - 9)) -> False;
        //Or (BSP01 = C## or V## in ANY Spans dataset) AND (BSP05 <>7) AND (BC03 <> RANGE (0 - 9)) -> False;
        //Else True;
        public static bool CrossCheckBC03_BSP01(object record)
        {
            if (record is not SNBIRecord snbiRecord)
            {
                // Record is not of the expected type; validation cannot proceed
                return true;
            }

            if (!IsValidBC01_BC07(snbiRecord.BC03))
            {
                return true;  // Do not validate
            }

            // Check if BC03 is within the valid range (0-9)
            bool isBC03Valid = Regex.IsMatch(snbiRecord.BC03, @"^[0-9]$");

            foreach (var span in snbiRecord.SpanSets)
            {
                if (span == null)
                {
                    continue;
                }

                // Condition 1: If BSP01 does not match C## or V## AND BC03 is not valid
                if (!Regex.IsMatch(span.BSP01 ?? string.Empty, Constants.CulvertSpanRegex) && !isBC03Valid)
                {
                    return false;
                }

                // Condition 2: If BSP01 matches C## or V## AND (BSP05 is not "7" OR "7-T") AND BC03 is not valid
                if (Regex.IsMatch(span.BSP01 ?? string.Empty, Constants.CulvertSpanRegex) && (span.BSP05 != "7" && span.BSP05 != "7-T") && !isBC03Valid)
                {
                    return false;
                }
            }

            return true;
        }


        public static bool CrossCheckBC03_BSP01_BSP05(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (!IsValidBC01_BC07(snbiRecord.BC03))
                {
                    return true;  // Do not validate
                }

                // Check all spans for BSP01 being "C##" or "V##" and BSP05 being "7" or "7-T"
                bool allSpansMatch = snbiRecord.SpanSets.All(span =>
                    (Regex.IsMatch(span.BSP01 ?? string.Empty, Constants.CulvertSpanRegex) && (span.BSP05?.Trim() == "7" || span.BSP05?.Trim() == "7-T")));

                // If all spans match the criteria and BC02 is not "N", return false (condition fails)
                if (allSpansMatch && snbiRecord.BC03?.Trim() != "N")
                {
                    return false;
                }

                return true;
            }
            return true;
        }

        //BC04 - Culvert Condition Rating

        //Rule BC04-2
        //If the Span Configuration Designation in ALL of the Spans Datasets is equal to C## or V##
        //AND the Span Continuity is equal to seven; then Substructure Condition Rating should be within the range of 0 - 9
        //(BSP01 = C## or V## in ANY Spans dataset) AND (BSP05 <>7) AND (BC03 <> RANGE (0 - 9)) - Flag
        public static bool CrossCheckBC04_0_9(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (!IsValidBC01_BC07(snbiRecord.BC04))
                {
                    return true;  // Do not validate
                }

                // Check all spans for BSP01 being "C##" or "V##" and BSP05 = "7"
                bool allSpansMatch = snbiRecord.SpanSets.All(span =>
                    Regex.IsMatch(span.BSP01 ?? string.Empty, Constants.CulvertSpanRegex) && span.BSP05?.Trim() == "7");

                if (allSpansMatch && !Regex.IsMatch(snbiRecord.BC04, @"^[0-9]$"))
                {
                    return false;
                }
            }
            return true;
        }

        //Rule BC04-3 : Spans datasets indicate this bridge is not a culvert - BC04 Culvert Condition Rating should be equal to 'N'
        public static bool CrossCheckBC04_N_1(object record)
        {
            if (record is SNBIRecord snbiRecord && snbiRecord.SpanSets != null && snbiRecord.SpanSets.Any())
            {
                if (!IsValidBC01_BC07(snbiRecord.BC04))
                {
                    return true;  // Do not validate
                }

                // Check if all spans have BSP01 values that do not match "C##" or "V##"
                bool allNonCulvert = snbiRecord.SpanSets.All(span => !Regex.IsMatch(span.BSP01 ?? "", Constants.CulvertSpanRegex));

                // If all spans do not match "C##" or "V##" and BC04 is not "N", return false
                if (allNonCulvert && snbiRecord.BC04?.Trim() != "N")
                {
                    return false;
                }

                return true;
            }

            return true;
        }

        //Rule BC04-4
        public static bool CrossCheckBC04_N_2(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (!IsValidBC01_BC07(snbiRecord.BC04))
                {
                    return true;  // Do not validate
                }

                // Check all spans for BSP01 being "M##" or "A##" or "W##" 
                bool allSpansMatch = snbiRecord.SpanSets.All(span =>
                    Regex.IsMatch(span.BSP01 ?? string.Empty, Constants.NonCulvertSpanRegex));

                if (allSpansMatch && snbiRecord.BC04?.Trim() != "N")
                {
                    return false;
                }
            }
            return true;
        }

        //BC05 - Bridge Railings Condition Rating
        //Must be 0-9
        public static bool IsValidBC05_1(object BC05, object BRH01)
        {
            if (IsNullOrEmpty(BC05) || IsNullOrEmpty(BRH01))
            {
                return true;  // Cannot validate
            }

            if (!IsValidBC01_BC07(BC05)) //We need this check so only one error for BC05 is displayed on the report.
            {
                return true;  // Do not validate
            }

            string? brh01String = BRH01 as string;
            string? bc05String = BC05 as string;

            bool isBC05Valid = bc05String != null && Regex.IsMatch(bc05String, @"^[0-9]$");

            if (brh01String != "N" && brh01String != "0")
            {
                if (isBC05Valid)
                    return true;
                else
                    return false;
            }

            return true;
        }

        //Must be N
        public static bool IsValidBC05_2(object BC05, object BRH01)
        {
            if (IsNullOrEmpty(BC05) || IsNullOrEmpty(BRH01))
            {
                return true;  // Cannot validate
            }

            if (!IsValidBC01_BC07(BC05)) //We need this check so only one error for BC05 is displayed on the report.
            {
                return true;  // Do not validate
            }

            string? brh01String = BRH01 as string;
            string? bc05String = BC05 as string;

            if (brh01String == "N" || brh01String == "0")
            {
                if (bc05String == "N")
                    return true;
                else
                    return false;
            }

            return true;

        }


        //BC06 - Bridge Railing Transitions Condition Rating
        public static bool IsValidBC06_1(object BC06, object BRH02)
        {
            if (IsNullOrEmpty(BC06) || IsNullOrEmpty(BRH02))
            {
                return true;  // Cannot validate
            }

            string? bc06String = BC06 as string;
            string? brh02String = BRH02 as string;

            if (!IsValidBC01_BC07(BC06)) //We need this check so only one error for BC06 is displayed on the report.
            {
                return true;  // Do not validate
            }

            bool isBC06Valid = bc06String != null && Regex.IsMatch(bc06String, @"^[0-9]$");

            if (brh02String != "N" && brh02String != "0")
            {
                if (isBC06Valid)
                    return true;
                else
                    return false;
            }

            return true;
        }

        public static bool IsValidBC06_2(object BC06, object BRH02)
        {
            if (IsNullOrEmpty(BC06) || IsNullOrEmpty(BRH02))
            {
                return true;  // Cannot validate
            }

            string? brh02String = BRH02 as string;
            string? bc06String = BC06 as string;

            if (!IsValidBC01_BC07(BC06)) //We need this check so only one error for BC06 is displayed on the report.
            {
                return true;  // Do not validate
            }

            if (brh02String == "N" || brh02String == "0")
            {
                if (bc06String == "N")
                    return true;
                else
                    return false;
            }

            return true;
        }

        public static bool IsValidBC08(object value)
        {
            string stringValue = value?.ToString();

            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BC08", stringValue);
            }

            return false;
        }

        //BC09 - Channel Condition Rating
        public static bool IsValidBC09(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBC09_ForWaterway(object record)
        {
            if (!(record is SNBIRecord snbiRecord))
            {
                return true;
            }

            if (IsNullOrEmpty(snbiRecord.BC09))
            {
                return true;  //skip validation
            }

            bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

            if (hasWaterwayFeature)
            {
                // Check if BC10 is a valid code from the list
                return validatorFactory.Create("LookupValues").IsValidCode("BC09", snbiRecord.BC09) && snbiRecord.BC09 != "N";
            }

            return true;
        }

        public static bool IsValidBC09_WhenNoWaterway(object record)
        {
            if (!(record is SNBIRecord snbiRecord))
            {
                return true;
            }

            if (IsNullOrEmpty(snbiRecord.BC09))
            {
                return true;  //skip validation
            }

            bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

            if (hasWaterwayFeature)
            {
                return true;  //Has waterway feature, no need to validate BC09
            }

            return snbiRecord.BC09?.Trim() == "N";

        }


        //BC10 - Channel Protection Condition Rating
        public static bool IsValidBC10(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            return true;
        }

        //Channel Protection Condition Rating should be in the valid value range of \"0-9\" or \"N\" for all bridges with a \"waterway\" feature
        public static bool IsValidBC10_ForWaterway(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (IsNullOrEmpty(snbiRecord.BC10))
                {
                    return true;  //skip validation
                }

                bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

                if (hasWaterwayFeature)
                {
                    if (string.IsNullOrWhiteSpace(snbiRecord.BC10))
                    {
                        return true;
                    }

                    // Check if BC10 is a valid code from the list
                    return validatorFactory.Create("LookupValues").IsValidCode("BC10", snbiRecord.BC10);
                }

            }
            return true;
        }

        //Channel protection condition rating should be equal to 'N' for all bridges that do not have a 'waterway' feature
        //(BF01 in ALL Features dataset <> W##) AND (BC10 <> "N") - False
        public static bool IsValidBC10_WhenNoWaterway(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (IsNullOrEmpty(snbiRecord.BC10))
                {
                    return true;  //skip validation
                }

                bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

                if (!hasWaterwayFeature && snbiRecord.BC10?.Trim() != "N")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        //BC11 Scour Condition Rating
        public static bool IsValidBC11(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBC11_ForWaterway(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (IsNullOrEmpty(snbiRecord.BC11))
                {
                    return true;  //skip validation
                }

                bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

                if (hasWaterwayFeature)
                {
                    if (Constants.TemporaryCodes.Contains(snbiRecord.BC11))
                    {
                        return true;

                    }

                    // Check if BC11 is a valid code from the list
                    return validatorFactory.Create("LookupValues").IsValidCode("BC11", snbiRecord.BC11) && snbiRecord.BC09 != "N";
                }
            }

            return true;
        }

        public static bool IsValidBC11_WhenNoWaterway(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (IsNullOrEmpty(snbiRecord.BC11))
                {
                    return true;  //skip validation
                }

                bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

                if (!hasWaterwayFeature && snbiRecord.BC11?.Trim() != "N")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        //BC14 NSTM Inspection Condition
        //If BIR01 <> “N”; valid values in range of 0-9
        public static bool IsValidBC14(object record)
        {
            if (record is not SNBIRecord snbiRecord)
            {
                return true;
            }

            if (snbiRecord.BIR01?.Trim() == "Y")
            {
                if (string.IsNullOrWhiteSpace(snbiRecord.BC14) || snbiRecord.BC14?.Trim() == "N")
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(snbiRecord.BC14) && !IsValidBC01_BC07(snbiRecord.BC14))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool DoNotReportBC14(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (snbiRecord.BIR01?.Trim() == "N" && !string.IsNullOrWhiteSpace(snbiRecord.BC14))
                {
                    return false;
                }

                return true;
            }
            else
            {
                return true;
            }
        }

        //BC15 Underwater Inspection Condition
        public static bool IsValidBC15(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                // First, check if BIR03 is "Y" and BC15 is null or whitespace. If so, return false immediately.
                if (snbiRecord.BIR03?.Trim() == "Y" && string.IsNullOrWhiteSpace(snbiRecord.BC15))
                {
                    return false;
                }

                // Then, check if BC15 is not null or whitespace and also check if it's a valid code
                if (!string.IsNullOrWhiteSpace(snbiRecord.BC15) && !IsValidBC01_BC07(snbiRecord.BC15))
                {
                    return false;
                }

                if (snbiRecord.BIR03?.Trim() == "Y" && snbiRecord.BC15?.Trim() == "N")  //Valid Condition Rating Code but not valid for BC15.
                {
                    return false;
                }

                return true;
            }
            return true;
        }

        public static bool DoNotReportBC15(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (snbiRecord.BIR03?.Trim() != "Y" && !string.IsNullOrWhiteSpace(snbiRecord.BC15))
                {
                    return false;
                }

                return true;
            }
            return true;
        }

        //**************** APPRAISAL SUBSECTION ***************/////

        //BAP01 Approach Roadway Alignment
        public static bool IsValidBAP01(object value)
        {
            // Check if the input is a string and not null or empty
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BAP01", stringValue);
            }

            return false;
        }

        public static bool IsValidBAP02(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                string str = snbiRecord.BAP02?.ToString();

                if (Constants.TemporaryCodes.Contains(str))
                {
                    TemporaryCounts.BAP02Count++;
                }

                bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

                // If there's a "W##" code in any feature dataset
                if (hasWaterwayFeature)
                {
                    if (!string.IsNullOrWhiteSpace(snbiRecord.BAP02) &&
                        validatorFactory.Create("LookupValues").IsValidCode("BAP02", snbiRecord.BAP02))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        public static bool DoNotReportBAP02(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                bool hasWaterwayFeature = HasWaterwayFeatureAndRelief(snbiRecord);

                if (!hasWaterwayFeature)
                {
                    return string.IsNullOrWhiteSpace(snbiRecord.BAP02);
                }

            }
            return true;
        }

        public static bool IsValidBAP03(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                string str = snbiRecord.BAP03?.ToString();

                if (Constants.TemporaryCodes.Contains(str))
                {
                    TemporaryCounts.BAP03Count++;
                }

                bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

                // If there's a "W##" code in any feature dataset
                if (hasWaterwayFeature)
                {
                    if (!string.IsNullOrWhiteSpace(snbiRecord.BAP03) &&
                        validatorFactory.Create("LookupValues").IsValidCode("BAP03", snbiRecord.BAP03))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        //Do not report scour vulnerability if the bridge does not cross over a waterway as indicated in BF01 Feature Type
        public static bool DoNotReportBAP03(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                bool hasWaterwayFeature = HasWaterwayFeatureAndRelief(snbiRecord);

                if (!hasWaterwayFeature)
                {
                    if (!string.IsNullOrWhiteSpace(snbiRecord.BAP03))
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        public static bool IsValidBAP04(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                bool hasWaterwayFeature = HasWaterwayFeature(snbiRecord);

                // If there's a "W##" code in any feature dataset
                if (hasWaterwayFeature)
                {
                    if (!string.IsNullOrWhiteSpace(snbiRecord.BAP04) &&
                        validatorFactory.Create("LookupValues").IsValidCode("BAP04", snbiRecord.BAP04))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        //Do not report scour plan of action if the bridge does not cross over a waterway as indicated in BF01 Feature Type
        public static bool DoNotReportBAP04(object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                bool hasWaterwayFeature = HasWaterwayFeatureAndRelief(snbiRecord);

                if (!hasWaterwayFeature)
                {
                    if (!string.IsNullOrWhiteSpace(snbiRecord.BAP04))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //BAP05 Seismic Vulnerability
        public static bool IsValidBAP05(object value)
        {
            // Check if the input is a string and not null or empty
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BAP05", stringValue);
            }

            return false;
        }

        public static bool IsValidBW01(object value)
        {
            int year;

            if (value is int intValue)
            {
                year = intValue;
            }
            else if (value is string stringValue && int.TryParse(stringValue, out year))
            {
                // The year is already assigned in the TryParse
            }
            else
            {
                return false;
            }

            // Check if the year is within a realistic range for bridge construction
            return year >= 1800 && year <= DateTime.Now.Year;
        }

        public static bool IsValidBSP01(object value)
        {
            string str = value?.ToString();

            if (value == null || (string.IsNullOrWhiteSpace(str)))
            {
                return false;
            }

            // Regex to match 'M##', 'A##', 'C##', 'V##', and 'W##' where ## is from 01 to 99
            var regex = new Regex(@"^(M|A|C|V|W)([0-9]{2})$");

            if (!regex.IsMatch(str))
                return false;

            // Extract the numeric part and parse it
            int number = int.Parse(str.Substring(1));

            return number >= 1 && number <= 99;
        }

        public static bool IsValidBSP04(object value)
        {
            if (value == null || IsNullOrEmpty(value))
            {
                return false;
            }
            string str = value?.ToString();

            if (Constants.TemporaryCodes.Contains(str))
            {
                TemporaryCounts.BSP04Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSP04", value.ToString());
        }
        public static bool IsValidBSP05(object value)
        {
            if (value == null || IsNullOrEmpty(value))
            {
                return false;
            }
            string str = value?.ToString();

            if (Constants.TemporaryCodes.Contains(str))
            {
                TemporaryCounts.BSP05Count++;
            }
            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSP05", value.ToString());
        }
        public static bool IsValidBSP06(object value)
        {
            if (value == null || IsNullOrEmpty(value))
            {
                return false;
            }
            string str = value?.ToString();

            if (Constants.TemporaryCodes.Contains(str))
            {
                TemporaryCounts.BSP06Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSP06", value.ToString());
        }

        public static bool IsValidBSP07(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return false;
            }
            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSP07", value.ToString());
        }

        public static bool IsValidBSP08(object bsp08, object bsp09)
        {
            string bsp08str = bsp08?.ToString();
            string bsp09str = bsp09?.ToString();

            if (bsp09str != "0" && (int.TryParse(bsp09str, out int num) && num != 0))
            {
                // return string.IsNullOrEmpty(bsp08str);

                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BSP08", bsp08str);
            }

            return true;

        }

        //Validate BSP08, BSP10, BSP11, BSP13
        public static bool IsNullOrEmptyBSP(object value, object bsp09)
        {
            string str = value?.ToString();
            string bsp09str = bsp09?.ToString();

            // If bsp09 is "0" or the integer 0, ensure bsp08 is null or empty.
            if (bsp09str == "0" || (int.TryParse(bsp09str, out int num) && num == 0))
            {
                return string.IsNullOrEmpty(str);
            }

            return true;

        }

        public static bool IsValidBSP09(object value)
        {
            if (value == null || IsNullOrEmpty(value))
            {
                return false;
            }
            string str = value?.ToString().Trim();

            // CR-T, CP-T, S-T, T-T and X-T
            if (Constants.TemporaryCodes.Contains(str))
            {
                TemporaryCounts.BSP09Count++;
            }
            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSP09", value.ToString());
        }

        public static bool IsValidBSP10(object bsp10, object bsp09)
        {
            string bsp09str = bsp09?.ToString();
            string bsp10str = bsp10?.ToString();

            if (Constants.TemporaryCodes.Contains(bsp10str))
            {
                TemporaryCounts.BSP10Count++;
            }

            if (bsp09str != "0" && (int.TryParse(bsp09str, out int num) && num != 0))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BSP10", bsp10str);
            }

            return true;

        }

        public static bool IsValidBSP11(object bsp11, object bsp09)
        {
            string bsp09str = bsp09?.ToString();
            string bsp11str = bsp11?.ToString();

            if (Constants.TemporaryCodes.Contains(bsp11str))
            {
                TemporaryCounts.BSP11Count++;
            }

            if (bsp09str != "0" && int.TryParse(bsp09str, out int num) && num != 0)
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BSP11", bsp11str);
            }

            return true;

        }
        public static bool IsValidBSP12(object bsp12, object bsp09)
        {
            string bsp09str = bsp09?.ToString();
            string bsp12str = bsp12?.ToString();

            if (Constants.TemporaryCodes.Contains(bsp12str))
            {
                TemporaryCounts.BSP12Count++;
            }

            var concreteCodes = new HashSet<string> { "C01", "C02", "C03", "C04", "C05", "CX" };

            if (!concreteCodes.Contains(bsp09str))
            {
                return true;
            }

            else
            {
                // BSP12 must not be null or empty if BSP09 is one of the concrete codes
                return !string.IsNullOrEmpty(bsp12str);
            }
            //else
            //{
            //    // For all other codes, BSP12 must be null or empty
            //    return string.IsNullOrEmpty(bsp12str);
            //}
        }
        public static bool IsValidBSP13(object bsp13, object bsp09)
        {
            string bsp09str = bsp09?.ToString();
            string bsp13str = bsp13?.ToString();

            if (bsp09str != "0" && (int.TryParse(bsp09str, out int num) && num != 0))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BSP13", bsp13str);
            }

            return true;

        }

        #region ELEMENTS

        //Element Number
        public static bool IsValidBE01_1(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BE01", value.ToString());
        }

        public static bool IsValidBE01_2(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            // Check if all SpanSets have BSP10 equal to 0
            bool allSpansNoWearingSurface = record.SpanSets.All(span => span.BSP10?.Trim() == "0");

            bool elementIs510 = elRecord.BE01?.Trim() == "510";

            // Validation fails if all spans have no wearing surface and the provided element is 510
            if (allSpansNoWearingSurface && elementIs510)
            {
                return false;
            }

            return true;
        }

        //Per Wendy's comment - Check only applies when BE02 EPN is in range 100 - 199 (i.e. superstructure EPNs)
        public static bool IsValidBE01_3(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            bool noCoatingSystem = record.SpanSets.All(span => !Constants.coatingSystems.Contains(span.BSP07?.Trim()));

            //superstructure EPNs
            bool isSuperstructureEPN = int.TryParse(elRecord.BE02, out int be02) && be02 is >= 100 and <= 199;

            if (noCoatingSystem && (elRecord.BE01?.Trim() == "515" || elRecord.BE01?.Trim() == "521") && isSuperstructureEPN)
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBE01_4(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            bool noCoatingSystems = record.SubstructureSets.All(x => !Constants.coatingSystems.Contains(x.BSB05));

            // substructure EPNs
            bool isSubstructureEPN = int.TryParse(elRecord.BE02, out int be02) && be02 is >= 200 and <= 299;

            if (noCoatingSystems && (elRecord.BE01?.Trim() == "515" || elRecord.BE01?.Trim() == "521") && isSubstructureEPN)
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBE01_5(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            bool allSpansNotFive = record.SpanSets.All(span => span.BSP05?.Trim() != "5");

            bool elementIs161 = elRecord.BE01?.Trim() == "161";

            if (allSpansNotFive && elRecord.BE01?.Trim() == "161")
            {
                return false;
            }

            return true;
        }

        //Superstructure element submitted with slab element. Intent for this rule is when a slab element (BE01 = 38, 54, or 65) is submitted with a superstructure element (BE01 = any 100 series)
        //then check the span datasets. If at least one BSP06 = S01 or S02; and at least one BSP06 <> S01 or S02; then no error/flag; Otherwise error/flag.
        public static bool IsValidBE01_6(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            if (Constants.slabElements.Contains(elRecord.BE01))
            {
                // Check if there's at least one Superstructure element
                bool hasSuperstructureElement = record.Elements.Any(e => Constants.superstructureElements.Contains(e.BE01));

                if (!hasSuperstructureElement)
                {
                    // Superstructure element is not present; no error
                    return true;
                }

                var s01OrS02 = new HashSet<string> { "S01", "S02" };

                bool hasS01OrS02 = record.SpanSets.Any(span => s01OrS02.Contains(span.BSP06));
                bool hasNonS01OrS02 = record.SpanSets.Any(span => !s01OrS02.Contains(span.BSP06));

                // If both conditions are met, no error
                return hasS01OrS02 && hasNonS01OrS02;
            }
            return true;
        }

        /// <summary>
        /// Culvert Elements - Error: Superstructure element submitted for culvert
        //Intent for this rule is whena culvert element(BE01 = 38, 54, or 65) is submitted with a superstructure element(BE01 = any 100 series)
        //then check the span datasets.If at least one BSP01 begins with C or V and at least one other BSP01 begins with M or A; then no error/flag;  Otherwise error/flag
        /// </summary>
        public static bool IsValidBE01_7(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            // Check if a Culvert element is submitted
            if (Constants.culvertElements.Contains(elRecord.BE01))
            {
                // Check if there is any Superstructure element submitted
                bool hasSuperstructureElement = record.Elements.Any(e =>
                Constants.superstructureElements.Contains(e.BE01));

                if (!hasSuperstructureElement)
                {
                    // No Superstructure elements submitted; no error
                    return true;
                }

                // Check span sets for required conditions
                bool hasCOrV = record.SpanSets.Any(span => span.BSP01.StartsWith("C") || span.BSP01.StartsWith("V"));
                bool hasMOrA = record.SpanSets.Any(span => span.BSP01.StartsWith("M") || span.BSP01.StartsWith("A"));

                if (hasCOrV && hasMOrA)
                {
                    // Conditions met; no error
                    return true;
                }

                return false;
            }

            // Not a Culvert element; no error
            return true;
        }

        //Culvert Elements - Error: Deck element submitted for culvert. 
        //Intent for this rule is when a culvert element (BE01 = 240, 245, 241, 242, 243, or 244) is submitted with a deck or slab element (BE01 = 12, 13, 31, 60, 38, 54, or 65)
        //then check the span datasets. If at least one BSP01 = C or V and at least one other [BSP01 = M or A and BSP09 <> 0]; then no error/flag; otherwise error/flag
        public static bool IsValidBE01_8(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            // Check if a Culvert element is submitted
            if (Constants.culvertElements.Contains(elRecord.BE01))
            {
                // Check if there is any Deck or Slab element submitted
                bool hasDeckOrSlabElement = record.Elements.Any(e =>
                    Constants.deckElements.Contains(e.BE01));

                if (!hasDeckOrSlabElement)
                {
                    // No Deck/Slab elements submitted; no error
                    return true;
                }

                // Exception conditions
                bool hasCulvertSpan = record.SpanSets.Any(span =>
                    span.BSP01.StartsWith("C") || span.BSP01.StartsWith("V"));

                bool hasDeckSpan = record.SpanSets.Any(span =>
                    (span.BSP01.StartsWith("M") || span.BSP01.StartsWith("A")) &&
                    span.BSP09 != "0");

                if (hasCulvertSpan && hasDeckSpan)
                {
                    // Exception condition met; no error
                    return true;
                }

                return false;
            }

            // Not a Culvert element; no error
            return true;
        }

        //Superstructure Elements - Error: Missing substructure element.
        //The intent for this rule is when a superstructure element (BE01 = any 100 series) and no substructure element (BE01 = any 200 series) are submitted.
        //Then check for substructure datasets.  If any SB01 is submitted; then error flag (i.e. there should be substructure elements); otherwise no error/flag
        public static bool IsValidBE01_9(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            // Check if a superstructure element is submitted
            if (Constants.superstructureElements.Contains(elRecord.BE01))
            {
                bool hasSubstructureElement = record.Elements.Any(e =>
                {
                    if (int.TryParse(e.BE01, out int value))
                    {
                        return value >= 200 && value < 300;
                    }
                    return false;
                });


                // If no substructure elements are submitted
                if (!hasSubstructureElement)
                {
                    // Check if any substructure dataset (SB01) is submitted
                    bool anySubstructureDataset = record.SubstructureSets.Any(sub => !string.IsNullOrEmpty(sub.BSB01));

                    if (anySubstructureDataset)
                    {
                        // If there is a substructure dataset but no substructure element, error/flag
                        return false;
                    }

                    // No substructure dataset, no error/flag
                    return true;
                }
            }

            // If not a superstructure element or substructure elements are present, no error
            return true;
        }


        /// <summary>
        /// For Deck Elements - Condition: If a Deck element is submitted, then a Superstructure element should generally also be submitted.
        /// Error: Missing superstructure element. Exception: Intent is to exclude slab superstructures with decks from this error check
        /// </summary>
        public static bool IsValidBE01_10(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            // Check if the element is a Deck element
            if (Constants.deckElements.Contains(elRecord.BE01))
            {
                //Slab elements (EN38, 54, and 65) should not be included in the Missing Superstructure Element check.  
                if (Constants.slabElements.Contains(elRecord.BE01))
                {
                    return true;
                }

                // Check for at least one Span dataset that meets the exception conditions
                bool hasExceptionSpan = record.SpanSets.Any(span =>
                    (span.BSP01.StartsWith("M") || span.BSP01.StartsWith("A")) &&
                    (slabSuperstructure.Contains(span.BSP06)) &&
                    span.BSP09 != "0");

                if (hasExceptionSpan)
                {
                    // Exception condition met; no error
                    return true;
                }

                // Check if there's at least one Superstructure element
                bool hasSuperstructureElement = record.Elements.Any(e => Constants.superstructureElements.Contains(e.BE01));

                if (hasSuperstructureElement)
                {
                    // Superstructure element is not present; no error
                    return true;
                }

                // Neither condition met; trigger error
                return false;
            }

            // Not a Deck element; no error
            return true;
        }


        //Element Parent Number
        public static bool IsValidBE02_1(object elRecord)
        {
            if (elRecord is SNBIRecord.Element elemRecord)
            {
                string epn = elemRecord.BE02?.ToString();
                string en = elemRecord.BE01?.ToString();

                if (string.IsNullOrEmpty(epn) || epn == "0")
                {
                    if (en == "510" || en == "515" || en == "521")
                    {
                        return false;
                    }
                }

                if (epn != null && epn.Length > 4)
                {
                    return false;
                }

                // Checking if epn is in a specific list based on the value of en
                switch (en)
                {
                    case "510":
                        if (!Constants.array510.Contains(epn))
                        {
                            return false;
                        }
                        break;
                    case "515":
                        if (!Constants.array515.Contains(epn))
                        {
                            return false;
                        }
                        break;
                    case "521":
                        if (!Constants.array521.Contains(epn))
                        {
                            return false;
                        }
                        break;
                }

                return true;
            }

            return true;
        }

        public static bool IsValidBE02_2(object elRecord, object snbiRecord)
        {
            if (elRecord is SNBIRecord.Element elemRecord && snbiRecord is SNBIRecord record)
            {
                string epn = elemRecord.BE02?.ToString();
                string en = elemRecord.BE01?.ToString();

                if (!string.IsNullOrEmpty(epn) && epn != "0")
                {
                    bool exists = record.Elements.Any(x => x.BE01?.ToString() == epn);
                    return exists; // Return true if an element with EN equal to EPN exists
                }
            }

            return true;
        }

        public static bool IsValidBE02_3(SNBIRecord.Element elRecord)
        {
            var ens = new HashSet<string> { "510", "515", "521" };

            if (ens.Contains(elRecord.BE01) && elRecord.BE02 == "0")
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBE02_4(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            if (elRecord.BE01?.Trim() == "510")
            {
                if (!Constants.culvertElements.Contains(elRecord.BE02))
                {
                    bool allSpansCondition = record.SpanSets.All(span =>
                        (span.BSP01.StartsWith("C") || span.BSP01.StartsWith("V")) && span.BSP05 == "7");

                    if (allSpansCondition)
                    {
                        return false; // Invalid state due to parent number and span conditions
                    }
                }
            }

            return true;
        }

        public static bool IsValidBE02_5(SNBIRecord.Element elRecord, SNBIRecord record)
        {
            if (elRecord.BE01?.Trim() == "515" && !Constants.nonSteelSubstructures.Contains(elRecord.BE02?.Trim()))
            {
                var steelCodes = new HashSet<string> { "S01", "S02", "S03", "S04", "S05", "S06" };
                var triggerValues = new HashSet<string> { "0", "C01", "C04", "C05" };

                bool allSubsInvalid = record.SubstructureSets.All(sub =>
                    !steelCodes.Contains(sub.BSB03) && triggerValues.Contains(sub.BSB05));

                return !allSubsInvalid;
            }

            return true;
        }

        public static bool IsValidTotalSum(object record)
        {
            if (record is Element elRecord)
            {
                if (!elRecord.BCS01.HasValue || !elRecord.BCS02.HasValue ||
                    !elRecord.BCS03.HasValue || !elRecord.BCS04.HasValue || !elRecord.BE03.HasValue)
                {
                    return true;  //Cannot validate
                }

                double Q1 = elRecord.BCS01.Value;
                double Q2 = elRecord.BCS02.Value;
                double Q3 = elRecord.BCS03.Value;
                double Q4 = elRecord.BCS04.Value;
                double Q = elRecord.BE03.Value;

                return Q1 + Q2 + Q3 + Q4 == Q;
            }
            else
            {
                return true;  //Cannot validate
            }
        }

        //BCS01, BCS02, BSC03, BSC04
        public static bool IsValidBCS(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;

            }

            if (!HasMaxLengthDouble(value, 8))
            {
                return true; //Already has been validated.
            }

            return IsPositiveIntegerOr0(value);
        }

        //BE03(Total Qty)
        public static bool IsValidBE03(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;

            }

            if (!HasMaxLengthDouble(value, 8))
            {
                return true; //Already has been validated.
            }

            return IsPositiveInteger(value); //Cannot be 0.
        }

        #endregion

        #region FEATURES

        //BF01 - FEATURE TYPE
        public static bool IsValidBF01(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            string str = value.ToString().Trim();

            // Regex to match a single letter followed by exactly two digits
            Regex regex = new Regex(@"^[A-Z][0-9]{2}$", RegexOptions.IgnoreCase);

            if (!regex.IsMatch(str))
            {
                return false;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BF01", str[0].ToString());
        }

        //BF01 Feature Type = "H01" (highway) must be carried on the bridge(BF02 = "C")
        public static bool CrossCheckBF01_BF02(SNBIRecord.Feature feature)
        {
            bool isHighwayFeature = feature.BF01 == "H01";
            bool isCarriedOnBridge = feature.BF02 == "C";

            if (isHighwayFeature && !isCarriedOnBridge)
            {
                return false;
            }

            if (!isHighwayFeature && isCarriedOnBridge)
            {
                return false;
            }

            return true;
        }

        public static bool IsValidBF02(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            string str = value.ToString().Trim();

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BF02", str);
        }

        public static bool IsValidBF03(object value, int maxLength)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            if (!HasMaxLengthString(value, maxLength))
            {
                return false;
            }
            return true;

        }

        //BH01 - FUNCTIONAL CLASSIFICATION
        //Must be reported for all highway Features associated with a bridge except when that Feature is carried on another Bridge in the inventory - CRITICAL
        public static bool IsValidBH01(Feature feRecord)
        {
            if (!IsNullOrEmpty(feRecord.BF01) && feRecord.BF01.StartsWith("H") && string.IsNullOrWhiteSpace(feRecord.BH18))
            {
                if (IsNullOrEmpty(feRecord.BH01))
                {
                    return false;
                }
            }

            if (!IsNullOrEmpty(feRecord.BH01))
            {
                string str = feRecord.BH01.ToString().Trim();

                if (Constants.TemporaryCodes.Contains(str))
                {
                    TemporaryCounts.BH01Count++;
                }

                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BH01", str);
            }
            return true;
        }

        //BH18 crossing Bridge Number indicates this highway feature is carried on another bridge - do not report this Item for this Features dataset
        public static bool DoNotReportBH(Feature feRecord, object value)
        {
            if (IsNullOrEmpty(feRecord.BF01))
            {
                return true;
            }
            if (feRecord.BF01.StartsWith("H"))
            {
                if (!string.IsNullOrWhiteSpace(feRecord.BH18))
                {
                    return IsNullOrEmpty(value);
                }
            }
            return true;
        }


        //BH02 - URBAN CODE must be reported for all HIGHWAY ("H") Features
        public static bool IsValidBH02(SNBIRecord.Feature feRecord)
        {
            if (IsNullOrEmpty(feRecord.BF01))
            {
                return true;
            }

            string strBH02 = feRecord.BH02 ?? "";

            if (feRecord.BF01.StartsWith("H"))
            {

                if (string.IsNullOrWhiteSpace(feRecord.BH18) && string.IsNullOrEmpty(strBH02))
                {
                    return false;
                }

                // Check for predefined valid codes
                if (strBH02 == Constants.TempCodeBH02 || strBH02 == "99999" || strBH02 == "99998")
                {
                    return true;
                }

                // Ensure BH02 has exactly 5 numeric digits
                if (!System.Text.RegularExpressions.Regex.IsMatch(strBH02, @"^\d{5}$"))
                {
                    return false;
                }

                //if (int.TryParse(strBH02, out int intBH02))
                //{
                //    // Use singleton to check urban codes
                //    if (UrbanCodeService.Instance.TryGetUrbanCodes((int)feRecord.BL01, out HashSet<int> urbanCodes) && urbanCodes.Contains(intBH02))
                //    {
                //        return true;
                //    }
                //}

            }

            return true;
        }


        //BH03 - NHS DESIGNATION must be reported for all HIGHWAY ("H") Features
        public static bool IsValidBH03(Feature feRecord)
        {
            if (IsNullOrEmpty(feRecord.BF01))
            {
                return true;
            }

            string strBH03 = feRecord.BH03 ?? "";

            if (feRecord.BF01.StartsWith("H"))
            {
                if (string.IsNullOrWhiteSpace(feRecord.BH18))
                {
                    if (IsNullOrEmpty(strBH03))
                    {
                        return false;
                    }
                }
                if (strBH03 == "Y" || strBH03 == "N")
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        public static bool IsValidBH04(Feature feRecord)
        {
            if (IsNullOrEmpty(feRecord.BF01))
            {
                return true;
            }

            string str = feRecord.BH04 ?? "";

            if (feRecord.BF01.StartsWith("H"))
            {
                if (string.IsNullOrWhiteSpace(feRecord.BH18))
                {
                    if (IsNullOrEmpty(str))
                    {
                        return false;
                    }
                }

                if (Constants.TemporaryCodes.Contains(str))
                {
                    TemporaryCounts.BH04Count++;
                }

                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BH04", str);
            }
            return true;
        }

        public static bool IsValidBH05(Feature feRecord)
        {
            if (IsNullOrEmpty(feRecord.BF01))
            {
                return true;
            }

            string str = feRecord.BH05 ?? "";

            if (feRecord.BF01.StartsWith("H"))
            {

                if (string.IsNullOrWhiteSpace(feRecord.BH18))
                {
                    if (IsNullOrEmpty(feRecord.BH05))
                    {
                        return false;
                    }
                }

                if (str == "1" || str == "2" || str == "N")
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public static bool IsValidBH06(Feature feRecord, int maxLength)
        {
            if (IsNullOrEmpty(feRecord.BF01))
            {
                return true;
            }

            if (feRecord.BF01.StartsWith("H") && string.IsNullOrWhiteSpace(feRecord.BH18))
            {
                if (IsNullOrEmpty(feRecord.BH06))
                {
                    return false;
                }
            }

            if (!HasMaxLengthString(feRecord.BH06, maxLength))
            {
                return false;
            }

            return true;
        }

        //B.H.07 - LRS Mile Point - N (8,3)
        //LRS Mile Point is reported for all highway Features datasets associated with a bridge.
        public static bool IsValidBH07(Feature feRecord, int decimalPlaces)
        {
            if (!HasMaxLengthDouble(feRecord.BH07, 8))
                return true; // Skip validation

            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (feRecord.BH06 == "N") // LRS Route ID has not been assigned.
                return true;

            if (IsNullOrEmpty(feRecord.BH07))
                return false;

            // Check if the value is positive or 0
            if (feRecord.BH07 < 0)
                return false;

            // Check if the number of decimal places matches the specified amount
            //int actualDecimalPlaces = GetDecimalPlaces(feRecord.BH07.ToString());

            //if (actualDecimalPlaces != decimalPlaces && actualDecimalPlaces != 0)
            //    return false;

            return true;
        }


        //B.H.08 - LANES ON HIGHWAY - N(2,0)
        public static bool IsValidBH08(Feature feRecord)
        {
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (string.IsNullOrWhiteSpace(feRecord.BH18) && feRecord.BH08 == 0)
                return false;

            if (feRecord.BH08 < 0)
                return false;

            if (feRecord.BH08 > 99) // more than 2 digits
                return false;

            return true;
        }



        //B.H.09 - Annual Average Daily Traffic - N(8,0)
        public static bool IsValidBH09(Feature feRecord)
        {
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (string.IsNullOrWhiteSpace(feRecord.BH18) && feRecord.BH09 == 0) //BH18 - Crossing Bridge Number
                return false;

            if (feRecord.BH09 < 0) // Check if BH09 is positive
                return false;

            if (feRecord.BH09 > 99999999) // more than 8 digits
                return false;

            return true;
        }


        //B.H.10 - Annual Average Daily Truck Traffic - N(8,0)
        public static bool IsValidBH10(Feature feRecord)
        {
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (feRecord.BH10 == 0) // "0" is a valid value as per Wendy's note
                return true;

            if (feRecord.BH10 < 0) // Check for valid positive integer or 0
                return false;

            if (feRecord.BH10 > 99999999) // More than 8 digits
                return false;

            return true;
        }


        //B.H.11 - Year of Annual Average Daily Traffic - N(4,0)
        public static bool IsValidBH11(Feature feRecord)
        {
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (string.IsNullOrWhiteSpace(feRecord.BH18) && !feRecord.BH11.HasValue)
                return false;

            // Validate that BH11 is within a realistic year range
            if (feRecord.BH11.HasValue)
            {
                int year = feRecord.BH11.Value;
                return year >= 1800 && year <= DateTime.Now.Year;
            }

            return true;
        }


        //B.H.12 - Highway Maximum Usable Vertical Clearance - N(3,1)
        public static bool IsValidBH12(Feature feRecord)
        {
            // Check if the conditions that require BH12 to be validated are met
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (string.IsNullOrWhiteSpace(feRecord.BH18) &&
                (feRecord.BF02 != "B" || (feRecord.BF02 == "B" && feRecord.BH03 == "Y")))
            {
                // Ensure BH12 is not null/empty, is numeric, has 1 decimal place, and <= 99.9
                if (IsNullOrEmpty(feRecord.BH12))
                    return false;

                // Check if the value is positive
                if (feRecord.BH12 < 0 || feRecord.BH12 > 99.9)
                    return false;

                // Check if the number of decimal places matches the specified amount
                int actualDecimalPlaces = GetDecimalPlaces(feRecord.BH12.ToString());

                if (actualDecimalPlaces > 1)
                    return false;

            }

            return true;
        }

        //B.H.13 - Highway Minimum Vertical Clearance
        public static bool IsValidBH13(Feature feRecord)
        {
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (string.IsNullOrWhiteSpace(feRecord.BH18))
            {
                // Ensure BH13 is not null/empty, is numeric, has 1 decimal place, and <= 99.9
                if (IsNullOrEmpty(feRecord.BH13))
                    return false;

                // Check if the value is positive
                if (feRecord.BH13 < 0 || feRecord.BH13 > 99.9)
                    return false;

                int actualDecimalPlaces = GetDecimalPlaces(feRecord.BH13.ToString());

                if (actualDecimalPlaces > 1)
                    return false;

            }

            return true;
        }


        //B.H.14 - Highway Minimum Horizontal Clearance, Left
        //(BF01 = H##) AND (BF02 = "B") AND (BH18 = null) AND (BH14 = null)
        public static bool IsValidBH14(Feature feRecord)
        {
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (feRecord.BF02 == "B" && string.IsNullOrWhiteSpace(feRecord.BH18))
            {
                if (IsNullOrEmpty(feRecord.BH14))
                    return false;

                if (feRecord.BH14 < 0 || feRecord.BH14 > 99.9)
                    return false;

                int actualDecimalPlaces = GetDecimalPlaces(feRecord.BH14.ToString());

                if (actualDecimalPlaces > 1)
                    return false;

            }

            return true;
        }


        //B.H.15 - Highway Minimum Horizontal Clearance, Right
        public static bool IsValidBH15(Feature feRecord)
        {
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (feRecord.BF02 == "B" && string.IsNullOrWhiteSpace(feRecord.BH18))
            {
                if (IsNullOrEmpty(feRecord.BH15))
                    return false;

                if (feRecord.BH15 < 0 || feRecord.BH15 > 99.9)
                    return false;

                int actualDecimalPlaces = GetDecimalPlaces(feRecord.BH15.ToString());

                if (actualDecimalPlaces > 1)
                    return false;

            }

            return true;
        }

        //B.H.16 - Highway Maximum Usable Surface Width
        public static bool IsValidBH16(Feature feRecord)
        {
            // Check if the conditions that require BH12 to be validated are met
            if (string.IsNullOrEmpty(feRecord.BF01) || !feRecord.BF01.StartsWith("H"))
                return true;

            if (string.IsNullOrWhiteSpace(feRecord.BH18) &&
                (feRecord.BF02 != "B" || (feRecord.BF02 == "B" && feRecord.BH03 == "Y")))
            {
                // Ensure BH12 is not null/empty, is numeric, has 1 decimal place, and <= 99.9
                if (IsNullOrEmpty(feRecord.BH16))
                    return false;

                // Check if the value is positive
                if (feRecord.BH16 < 0 || feRecord.BH16 > 99.9)
                    return false;

                // Check if the number of decimal places matches the specified amount
                //int actualDecimalPlaces = GetDecimalPlaces(feRecord.BH16.ToString());

                //if (actualDecimalPlaces > 1)
                //    return false;

            }

            return true;
        }

        //B.H.17 - Bypass Detour Length; N(3,0)
        public static bool IsValidBH17(Feature feRecord)
        {
            if (feRecord?.BF01 == null || !feRecord.BF01.StartsWith("H"))
                return true;

            // Check if BH18 is null or whitespace and requires BH17 validation
            if (string.IsNullOrWhiteSpace(feRecord.BH18))
            {
                // If BH17 is null or outside the range 0–999, return false
                if (feRecord.BH17 == null || feRecord.BH17 < 0 || feRecord.BH17 > 999)
                    return false;

                // Validate that BH17 has no decimal places
                if (feRecord.BH17 % 1 != 0)
                    return false;
            }

            return true;
        }


        //B.H.18 Crossing Bridge Number
        public static bool IsValidBH18(string value, string pattern)
        {
            if (!HasMaxLengthString(value, 15))
            {
                return true; //Already validated.
            }

            return IsValidWithRegex(value, pattern);

        }

        //FEATURES - RAILROADS
        //BRR01 - Railroad Service Type
        public static bool IsValidBRR01(SNBIRecord.Feature feature)
        {
            if (feature.BF01 != null && feature.BF01.StartsWith("R"))
            {
                if (IsNullOrEmpty(feature.BRR01))
                {
                    return false;
                }

                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BRR01", feature.BRR01);
            }

            return true;
        }

        //BRR02 - Railroad Minimum Vertical Clearance N(3,1)
        public static bool IsValidBRR02(SNBIRecord.Feature feature, int decimalPlaces)
        {
            if (feature.BF02 != "B")
            {
                return true;
            }

            if (feature.BF01 != null && feature.BF01.StartsWith("R"))
            {
                if (feature.BF02 == "B")
                {
                    if (IsNullOrEmpty(feature.BRR02))
                    {
                        return false;
                    }
                }

                if (!IsNullOrEmpty(feature.BRR02))
                {
                    // Check if the value is positive or 0
                    if (feature.BRR02 < 0)
                        return false;

                    // Check if the number of decimal places matches the specified amount
                    int actualDecimalPlaces = GetDecimalPlaces(feature.BRR02.ToString());

                    if (actualDecimalPlaces != 1 && actualDecimalPlaces != 0)
                        return false;

                }

            }

            return true;
        }

        //BRR03 - Railroad Minimum Horizontal Offset N(3,1)
        public static bool IsValidBRR03(SNBIRecord.Feature feature, int decimalPlaces)
        {
            if (feature.BF02 != "B")
            {
                return true;
            }

            if (feature.BF01 != null && feature.BF01.StartsWith("R"))
            {
                if (feature.BF02 == "B")
                {
                    if (IsNullOrEmpty(feature.BRR03))
                    {
                        return false;
                    }
                }
                if (!IsNullOrEmpty(feature.BRR03))
                {
                    // Check if the value is positive or 0
                    if (feature.BRR03 < 0)
                        return false;

                    // Check if the number of decimal places matches the specified amount
                    int actualDecimalPlaces = GetDecimalPlaces(feature.BRR03.ToString());

                    if (actualDecimalPlaces != 1 && actualDecimalPlaces != 0)
                        return false;
                }
            }

            return true;
        }

        public static bool IsValidBN01(SNBIRecord.Feature feature)
        {
            const string waterwayFeature = "W";

            if (feature.BF01 != null && feature.BF01.StartsWith(waterwayFeature))
            {
                if (IsNullOrEmpty(feature.BN01))
                {
                    return false;
                }
            }

            if (!IsNullOrEmpty(feature.BN01))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BN01", feature.BN01);
            }

            return true;
        }

        //B.N.02 - Navigation Minimum Vertical Clearance N(4.1)
        public static bool IsValidBN02(SNBIRecord.Feature feature)
        {
            const string waterwayFeature = "W";
            const double maxValue = 999.9;

            // For waterway features (BF01 starts with "W") where BN01 is "Y",
            // BN02 must be provided.
            if (!string.IsNullOrEmpty(feature.BF01) &&
                feature.BF01.StartsWith(waterwayFeature) &&
                feature.BN01 == "Y")
            {
                if (feature.BN02 == null)
                {
                    return false;
                }
            }

            // If BN02 is provided, it must be a positive number and within maxValue.
            if (feature.BN02 != null)
            {
                double value = feature.BN02.Value;
                if (value < 0)
                {
                    return false;
                }
            }

            return true;
        }
        //B.N.03 - Movable Bridge Maximum Navigation Vertical Clearance N(4.1)
        public static bool IsValidBN03(SNBIRecord.Feature feature, SNBIRecord snbiRecord)
        {
            const string waterwayFeature = "W";
            const string movableSpanPrefix = "M";

            // For waterway features (BF01 starts with "W") with BN01 = "Y",
            // if it's a movable span then BN03 must be provided.
            if (!string.IsNullOrEmpty(feature.BF01) &&
                feature.BF01.StartsWith(waterwayFeature) &&
                feature.BN01 == "Y")
            {
                bool isMovableSpan = snbiRecord.SpanSets.Any(x => x.BSP06.StartsWith(movableSpanPrefix));

                if (isMovableSpan && !feature.BN03.HasValue)
                {
                    return false;
                }
            }

            // If BN03 is provided, ensure it is non-negative.
            if (feature.BN03 != null)
            {
                double value = feature.BN03.Value;
                if (value < 0)
                {
                    return false;
                }
            }

            return true;
        }

        //B.N.04 - Navigation Channel Width N(5.1)
        public static bool IsValidBN04(SNBIRecord.Feature feature)
        {
            const string waterwayFeature = "W";

            if (feature.BF01 != null && feature.BF01.StartsWith(waterwayFeature) && feature.BN01 == "Y")
            {
                if (!feature.BN04.HasValue)
                {
                    return false;
                }
            }

            // If BN04 is provided, ensure it is non-negative.
            if (feature.BN04 != null)
            {
                double value = feature.BN04.Value;
                if (value < 0)
                {
                    return false;
                }
            }

            return true;

        }

        //B.N.05 - Navigation Channel Minimum Horizontal Clearance N(5.1)
        public static bool IsValidBN05(SNBIRecord.Feature feature)
        {
            const string waterwayFeature = "W";

            if (feature.BF01 != null && feature.BF01.StartsWith(waterwayFeature) && feature.BN01 == "Y")
            {
                if (!feature.BN04.HasValue)
                {
                    return false;
                }
            }

            // If BN05 is provided, ensure it is non-negative.
            if (feature.BN05 != null)
            {
                double value = feature.BN05.Value;
                if (value < 0)
                {
                    return false;
                }
            }

            return true;

        }

        public static bool IsValidBN06(SNBIRecord.Feature feature)
        {
            const string waterwayFeature = "W";

            if (feature.BF01 != null && feature.BF01.StartsWith(waterwayFeature) && feature.BN01 == "Y")
            {
                if (IsNullOrEmpty(feature.BN06))
                {
                    return false;
                }
            }

            if (!IsNullOrEmpty(feature.BN06))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BN06", feature.BN06);
            }

            return true;
        }

        #endregion

        #region FEATURE ROUTES

        //BRT01 - Route Designation
        public static bool IsValidBRT01(SNBIRecord.Route route)
        {
            if (route.BF01 != null && route.BF01.StartsWith("H"))
            {
                if (string.IsNullOrEmpty(route.BRT01))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(route.BRT01))
            {
                string str = route.BRT01.Trim();

                // Regex to match a single letter "R" followed by exactly two digits
                Regex regex = new Regex(@"^R[0-9]{2}$", RegexOptions.IgnoreCase);

                if (!regex.IsMatch(str))
                {
                    return false;
                }
            }

            return true;
        }

        //BRT02 - Route Number
        public static bool IsValidBRT02(SNBIRecord.Route route)
        {
            if (route.BF01 != null && route.BF01.StartsWith("H"))
            {
                if (string.IsNullOrEmpty(route.BRT02))
                {
                    return false;
                }
            }

            return true;
        }

        //BRT03 - Route Direction
        public static bool IsValidBRT03(SNBIRecord.Route route)
        {
            if (route.BF01 != null && route.BF01.StartsWith("H"))
            {
                if (string.IsNullOrEmpty(route.BRT03))
                {
                    return false;
                }
            }

            if (Constants.TemporaryCodes.Contains(route.BRT03))
            {
                TemporaryCounts.BRT03Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BRT03", route.BRT03);

        }

        //BRT04 - Route Type
        public static bool IsValidBRT04(SNBIRecord.Route route)
        {
            if (route.BF01 != null && route.BF01.StartsWith("H"))
            {
                if (IsNullOrEmpty(route.BRT04))
                {
                    return false;
                }
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BRT04", route.BRT04);
        }

        //BRT05 - Service Type
        public static bool IsValidBRT05(SNBIRecord.Route route)
        {
            if (route.BF01 != null && route.BF01.StartsWith("H"))
            {
                if (IsNullOrEmpty(route.BRT05))
                {
                    return false;
                }
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BRT05", route.BRT05);
        }





        #endregion

        public static bool IsNullOrEmpty(object value)
        {
            if (value == null)
                return true;

            if (value is string str)
                return string.IsNullOrWhiteSpace(str);

            if (value is double d)
                return double.IsNaN(d);

            return false;
        }

        public static bool IsNotNull(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }
            return true;
        }


        public static bool IsValidWithRegex(string input, string pattern)
        {
            if (IsNullOrEmpty(input))
            {
                return true;
            }

            var regex = new System.Text.RegularExpressions.Regex(pattern);
            return regex.IsMatch(input.Trim());
        }

        public static bool IsYOrN(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            string strValue = value.ToString().Trim();
            return strValue == "Y" || strValue == "N";
        }

        public static bool IsNotNullOrWhiteSpace(object value)
        {
            string str = value as string;
            return !string.IsNullOrWhiteSpace(str);
        }

        public static bool IsPositiveInteger(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            if (value is string strValue)
            {
                if (int.TryParse(strValue, out int x) && x > 0)
                {
                    return true;
                }
            }
            else
            {
                if (int.TryParse(value.ToString(), out int x) && x > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPositiveIntegerOr0(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            if (value is string strValue)
            {
                if (Regex.IsMatch(strValue, @"^\d+$"))
                {
                    return true;
                }

                //if (int.TryParse(strValue, out int x) && x >= 0)
                //{
                //    return true;
                //}
            }
            else
            {
                // Handle non-string objects that might still be numbers.
                // It handles cases where the object might be an actual numeric type.
                if (int.TryParse(value.ToString(), out int x) && x >= 0)
                {
                    return true;
                }

                if (decimal.TryParse(value.ToString(), out decimal d) && d >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidDouble(object value)
        {
            if (value == null)
            {
                return false;
            }

            return double.TryParse(value.ToString().Trim(), out double o);
        }

        public static bool IsPositiveNumericAndMatchDecimal(object value, int decimalPlaces)
        {
            if (value == null)
            {
                return true;
            }

            // Try to parse the value as a double
            if (!double.TryParse(value.ToString().Trim(), out double number))
            {
                return false; // Not a numeric value
            }

            // Check if the number is positive
            if (number < 0)
            {
                return false;
            }

            // Convert the number to a string with invariant culture to ensure consistent decimal formatting
            string[] parts = number.ToString(System.Globalization.CultureInfo.InvariantCulture).Split('.');

            if (decimalPlaces == 0)
            {
                return parts.Length == 1;
            }

            return parts.Length == 2 && parts[1].Length <= decimalPlaces;
        }


        public static bool IsDifferent(object value1, object value2)
        {
            if (IsNullOrEmpty(value1) || IsNullOrEmpty(value2))
            {
                return true;
            }

            var value1Str = value1.ToString().Trim();
            var value2Str = value2.ToString().Trim();

            // Return true if value1 and value2 are not the same
            return !value1Str.Equals(value2Str);
        }

        public static bool IsValidYYYYMMDD(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                return true;
            }

            if (date != null && date.Trim().Length == 8 && date.All(char.IsDigit))
            {
                return DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
            }
            return false;
        }

        public static bool IsValidLengthDouble(object value, int maxLength, int predecimal)
        {
            if (!IsValidIntegerPart(value, predecimal))
            {
                return true; //Do not check total length if the number of digits before the decimal point exceeds the allowed limit, to avoid displaying multiple errors for the same item.
            }

            return HasMaxLengthDouble(value, maxLength);
        }

        public static bool HasMaxLengthDouble(object value, int maxLength)
        {
            if (value == null)
            {
                return true;
            }
            return value.ToString().Trim().Replace(".", "").Length <= maxLength;
        }

        public static bool HasMaxLengthString(object value, int maxLength)
        {
            if (IsNullOrEmpty(value))
            {
                return true;
            }

            string stringValue = value.ToString().Trim();
            return stringValue.Length <= maxLength;
        }

        public static bool HasMaxLengthStringTruncate(object value, int maxLength)
        {
            if (IsNullOrEmpty(value))
            {
                return true;
            }

            string stringValue = value.ToString().Trim();

            if (stringValue.Length > maxLength)
            {
                value = stringValue.Substring(0, maxLength); // Truncate if max length
                return false;
            }

            return true;
        }

        public static bool IsWholeNumber(object value)
        {
            if (value == null)
            {
                return true;
            }
            string stringValue = value.ToString().Trim();

            if (decimal.TryParse(stringValue, out _) && stringValue.Contains("."))
            {
                value = 0; //if decimal set to zero for output
                return false;
            }

            return true;
        }

        public static bool IsWholeNumberRoundDown(object value)
        {
            if (value == null)
            {
                return true;
            }
            string stringValue = value.ToString().Trim();

            if (decimal.TryParse(stringValue, out decimal numericValue) && stringValue.Contains("."))
            {
                value = Math.Floor(numericValue); //if decimal round down for output
                return false;
            }

            return true;
        }

        public static bool IsValidIntegerPart(object value, int predecimal)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }

            string stringValue = value.ToString().Trim();

            if (decimal.TryParse(stringValue, out decimal numericValue))
            {
                string preDecimalPart = stringValue.Split('.')[0];
                return preDecimalPart.Length <= predecimal;
            }

            return true;
        }


        public static bool HasMoreThanOneDecimalPlace(object value)
        {
            decimal roundedValue = 0;

            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }

            string stringValue = value.ToString().Trim();

            if (decimal.TryParse(stringValue, out decimal numericValue))
            {
                string[] parts = stringValue.Split('.');

                if (parts.Length > 1 && parts[1].Length > 1)
                {
                    roundedValue = Math.Floor(numericValue * 10) / 10; // Round down to one decimal place
                    return false;
                }

                roundedValue = numericValue; //out value
                return true;
            }

            return true;
        }


        ///////************ 5 - POSTING STATUSES ***************///////

        public static bool IsValidBPS01(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            string stringVal = value.ToString().Trim();

            if (Constants.TemporaryCodes.Contains(stringVal))
            {
                TemporaryCounts.BPS01Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BPS01", value.ToString());
        }

        public static bool IsValidBPS02(object value)
        {
            if (IsNullOrEmpty(value))
            {
                return false;
            }

            string strBPS02 = value.ToString().Trim();

            // Check if BPS02 is a valid date in YYYYMMDD format
            return IsValidYYYYMMDD(strBPS02);
        }


        ///////************ 7 - INSPECTIONS ***************///////

        public static bool IsValidBIE01(object BIE01)
        {
            string strBIE01 = BIE01?.ToString().Trim() ?? string.Empty;

            if (!IsNullOrEmpty(strBIE01))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BIE01", strBIE01);
            }

            return true;
        }

        public static bool IsValidBIE05(object value)
        {
            string? strBIE05 = value?.ToString();

            if (string.IsNullOrWhiteSpace(strBIE05))
            {
                return false;
            }

            if (!IsPositiveNumericAndMatchDecimal(strBIE05, 0))
                return false;

            // Try parsing the string to a double and validate its maximum allowed value
            if (double.TryParse(strBIE05, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
            {
                return number <= 99;
            }

            return false;
        }

        public static bool CrossCheckBIE01_BIE04(SNBIRecord.Inspection inspection)
        {
            string strBIE01 = inspection.BIE01?.ToString().Trim() ?? string.Empty;
            string strBIE04 = inspection.BIE04?.ToString().Trim() ?? string.Empty;
            int? bie05 = inspection.BIE05;

            // FOR (BIE01 = 7) AND (BIE04 = NULL) AND (BIE05 <> 0); TRUE = ERROR; FALSE = NO ERROR;
            //if (strBIE01 == "7" && string.IsNullOrEmpty(strBIE04) && bie05.HasValue && bie05 != 0)
            //{
            //    return false;
            //} this rule removed until further instructions

            // Handle other valid BIE01 values
            var validBIE01Values = new HashSet<string> { "1", "2", "3", "4", "6" };

            bool hasValidBIE01 = validBIE01Values.Contains(strBIE01);

            if (hasValidBIE01 && string.IsNullOrEmpty(strBIE04))
            {
                return false;
            }

            return true;
        }


        public static bool CrossCheckBIE05_BIE01(object BIE05, object BIE01)
        {
            string strBIE01 = BIE01?.ToString().Trim() ?? string.Empty;
            var validBIE01Values = new HashSet<string> { "5", "9" };

            if (validBIE01Values.Contains(strBIE01))
            {
                if (int.TryParse(BIE05?.ToString(), out int bie05Value))
                {
                    if (bie05Value != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsValidBIE07(object BIE07)
        {
            string strBIE07 = BIE07?.ToString() ?? string.Empty;

            if (!IsNullOrEmpty(strBIE07))
            {
                var validator = validatorFactory.Create("LookupValues");
                return validator.IsValidCode("BIE07", strBIE07);
            }

            return true;
        }

        public static bool CrossCheckBIE07NotN(object BIE07, object BIE01)
        {
            string strBIE01 = BIE01?.ToString() ?? string.Empty;
            string strBIE07 = BIE07?.ToString() ?? string.Empty;
            var validBIE01Values = new HashSet<string> { "1", "5", "6", "7", "8", "9" };

            if (validBIE01Values.Contains(strBIE01))
            {
                if (strBIE07 != "N")
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CrossCheckBIE07_BIE01(object BIE07, object BIE01)
        {
            string strBIE01 = BIE01?.ToString() ?? string.Empty;
            string strBIE07 = BIE07?.ToString() ?? string.Empty;

            var validBIE01Values = new HashSet<string> { "2", "3", "4" };
            var validBIE07Values = new HashSet<string> { "1", "2" };

            if (validBIE01Values.Contains(strBIE01))
            {
                if (!validBIE07Values.Contains(strBIE07))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsNotNullBIE11(object BIE11, object BIE01)
        {
            string strBIE01 = BIE01?.ToString() ?? string.Empty;
            string strBIE11 = BIE11?.ToString() ?? string.Empty;

            if (strBIE01.Length == 1 && char.IsDigit(strBIE01[0]) && strBIE01[0] >= '3' && strBIE01[0] <= '9')
            {
                return !string.IsNullOrEmpty(strBIE11);
            }

            return true;
        }

        public static bool IsValidBIE11(object value, string pattern)
        {
            string? strValue = value?.ToString();

            if (!HasMaxLengthString(strValue, 15))
            {
                return true; //Already validated.
            }

            return IsValidWithRegex(strValue, pattern);
        }

        public static bool IsValidBIE12(object value)
        {
            //Per Wendy's comment - SNBI states "Do not Report" when non of the equipment in the valid values list was used therefore null values are valid values.
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return true;
            }

            if (!HasMaxLengthString(value, 120))
            {
                return true;  //Separate rule
            }

            string stringVal = value.ToString();
            var codes = stringVal.Split('|');
            var validator = validatorFactory.Create("LookupValues");

            foreach (var code in codes)
            {
                string trimmedCode = code.Trim();

                if (Constants.TemporaryCodes.Contains(trimmedCode))
                {
                    TemporaryCounts.BIE12Count++;
                }

                if (!validator.IsValidCode("BIE12", trimmedCode))
                {
                    return false;
                }
            }

            return true;
        }



        ///******************* Substructure Material and Type *******************////

        public static bool IsValidBSB01(object value)
        {
            string strValue = value?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(strValue))
            {
                return false;
            }

            //Match the format A##, P##, or W##.
            var regex = new Regex(@"^[APW]\d{2}$");

            return regex.IsMatch(strValue);
        }



        // Should be a positive integer greater than 0 and no greater than 999
        public static bool IsValidBSB02(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true; // Skip validation
            }

            string strValue = value.ToString().Trim();

            // Attempt to parse the value as an integer with strict number styles
            if (!int.TryParse(strValue, NumberStyles.None, CultureInfo.InvariantCulture, out int numericValue))
                return false;

            // Check if the value is positive and within the specified range
            if (numericValue <= 0 || numericValue > 999)
                return false;

            return true;
        }

        public static bool IsValidBSB03(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return false;
            }

            string stringVal = value.ToString();

            if (Constants.TemporaryCodes.Contains(stringVal))
            {
                TemporaryCounts.BSB03Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSB03", value.ToString());
        }

        public static bool IsValidBSB04(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return false;
            }

            string stringVal = value.ToString();

            if (Constants.TemporaryCodes.Contains(stringVal))
            {
                TemporaryCounts.BSB04Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSB04", value.ToString());
        }

        //(BSB04 <> 0) AND (BSB05 = null)
        public static bool IsValidBSB05(object BSB04, object BSB05)
        {
            // If BSB04 is not "0" and BSB05 is null or whitespace, return false
            if (BSB04?.ToString() != "0" && (BSB05 == null || string.IsNullOrWhiteSpace(BSB05.ToString())))
            {
                return false;
            }

            if (BSB05 is string str && Constants.TemporaryCodes.Contains(str))
            {
                TemporaryCounts.BSB05Count++;
            }

            // Validate BSB05 using the validator
            var validator = validatorFactory.Create("LookupValues");
            return BSB05 != null && validator.IsValidCode("BSB05", BSB05.ToString());
        }

        //(BSB04 = 0) AND (BSB05 <> null)
        public static bool DoNotReportBSB05(object BSB04, object BSB05)
        {
            string strBSB04 = BSB04?.ToString();
            string strBSB05 = BSB05?.ToString();

            // Return false if BSB04 equals "0" and BSB05 is not null or empty; otherwise, return true
            return !(strBSB04 == "0" && !string.IsNullOrEmpty(strBSB05));
        }


        public static bool IsValidBSB06(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return false;
            }

            string stringVal = value.ToString();

            if (Constants.TemporaryCodes.Contains(stringVal))
            {
                TemporaryCounts.BSB06Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSB06", value.ToString());
        }

        public static bool IsValidBSB07(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return false;
            }

            string stringVal = value.ToString();

            if (Constants.TemporaryCodes.Contains(stringVal))
            {
                TemporaryCounts.BSB07Count++;
            }

            var validator = validatorFactory.Create("LookupValues");
            return validator.IsValidCode("BSB07", value.ToString());
        }



        ///////************ SPAN SETS ***************///////

        //BSP01 - Main Span dataset missing

        public static bool CheckSpanSetsForMain(object bsp01, object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                var patternAorW = new Regex(@"^[AW]\d{2}$");
                var patternM = new Regex(@"^M\d{2}$");
                string strBSP01 = bsp01?.ToString() ?? string.Empty;

                if (patternAorW.IsMatch(strBSP01))
                {
                    bool hasM = snbiRecord.SpanSets.Any(spanSet =>
                        spanSet != null &&
                        spanSet.BSP01 is string spanSetBSP01 &&
                        patternM.IsMatch(spanSetBSP01)
                    );

                    if (!hasM) { return false; }
                }

                return true;
            }

            return true;
        }

        //BSP01 - Check if Valid Culvert
        public static bool ValidateCulvertSpanConfig(object record)
        {
            if (record is not SNBIRecord snbiRecord)
                return true;

            // Find SpanSets where BSP01 matches the culvert span regex and BSP05 is "7"
            bool hasCulvertSpan = snbiRecord.SpanSets?.Any(span =>
                !string.IsNullOrEmpty(span.BSP01) &&
                Regex.IsMatch(span.BSP01, Constants.CulvertSpanRegex) &&
                span.BSP05 == "7") ?? false;

            if (hasCulvertSpan)
            {
                // Check for at least one Feature where BF01 equals "W01"
                bool hasWaterwayFeature = snbiRecord.Features?.Any(feature => feature.BF01 == "W01") ?? false;

                if (!hasWaterwayFeature)
                {
                    // Validation fails: Culvert spans require at least one waterway feature
                    return false;
                }
            }

            return true;
        }

        // Should be a positive integer between 1 and 9999
        public static bool IsValidBSP02(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            string strValue = value.ToString().Trim();

            if (!int.TryParse(strValue, NumberStyles.None, CultureInfo.InvariantCulture, out int numericValue))
                return false;

            // Check if the value is positive and within the specified range
            if (numericValue < 1 || numericValue > 9999)
                return false;

            return true;
        }

        // Should be a positive integer between 1 and 999
        public static bool IsValidBSP03(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            string strValue = value.ToString().Trim();

            if (!int.TryParse(strValue, NumberStyles.None, CultureInfo.InvariantCulture, out int numericValue))
                return false;

            // Check if the value is positive and within the specified range
            if (numericValue < 1 || numericValue > 999)
                return false;

            return true;
        }

        public static bool CheckSumOfBSP02(SNBIRecord record)
        {
            if (record.SpanSets == null) return true;

            int sumBSP02 = record.SpanSets.Sum(spanSet => Convert.ToInt32(spanSet.BSP02));
            return sumBSP02 != 0;
        }

        /// <summary>
        /// Validates the sum of BSP03 values within an SNBIRecord based on specific conditions.
        /// 
        /// Requirements:
        /// 1. **Skip Validation**:
        ///     - If any `BSP03` value is not a valid positive integer between `0` and `999` inclusive,
        ///       the method returns `true` immediately, skipping further validation.
        /// 
        /// 2. **Validation Fails**:
        ///     - If all `BSP03` values are valid positive integers between `0` and `999`, and:
        ///         - At least one `BSP06` value is not "P01" or "P02".
        ///         - The sum of all valid `BSP03` values is `0`.
        ///       Then the validation fails, and the method returns `false`.
        /// 
        /// 3. **Validation Passes**:
        ///     - In all other cases, the validation passes, and the method returns `true`.
        /// </summary>
        /// <param name="record">An SNBIRecord containing a collection of SpanSets to validate.</param>
        /// <returns>
        /// Returns `true` if validation passes or is skipped based on the conditions.
        /// Returns `false` if validation fails due to the specific conditions.
        /// </returns>
        public static bool CheckSumOfBSP03(SNBIRecord record)
        {
            if (record?.SpanSets == null || !record.SpanSets.Any())
                return true;

            int sumBSP03 = 0;
            bool isNotPipeBridge = false;

            foreach (var span in record.SpanSets)
            {
                // Attempt to parse BSP03 as integer
                if (!int.TryParse(span.BSP03?.ToString(), out int bsp03Value) || bsp03Value < 0 || bsp03Value > 999)
                {
                    // BSP03 is not a valid positive integer between 0 and 999, skip validation
                    return true;
                }

                sumBSP03 += bsp03Value;

                // Check if BSP06 is not "P01" or "P02"
                string bsp06 = span.BSP06;
                if (bsp06 != "P01" && bsp06 != "P02")
                {
                    isNotPipeBridge = true;
                }
            }

            // If sum of BSP03 is zero and any BSP06 is not "P01" or "P02", validation fails
            if (isNotPipeBridge && sumBSP03 == 0)
            {
                return false;
            }

            return true;
        }



        /// <summary>
        /// Validates that the sum of BSP03 values in an SNBIRecord is zero when all BSP06 values are "P01" or "P02".
        /// 
        /// Requirements:
        /// 1. **Skip Validation**:
        ///     - If `record` or `record.SpanSets` is null or empty, return `true` immediately.
        /// 
        /// 2. **Validation Fails**:
        ///     - If all `BSP06` values are either "P01" or "P02", and the sum of all valid `BSP03` values is not zero,
        ///       the validation fails, and the method returns `false`.
        /// 
        /// 3. **Validation Passes**:
        ///     - In all other cases, the validation passes, and the method returns `true`.
        /// </summary>
        /// <param name="record">An SNBIRecord containing a collection of SpanSets to validate.</param>
        /// <returns>
        /// Returns `true` if validation passes or is skipped based on the conditions.
        /// Returns `false` if validation fails due to the specific conditions.
        /// </returns>
        public static bool CheckBSP03_0(SNBIRecord record)
        {
            // Check if record or SpanSets is null or empty
            if (record?.SpanSets == null || !record.SpanSets.Any())
                return true; // No spans to validate against

            double sumBSP03 = 0;
            bool isPipeBridge = true;

            foreach (var span in record.SpanSets)
            {
                // Attempt to parse BSP03 as a double
                if (double.TryParse(span.BSP03?.ToString(), out double bsp03Value))
                {
                    sumBSP03 += bsp03Value;
                }

                // Check if BSP06 is not "P01" or "P02"
                if (span.BSP06 != "P01" && span.BSP06 != "P02")
                {
                    isPipeBridge = false;
                }
            }

            // If all BSP06 are "P01" or "P02" and the sum of BSP03 is not zero, validation fails
            if (isPipeBridge && sumBSP03 != 0)
            {
                return false;
            }

            return true;
        }




        //Removed per Wendy's email from 10/30/2024
        //public static bool CheckBSP06_BSP03_For1(object bsp01, SNBIRecord record)
        //{
        //    if (record.SpanSets == null) return true;

        //    var spanSet = record.SpanSets.FirstOrDefault(s => s.BSP01 == bsp01.ToString());

        //    if (spanSet == null) return true;

        //    if ((spanSet.BSP06 == "F01" || spanSet.BSP06 == "F02" || spanSet.BSP06 == "S01" || spanSet.BSP06 == "S02")
        //        && Convert.ToInt32(spanSet.BSP03) != 1)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        public static bool CheckSpanContinuityBSP06(object BSP05, object record)
        {
            if (record is SNBIRecord snbiRecord)
            {
                if (snbiRecord.SpanSets == null) return true;

                string strBSP05 = BSP05?.ToString() ?? string.Empty;
                var patternCOrV = new Regex(Constants.CulvertSpanRegex);

                bool hasMatchingSpanSet = snbiRecord.SpanSets.Any(spanSet =>
                    patternCOrV.IsMatch(spanSet.BSP01 ?? string.Empty) &&
                    (spanSet.BSP06 == "P01" || spanSet.BSP06 == "P02") &&
                    strBSP05 != "7"
                );

                return !hasMatchingSpanSet;
            }

            return true;
        }



        ///////************ POSTING EVALUATIONS ***************///////

        //B.EP.02 - Legal Load Rating Factor - N(4,2)
        public static bool IsValidBEP02(object BEP02)
        {
            if (BEP02 == null || (BEP02 is string str && string.IsNullOrWhiteSpace(str)))
            {
                return true;
            }

            if (!HasMaxLengthDouble(BEP02, 4))
                return true; //Skip validation

            if (BEP02 is double value)
            {
                // Check if BEP02 is within the range 0.00 to 99.99 and has 2 decimal places
                return value <= 99.99 && value >= 0.00 && Math.Round(value, 2) == value;
            }

            return false;
        }

        public static bool IsRequiredBEP02(object BEP02, object BEP01)
        {
            if (BEP01 == null)
                return true;

            return BEP02 != null && !(BEP02 is string str && string.IsNullOrWhiteSpace(str)); //false if null
        }


        public static bool IsValidBEP03(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return true;
            }

            string stringVal = value?.ToString();
            var values = stringVal.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var validator = validatorFactory.Create("LookupValues");

            foreach (var val in values)
            {
                var trimmedVal = val.Trim();

                if (!validator.IsValidCode("BEP03", trimmedVal))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckBEP03_BEP02(object BEP03, object BEP02)
        {
            string bep03Value = BEP03 as string ?? string.Empty;
            double bep02Value = Convert.ToDouble(BEP02 ?? 0);

            if (bep02Value < 1.00 && string.IsNullOrEmpty(bep03Value))
            {
                return false;
            }

            return true;
        }

        public bool CheckPostingTypeBEP03(SNBIRecord record)
        {
            bool isBL07N = record.BL07 == "N";

            bool hasBEP02LessThanOneAndBEP03Null = record.PostingEvaluations
                .Any(evaluation => evaluation.BEP02 < 1.00 && string.IsNullOrEmpty(evaluation.BEP03));

            return isBL07N && hasBEP02LessThanOneAndBEP03Null;
        }

        public static bool CheckBEP04_BEP02ANDBEP03(object BEP04, object BEP03, object BEP02, SNBIRecord record)
        {
            var validBEP03Values = new HashSet<string> { "G", "A", "D", "T", "X" };
            double bep02Value = Convert.ToDouble(BEP02 ?? 0);

            string bep03Value = BEP03 as string ?? string.Empty;
            string bep04Value = BEP04 as string ?? string.Empty;

            bool isCulvert = record.PostingStatuses.Any(postingStatus => postingStatus.BPS01.Contains("C"));

            if (bep02Value < 1.0
                && bep03Value != null
                && isCulvert == false
                && bep03Value.Split('|').Any(value => validBEP03Values.Contains(value))
                && (bep04Value == null || string.IsNullOrEmpty(bep04Value)))
            {
                return false;
            }

            return true;
        }

        public static bool CheckBEP04_BEP02ANDBEP03_DoNotReport(object BEP04, object BEP03, object BEP02, SNBIRecord record)
        {
            var validBEP03Values = new HashSet<string> { "G", "A", "D", "T", "X" };
            double bep02Value = Convert.ToDouble(BEP02 ?? 0);

            string bep03Value = BEP03 as string ?? string.Empty;
            string bep04Value = BEP04 as string ?? string.Empty;

            bool isCulvert = record.PostingStatuses.Any(postingStatus => postingStatus.BPS01.Contains("C"));

            if (bep02Value < 1.0
                && bep03Value != null
                && isCulvert == false
                && (!bep03Value.Split('|').Any(value => validBEP03Values.Contains(value)))
                && !string.IsNullOrEmpty(bep04Value))
            {
                return false;
            }

            return true;
        }


        ///////************ WORKS ***************///////

        public static bool IsValidBW02(object value)
        {
            if (value is int year || (value is string stringValue && int.TryParse(stringValue, out year)))
            {
                // Check if the year is within a realistic range for bridge construction
                return year >= 1800 && year <= DateTime.Now.Year;
            }

            return false;
        }

        public static bool CrossCheckBW01_BW02(object BW01, object BW02)
        {
            if (BW01 == null || BW02 == null)
                return true;

            bool isBw01Int = int.TryParse(BW01.ToString(), out int bw01Value);
            bool isBw02Int = int.TryParse(BW02.ToString(), out int bw02Value);

            // Return the comparison result if both conversions are successful
            return isBw01Int && isBw02Int && bw02Value >= bw01Value;

        }

        public static bool IsValidBW03(object value)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                return true;
            }

            string stringVal = value.ToString().Trim();
            var values = stringVal.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var validator = validatorFactory.Create("LookupValues");

            foreach (var val in values)
            {
                var trimmedVal = val.Trim();

                if (Constants.TemporaryCodes.Contains(trimmedVal))
                {
                    TemporaryCounts.BW03Count++;
                }

                if (!validator.IsValidCode("BW03", trimmedVal))
                {
                    return false;
                }
            }

            // If all values are valid, return true
            return true;
        }

        public static bool WorkedPerformedWorkEventsCrossCheckBW03(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var codes = bw03Value.Split('|');
                bool containsBR1 = codes.Contains("BR1");
                bool hasOtherCodes = codes.Length > 1;

                if (containsBR1 && hasOtherCodes)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckWorkPerformedBW03_BF01(object BW03, SNBIRecord record)
        {
            bool hasWaterway = record.Features.Any(feature => feature.BF01.StartsWith("W"));

            var validBW03Values = new HashSet<string> { "SC1", "SC2", "CP1", "CP2", "CH1" };
            string bw03Value = BW03 as string;

            bool anyValidBW03 = !string.IsNullOrEmpty(bw03Value)
                                && bw03Value.Split('|').Any(code => validBW03Values.Contains(code));

            if (anyValidBW03)
            {
                if (hasWaterway)
                    return true;
                else
                    return false;
            }

            return true;
        }

        public static bool CheckWorkPerformedBW03_BRH01(object BW03, SNBIRecord record)
        {
            var validBW03Values = new HashSet<string> { "RT1", "RT2" };
            bool isBRHValid = record.BRH01 == "N" || record.BRH02 == "N";
            string bw03Value = BW03 as string;

            bool anyValidBW03 = !string.IsNullOrEmpty(bw03Value)
                                && bw03Value.Split('|').Any(code => validBW03Values.Contains(code));

            return !(isBRHValid && anyValidBW03);
        }


        public static bool CheckBW03WorkPerformed_BSP01(object BW03, SNBIRecord record)
        {
            var validBW03Values = new HashSet<string>
            {
                "SP1", "SP2", "SP3", "SP5", "SP6", "SP7",
                "SB1", "SB2", "SB3", "SB5", "SB6", "SB7"
            };

            bool isBSP01Valid = record.SpanSets.Any(span =>
                !string.IsNullOrEmpty(span.BSP01) &&
                (span.BSP01.StartsWith("C") || span.BSP01.StartsWith("V")));

            string bw03Value = BW03 as string ?? string.Empty;

            bool anyValidBW03 = !string.IsNullOrEmpty(bw03Value)
                                && bw03Value.Split('|').Any(code => validBW03Values.Contains(code));

            return !(isBSP01Valid && anyValidBW03);
        }


        public static bool CheckBW03Culvert_BSP01(object BW03, SNBIRecord record)
        {
            var validBW03Values = new HashSet<string>
            {
                "CU2", "CU3", "CU4", "CU5", "CU6", "CU7"
            };

            bool isBSP01Valid = record.SpanSets.Any(span =>
                !string.IsNullOrEmpty(span.BSP01) &&
                (span.BSP01.StartsWith("M") || span.BSP01.StartsWith("A") || span.BSP01.StartsWith("W")));

            string bw03Value = BW03 as string ?? string.Empty;

            bool anyValidBW03 = !string.IsNullOrEmpty(bw03Value)
                                && bw03Value.Split('|').Any(code => validBW03Values.Contains(code));

            return !(isBSP01Valid && anyValidBW03);
        }


        public static bool CheckYearWorkedPeformedDeckRehabilitation(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var bw03Codes = bw03Value.Split('|');

                bool hasDK1 = bw03Codes.Contains("DK1");
                bool hasDK2orDK3 = bw03Codes.Contains("DK2") || bw03Codes.Contains("DK3");

                return !(hasDK1 && hasDK2orDK3);
            }

            return true;
        }

        public static bool CheckYearWorkedPeformedSuperstructureRehabilitation(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var bw03Codes = bw03Value.Split('|');

                bool hasSP1 = bw03Codes.Contains("SP1");
                bool hasSP2orSP3 = bw03Codes.Contains("SP2") || bw03Codes.Contains("SP3");

                return !(hasSP1 && hasSP2orSP3);
            }

            return true;
        }

        public static bool CheckYearWorkedPeformedSubstructureRehabilitation(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var bw03Codes = bw03Value.Split('|');

                bool hasSB1 = bw03Codes.Contains("SB1");
                bool hasSB2orSB3 = bw03Codes.Contains("SB2") || bw03Codes.Contains("SB3");

                return !(hasSB1 && hasSB2orSB3);
            }

            return true;
        }

        public static bool CheckYearWorkedPeformedMajorDeckRehabilitation(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var bw03Codes = bw03Value.Split('|');

                bool hasDK2 = bw03Codes.Contains("DK2");
                bool hasDK3 = bw03Codes.Contains("DK3");

                return !(hasDK2 && hasDK3);
            }

            return true;
        }

        public static bool CheckYearWorkedPeformedSuperstructureMajorDeckRehabilitation(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var bw03Codes = bw03Value.Split('|');

                bool hasSP2 = bw03Codes.Contains("SP2");
                bool hasSP3 = bw03Codes.Contains("SP3");

                return !(hasSP2 && hasSP3);
            }

            return true;
        }

        public static bool CheckYearWorkedPeformedSubstructureMajorDeckRehabilitation(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var bw03Codes = bw03Value.Split('|');

                bool hasSB2 = bw03Codes.Contains("SB2");
                bool hasSB3 = bw03Codes.Contains("SB3");

                return !(hasSB2 && hasSB3);
            }

            return true;
        }

        public static bool CheckYearWorkedPeformedCulvertMajorDeckRehabilitation(object BW03)
        {
            if (BW03 is string bw03Value)
            {
                var bw03Codes = bw03Value.Split('|');

                bool hasCU2 = bw03Codes.Contains("CU2");
                bool hasCU3 = bw03Codes.Contains("CU3");

                return !(hasCU2 && hasCU3);
            }

            return true;
        }



    }
}
