using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Email
{
    public int Id { get; set; }

    public string From { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime EmailDate { get; set; }
}
