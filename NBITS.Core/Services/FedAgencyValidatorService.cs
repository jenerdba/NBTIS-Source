using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NBTIS.Core.Utilities;
using NBTIS.Data.Models;

namespace NBTIS.Core.Services
{
    // DTO for a federal agency lookup item.
    public class FedAgencyItem
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class FedAgencyValidatorService
    {
        private readonly DataContext _context;
        private readonly List<FedAgencyItem> _fedAgencyItems;

        // Inject the DataContext via constructor.
        public FedAgencyValidatorService(DataContext context)
        {
            _context = context;
            _fedAgencyItems = LoadFedAgenciesFromDatabase();
        }

        // Load all federal agencies from the Lookup_FedAgencies table.
        private List<FedAgencyItem> LoadFedAgenciesFromDatabase()
        {
            var items = _context.LookupValues.Where(s => s.FieldName == Constants.FedAgencyFieldName &&
                                    s.IsActive == true &&
                                    !s.Code.StartsWith("S") &&
                                    !s.Code.StartsWith("L") &&
                                    !s.Code.StartsWith("P"))
                .Select(fa => new FedAgencyItem
                {
                    Code = fa.Code,           // Code is a string.
                    Description = fa.Description
                })
                .ToList();

            if (!items.Any())
            {
                throw new Exception("No federal agency lookup values found in the database.");
            }

            return items;
        }

        // Validate if the provided federal agency code exists using a string parameter.
        public bool IsValidFedAgencyCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return _fedAgencyItems.Any(a =>
                string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase));
        }

        // Overload: Validate if the provided federal agency code exists using an object parameter.
        public bool IsValidFedAgencyCode(object code)
        {
            if (code == null)
                return false;

            string codeStr = code.ToString() ?? string.Empty;
            return IsValidFedAgencyCode(codeStr);
        }

        // Get the description by federal agency code (string).
        public string? GetNameByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var agency = _fedAgencyItems.FirstOrDefault(a =>
                string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase));
            return agency?.Description;
        }

        // Overload: Get the description by federal agency code (object).
        public string? GetNameByCode(object code)
        {
            if (code == null)
                return null;

            string codeStr = code.ToString() ?? string.Empty;
            return GetNameByCode(codeStr);
        }
    }
}

