using NBTIS.Core.DTOs;
using NBTIS.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBTIS.Core.Utilities;

namespace NBTIS.Web.Services
{
    public class LocationService
    {
        private readonly DataContext _context;
        private readonly ILogger<LocationService> _logger;

        public LocationService(DataContext context, ILogger<LocationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LocationDTO>> GetLocationListAsync(string? option)
        {
            try
            {
                // If the option is "state", query states; otherwise, query agencies.
                if (option?.ToLower() == "state")
                {
                    // Query LookupStates table and map to LocationDTO
                    return await _context.Lookup_States
                        .Select(s => new LocationDTO
                        {
                            Code = s.Code.ToString(),
                            Description = s.Description
                        })
                        .ToListAsync();
                }
                else
                {
                    return await _context.LookupValues
                        .Where(s => s.FieldName == Constants.FedAgencyFieldName &&
                                    s.IsActive == true &&
                                    !s.Code.StartsWith("S") &&
                                    !s.Code.StartsWith("L") &&
                                    !s.Code.StartsWith("P"))
                        .OrderBy(a => a.Description)
                        .Select(a => new LocationDTO
                        {
                            Code = a.Code,
                            Description = a.Description
                        })
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching locations: {Message}", ex.Message);
                return new List<LocationDTO>();
            }
        }
    }
}
