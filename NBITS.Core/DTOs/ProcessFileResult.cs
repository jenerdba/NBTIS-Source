using RulesEngine.Models;
using NBTIS.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBTIS.Core.DTOs
{
    public class ProcessFileResult
    {
        public ProcessingReport ReportData { get; set; }
        public List<RuleResultTree> ruleResults {  get; set; }
        public List<SNBIRecord> StagingData { get; set; }
    }

  
}
