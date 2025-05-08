using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class LookupValue
{
    public int Id { get; set; }

    public string? FieldName { get; set; }

    public string? Code { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? LastModified { get; set; }
}
