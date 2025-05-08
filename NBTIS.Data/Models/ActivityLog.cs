using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class ActivityLog
{
    public long ActivityId { get; set; }

    public DateTime ActivityDate { get; set; }

    public string LoginId { get; set; } = null!;

    public string? OfficeCode { get; set; }

    public string? IPAddress { get; set; }

    public short EventType { get; set; }

    public string? EventDescription { get; set; }

    public string? ScreenName { get; set; }
}
