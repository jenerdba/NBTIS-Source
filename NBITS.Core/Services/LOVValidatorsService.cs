using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NBTIS.Data.Models;

namespace NBTIS.Core.Services
{
    public class ListItem
    {
        public int? id { get; set; }
        public string? fieldName { get; set; }
        public string? st { get; set; }
        public string? code { get; set; }
        public string? description { get; set; }
    }

    public class LOVValidatorsService
    {
        // Thread-safe cache of validator instances by lookupName.
        private static readonly ConcurrentDictionary<string, LOVValidatorsService> lookupCache =
            new(StringComparer.OrdinalIgnoreCase);

        // Each service instance holds its own cached code list.
        private readonly List<ListItem> codesList;

        // Private constructor prevents direct instantiation.
        private LOVValidatorsService(List<ListItem> codesList)
        {
            this.codesList = codesList;
        }

        // Retrieves or creates a cached validator for the given lookupName.
        public static LOVValidatorsService GetValidator(DataContext context, string lookupName)
        {
            return lookupCache.GetOrAdd(lookupName, _ =>
            {
                // Load from DB once and store in memory for repeated use.
                var codes = LoadCodesFromDatabase(context, lookupName);
                return new LOVValidatorsService(codes);
            });
        }

        // Queries the DataContext for lookup values.
        private static List<ListItem> LoadCodesFromDatabase(DataContext context, string lookupName)
        {
            if (lookupName.Equals("LookupStates", StringComparison.OrdinalIgnoreCase))
            {
                // Query the LookupStates table.
                var items = context.Lookup_States
                    .Select(s => new ListItem
                    {
                        fieldName = "LookupStates",
                        code = s.Code.ToString(),  // Convert byte to string.
                        description = s.Description
                    })
                    .ToList();

                if (!items.Any())
                    throw new Exception($"No lookup values found for '{lookupName}'.");

                return items;
            }
            else if (lookupName.Equals("LookupCounties", StringComparison.OrdinalIgnoreCase))
            {
                // Query the LookupCounties table.
                var items = context.Lookup_Counties
                    .Select(s => new ListItem
                    {
                        fieldName = "LookupCounties",
                        st = s.St.ToString(),
                        code = s.Code.ToString(),
                        description = s.Name
                    })
                    .ToList();

                if (!items.Any())
                    throw new Exception($"No lookup values found for '{lookupName}'.");

                return items;
            }
            //else if (lookupName.Equals("LookupOwners", StringComparison.OrdinalIgnoreCase))
            //{
            //    // Query the LookupOwners table.
            //    var items = context.LookupOwners
            //        .Select(s => new ListItem
            //        {
            //            fieldName = "LookupOwners",
            //            code = s.Code.ToString(),  // Convert byte to string.
            //            description = s.Description
            //        })
            //        .ToList();

            //    if (!items.Any())
            //        throw new Exception($"No lookup values found for '{lookupName}'.");

            //    return items;
            //}
            else
            {
                // Default: Query the LookupValues table.
                var items = context.LookupValues
                    //.Where(lv => lv.FieldName == lookupName)
                    .Select(lv => new ListItem
                    {
                        id = lv.Id,
                        fieldName = lv.FieldName,
                        code = lv.Code,
                        description = lv.Description
                    })
                    .ToList();

                if (!items.Any())
                    throw new Exception($"No lookup values found for FieldName = '{lookupName}'.");

                return items;
            }
        }

        public bool IsValidCode(string lookupName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var values = value.Split('|');

            foreach (var singleValue in values)
            {
                var trimmedValue = singleValue.Trim();

                bool isValid =
                    // Found in the cached codes list matching the given lookupName.
                    codesList
                        .Where(lv => lv.fieldName == lookupName)
                        .Any(c => c.code != null &&
                                  c.code.Equals(trimmedValue, StringComparison.OrdinalIgnoreCase))
                    // OR it matches a specific pattern.
                    || Regex.IsMatch(trimmedValue, @"^(M|A|S)\d{2}(1|2|3|4|5|6)?$", RegexOptions.IgnoreCase);

                if (!isValid)
                    return false;
            }
            return true;
        }


        public bool IsValidStateCode(string stateCode)
        {
            if (string.IsNullOrWhiteSpace(stateCode))
                return false;
            if (!byte.TryParse(stateCode, out byte parsedCode))
                return false;
            return codesList.Any(c =>
                byte.TryParse(c.code, out byte cachedCode) &&
                cachedCode == parsedCode);
        }

        public bool IsValidCountyCode(string? stateCode, string? countyCode)
        {
            if (string.IsNullOrWhiteSpace(stateCode) || string.IsNullOrWhiteSpace(countyCode))
                return false;
            return codesList.Any(item =>
                !string.IsNullOrWhiteSpace(item.st) &&
                item.st.Equals(stateCode, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(item.code) &&
                item.code.Equals(countyCode, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsValidOwnerCode(string ownerCode)
        {
            if (string.IsNullOrWhiteSpace(ownerCode))
                return false;
            if (!byte.TryParse(ownerCode, out byte parsedCode))
                return false;
            return codesList.Any(c =>
                byte.TryParse(c.code, out byte cachedCode) &&
                cachedCode == parsedCode);
        }
    }
}
