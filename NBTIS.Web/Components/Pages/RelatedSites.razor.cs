using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

public class RelatedBase : ComponentBase
{
    // These properties will be filled from the query string.
    [Parameter, SupplyParameterFromQuery]
    public long? submitId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string submittedByDescription { get; set; } = string.Empty;


    protected List<RelatedSite> RelatedSites = new()
    {
        new RelatedSite
        {
            Title = "Office of Bridges and Structures",
            Url = "https://www.fhwa.dot.gov/bridge/",
            SubLinks = new List<RelatedSite>
            {
                new RelatedSite { Title = "Bridge Inspection", Url = "https://www.fhwa.dot.gov/bridge/nbi.cfm" },
                new RelatedSite { Title = "Tunnel Inspection", Url = "https://www.fhwa.dot.gov/bridge/inspection/tunnel/" }
            }
        },
        new RelatedSite { Title = "LTBP InfoBridge Web Portal", Url = "https://infobridge.fhwa.dot.gov/Home" },
        new RelatedSite { Title = "National Highway System (NHS) MAPS", Url = "http://www.fhwa.dot.gov/planning/national_highway_system/nhs_maps/" },
        new RelatedSite { Title = "ANSI (Formerly FIPS) Codes", Url = "https://www.census.gov/library/reference/code-lists/ansi.html" }
    };
}

public class RelatedSite
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<RelatedSite>? SubLinks { get; set; }
}
