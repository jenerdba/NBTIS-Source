using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Lookup_DataItem
{
    public string NBI_Id { get; set; } = null!;

    public string ItemId { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string? DataSet { get; set; }

    public string? Section { get; set; }

    public string? Format { get; set; }

    public string? Identifier { get; set; }
}
