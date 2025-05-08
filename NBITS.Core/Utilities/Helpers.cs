using System.Collections;

namespace NBTIS.Core.Utilities
{
    public class Helpers
    {
        //public static class DictionaryHelper
        //{
        //    // This method converts Dictionary<Type, IList> to Dictionary<string, IList> for DuplicateRecords
        //    public static List<KeyValuePair<string, IList>>? ConvertDuplicateRecordsToSerializableFormat(List<KeyValuePair<Type, IList>>? duplicateRecords)
        //    {
        //        if (duplicateRecords == null) return null;

        //        var serializableRecords = duplicateRecords
        //            .Select(kvp => new KeyValuePair<string, IList>(
        //                kvp.Key.FullName ?? kvp.Key.Name,
        //                kvp.Value
        //            ))
        //            .ToList();

        //        return serializableRecords;
        //    }
        //}
    }
}
