using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Lookup_Element
{
    public string ElementNo { get; set; } = null!;

    public string ElementName { get; set; } = null!;

    public string ElementType { get; set; } = null!;

    public string? ElementSubType { get; set; }

    public string MeasureUnit { get; set; } = null!;

    public virtual ICollection<BridgeElement> BridgeElements { get; set; } = new List<BridgeElement>();
}
