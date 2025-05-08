using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBTIS.Core.Settings
{
    public class AppSettings
    {
        public required string Name { get; set; }

        public required string DataProtectionKeyPath { get; set; }

        public required string ApiUrl { get; set; }

        public string EmailServiceUrl { get; set; }

        public string EmailServiceFrom { get; set; }

        public string ServiceAccessCode { get; set; }

        public static AppSettings Current { get; private set; }
    }
}
