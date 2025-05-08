using Newtonsoft.Json;
using RulesEngine.Models;
using NBTIS.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NBTIS.Core.Services
{
    public interface IRulesService
    {
        Task<List<RuleResultTree>> ValidateDataAsync(string workflowName, params object[] inputs);
        Task<List<RuleResultTree>> ValidateDataAsync(string workflowName, object input1, object input2);
    }

    public class RulesService : IRulesService
    {
        private readonly RulesEngine.RulesEngine _rulesEngine;

        // Constructor accepts a workflowsPath parameter.
        public RulesService(string workflowsPath)
        {
            var settings = new ReSettings
            {
                CustomTypes = new Type[]
                {
                    typeof(CustomRules),
                    typeof(CompletenessRules)
                }
            };

            // Retrieve the workflows from the specified path.
            var workflowRules = LoadWorkflows(workflowsPath);

            // Initialize the RulesEngine with the workflows and settings.
            _rulesEngine = new RulesEngine.RulesEngine(workflowRules, settings);
        }

        // Helper method to load workflows from the given folder.
        private Workflow[] LoadWorkflows(string workflowsPath)
        {
            var allRules = new List<Workflow>();
            var workflowFiles = Directory.GetFiles(workflowsPath, "*.json");

            foreach (var filePath in workflowFiles)
            {
                var jsonContent = File.ReadAllText(filePath);
                var workflows = JsonConvert.DeserializeObject<List<Workflow>>(jsonContent)
                                ?? throw new Exception("Missing workflows.");
                allRules.AddRange(workflows);
            }
            return allRules.ToArray();
        }

        public async Task<List<RuleResultTree>> ValidateDataAsync(string workflowName, params object[] inputs)
        {
            var results = await _rulesEngine.ExecuteAllRulesAsync(workflowName, inputs);
            return results.Where(result => !result.IsSuccess).ToList();
        }

        public async Task<List<RuleResultTree>> ValidateDataAsync(string workflowName, object input1, object input2)
        {
            var results = await _rulesEngine.ExecuteAllRulesAsync(workflowName, input1, input2);
            return results.Where(result => !result.IsSuccess).ToList();
        }
    }
}
