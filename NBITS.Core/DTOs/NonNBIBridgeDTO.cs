using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBTIS.Core.DTOs
{
    public class NonNBIBridge
    {
            public int? BL01 { get; set; }
            public string? BID01 { get; set; }
            public string? BCL01 { get; set; }
            public double? BG01 { get; set; } // NBIS Bridge Length
            public double? BG02 { get; set; } // Total Bridge Length
        
    }
}
