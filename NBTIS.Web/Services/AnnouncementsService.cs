using Microsoft.EntityFrameworkCore;
using NBTIS.Data; // Adjust the namespace to where your DataContext is defined
using NBTIS.Web.ViewModels;
using Microsoft.Extensions.Logging;
using NBTIS.Data.Models;

namespace NBTIS.Web.Services
{
    public class AnnouncementsService
    {
        private readonly DataContext _context;
        private readonly ILogger<AnnouncementsService> _logger;

        public AnnouncementsService(DataContext context, ILogger<AnnouncementsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AnnouncementsViewModel>> GetAnnouncementsAsync()
        {
            try
            {
                // Query the database for announcements
                var announcements = await _context.Announcements.Where(a => a.AnnouncementDate == _context.Announcements.Max(x => x.AnnouncementDate)).ToListAsync();

                // Map each entity to the view model
                return announcements.Select(a => new AnnouncementsViewModel
                {
                    AnnouncementText = a.AnnouncementText,
                    AnnouncementDate = a.AnnouncementDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new List<AnnouncementsViewModel>(); // Return empty list on failure
            }
        }
    }
}
