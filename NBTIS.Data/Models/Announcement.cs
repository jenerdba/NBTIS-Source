using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Announcement
{
    public int AnnouncementId { get; set; }

    public string LoginId { get; set; } = null!;

    public string? AnnouncementText { get; set; }

    public DateTime AnnouncementDate { get; set; }

    public bool Archived { get; set; }

    public bool Emailed { get; set; }

    public string? Subject { get; set; }

    public DateTime? EmailingDate { get; set; }

    public bool Draft { get; set; }

    public string? EmailingTime { get; set; }
}
