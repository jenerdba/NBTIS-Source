using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBTIS.Data.Models;

namespace NBTIS.Core.Services
{
    // DTO for a state lookup item.
    public class StateItem
    {
        public byte Code { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
    }

    public class StateValidatorService
    {
        private readonly DataContext _context;
        private readonly ILogger<StateValidatorService> _logger;
        private readonly List<StateItem> _stateItems;

        // Inject the DataContext via constructor.
        public StateValidatorService(DataContext context, ILogger<StateValidatorService> logger)
        {
            _context = context;
            _stateItems = LoadStatesFromDatabase();
            _logger = logger;
        }

        // Load all states from the Lookup_States table.
        private List<StateItem> LoadStatesFromDatabase()
        {

            try
            {
                var items = _context.Lookup_States
                .Select(ls => new StateItem
                {
                    Code = ls.Code,
                    Description = ls.Description
                })
                .ToList();

                if (!items.Any())
                {
                    throw new Exception("No state lookup values found in the database.");
                }

                return items;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error occurred while loading states from the database.");
                return new List<StateItem>();
            }
        }

        // Overload: Validate if the provided state code exists using a byte parameter.
        public bool IsValidStateCode(byte code)
        {
            return _stateItems.Any(s => s.Code == code);
        }

        // Overload: Validate if the provided state code exists using a string parameter.
        public bool IsValidStateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (!byte.TryParse(code, out byte codeByte))
                return false;

            return IsValidStateCode(codeByte);
        }

        public string? GetAbbreviationByCode(byte? code)
        {
            if (code == null || code == 0)
            {
                return null;
            }

            var state = _stateItems.FirstOrDefault(a => a.Code == code);
            return state?.Abbreviation;
        }

        public bool IsValidStateCode(object code)
        {
            if (code == null)
                return false;

            if (code is byte byteValue)
                return IsValidStateCode(byteValue);

            // Otherwise, try to parse the object’s string representation as a byte
            if (byte.TryParse(code.ToString(), out byte parsedValue))
                return IsValidStateCode(parsedValue);

            return false;
        }

        public string? GetAbbreviationByCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!byte.TryParse(code, out byte parsedCode))
                return null;

            return GetAbbreviationByCode(parsedCode);
        }

        public string? GetNameByCode(byte? code)
        {
            if (code == null || code == 0)
            {
                return null;
            }

            var state = _stateItems.FirstOrDefault(a => a.Code == code);
            return state?.Description;
        }

        public string? GetNameByCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!byte.TryParse(code, out byte parsedCode))
                return null;

            return GetNameByCode(parsedCode);
        }
    }
}
