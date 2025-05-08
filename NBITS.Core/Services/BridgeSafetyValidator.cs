using RulesEngine.Models;
using NBTIS.Core.DTOs;


public class BridgeSafetyValidator
{
    public static async Task ValidateAsync(SNBIRecord record, List<RuleResultTree> resultsForRecord)
    {
        try
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            double? blr06 = record.BLR06;
            double? blr07 = record.BLR07;

            // Fetch the latest BPS01 based on date and ensure it's not "C"
            var latestPostingStatus = record.PostingStatuses?
            .Where(s => !string.IsNullOrEmpty(s.BPS02))  // Ensure BPS02 is not null or empty
            .OrderByDescending(s => DateTime.ParseExact(s.BPS02, "yyyyMMdd", null))
            .FirstOrDefault();

            string bps01 = latestPostingStatus?.BPS01 ?? "C";  // Default to "C" if not available

            // Separate checks for each condition
            await Task.Run(() =>
            {
                CheckAndAddRule(bps01 != "C" && blr06 < 0.1,
                    "BLR06-PS01-Check",
                    "Operating Load Rating Factor (BLR06) is less than 0.1 and Load Posting Status (BPS01) is not 'C'",
                    resultsForRecord, record);

                CheckAndAddRule(bps01 != "C" && ParseConditionRating(record.BC01) < 2,
                    "BC01-PS01-Check",
                    "Deck Condition Rating (BC01) is less than 2 and Load Posting Status (BPS01) is not 'C'",
                    resultsForRecord, record);

                CheckAndAddRule(bps01 != "C" && ParseConditionRating(record.BC02) < 2,
                    "BC02-PS01-Check",
                    "Superstructure Condition Rating (BC02) is less than 2 and Load Posting Status (BPS01) is not 'C'",
                    resultsForRecord, record);

                CheckAndAddRule(bps01 != "C" && ParseConditionRating(record.BC03) < 2,
                    "BC03-PS01-Check",
                    "Substructure Condition Rating (BC03) is less than 2 and Load Posting Status (BPS01) is not 'C'",
                    resultsForRecord, record);

                CheckAndAddRule(bps01 != "C" && ParseConditionRating(record.BC04) < 2,
                    "BC04-PS01-Check",
                    "Culvert Condition Rating (BC04) is less than 2 and Load Posting Status (BPS01) is not 'C'",
                    resultsForRecord, record);

                var postedStatuses = new[] { "PP", "TP", "SP", "PR", "TR", "SR", "C" };
                CheckAndAddRule(!postedStatuses.Contains(bps01) && blr07 < 1.0,
                    "BLR07-PS01-Check",
                    "Controlling Legal Load Rating Factor (BLR07) is less than 1.0 and Load Posting Status (BPS01) is not PP, TP, SP, PR, TR, SR, or C",
                    resultsForRecord, record);
            });
        }
        catch (Exception e) {
            // Add custom message to the exception and preserve the stack trace.
            throw new Exception($"Error processing Bridge Number {record.BID01}: {e.Message}", e);
        }
    }

    private static void CheckAndAddRule(bool condition, string ruleName, string expression, List<RuleResultTree> resultsForRecord, SNBIRecord record)
    {
        if (condition)
        {
            RuleResultTree result = new RuleResultTree
            {
                Rule = new Rule { 
                    RuleName = ruleName, 
                    Expression = expression,
                    Properties = new Dictionary<string, object>
                    {
                         { "ErrorType", "Safety" },
                         { "DataSet", "Safety" }
                    },
                },
                ActionResult = new ActionResult { Exception = new Exception("Validation failed: " + expression) },
                ExceptionMessage = expression,
                Inputs = new Dictionary<string, object> { { "input1", record } },
                IsSuccess = false
            };
            resultsForRecord.Add(result);
        }
    }

    private static double? ParseConditionRating(string condition)
    {
        if (condition != "N" && double.TryParse(condition, out double result))
        {
            return result;
        }
        return null;  // Returns null if condition is "N" or not a number
    }
}
