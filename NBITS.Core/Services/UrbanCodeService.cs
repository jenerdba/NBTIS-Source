using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace NBTIS.Core.Services
{
   
    public class UrbanCodeService
    {
        private static readonly Lazy<UrbanCodeService> _instance = new Lazy<UrbanCodeService>(() => new UrbanCodeService());
        private Dictionary<int, HashSet<int>> _urbanCodes;

        // Private constructor to prevent instantiation outside
        private UrbanCodeService()
        {
            LoadUrbanCodes();
        }

        // Public property to access the instance
        public static UrbanCodeService Instance => _instance.Value;

        private void LoadUrbanCodes()
        {
            string jsonPath = "Lists/BH02.json";
            using (var jsonFileReader = File.OpenText(jsonPath))
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var urbanCodesList = JsonSerializer.Deserialize<List<StateUrbanCode>>(jsonFileReader.ReadToEnd(), options);

                _urbanCodes = urbanCodesList.ToDictionary(
                    u => u.StateCode,
                    u => new HashSet<int>(u.UrbanCodes));
            }
        }

        public bool TryGetUrbanCodes(int stateCode, out HashSet<int> urbanCodes)
        {
            return _urbanCodes.TryGetValue(stateCode, out urbanCodes);
        }
    }

    // Helper class to deserialize JSON data
    public class StateUrbanCode
    {
        public int StateCode { get; set; }
        public List<int> UrbanCodes { get; set; }
    }

}
