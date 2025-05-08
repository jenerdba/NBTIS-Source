namespace NBTIS.Web.ViewModels
{
    public class AnnouncementsViewModel
    {
        public int AnnouncementId { get; set; }
        public string LoginId { get; set; } = null!;
        public string? AnnouncementText { get; set; }
        public DateTime AnnouncementDate { get; set; }

    }
}
