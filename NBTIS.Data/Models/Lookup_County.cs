using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Lookup_County
{
    public byte St { get; set; }

    public int Code { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<BridgePrimary> BridgePrimaries { get; set; } = new List<BridgePrimary>();

    public virtual ICollection<Stage_BridgePrimary> Stage_BridgePrimaries { get; set; } = new List<Stage_BridgePrimary>();
}
