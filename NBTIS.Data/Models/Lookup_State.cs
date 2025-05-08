using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Lookup_State
{
    public byte Code { get; set; }

    public string? Description { get; set; }

    public string? Abbreviation { get; set; }

    public virtual ICollection<Stage_BridgePrimary> Stage_BridgePrimaries { get; set; } = new List<Stage_BridgePrimary>();
}
