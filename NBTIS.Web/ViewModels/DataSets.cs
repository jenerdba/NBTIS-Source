namespace NBTIS.Web.ViewModels
{
    public class DataSets
    {
      public List<string> DataTypes { get; set; } = new()
        {
            "Primary",
            "Elements",
            "Features",
            "Inspections",
            "Posting Evaluation",
            "Posting Status",
            "Routes",
            "Span Sets",
            "Substructure Sets",
            "Work"
        };
    }
}
