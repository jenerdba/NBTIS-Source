using NBTIS.Core.DTOs;
using NBTIS.Core.Utilities;
using System.Text.RegularExpressions;

namespace NBTIS.Core.Services
{
    /// <summary>
    /// The class's purpose is to ensure that each section of the record has been populated with at least one dataset.
    /// </summary>
    public class CompletenessRules
    {
        //ELEMENTS
        public static bool HasAtLeastOneElement(SNBIRecord record)
        {
            bool isHighwayFeature = record.Features.Any(f =>
                Regex.IsMatch(f.BF01 ?? string.Empty, @"^H\d{2}$") &&
                (f.BF02 ?? string.Empty) == "C" &&
                (f.BH03 ?? string.Empty) == "Y");

            // If no highway feature is present, return true without further validation
            if (!isHighwayFeature)
            {
                return true;
            }

            // If a highway feature is present, check if there is at least one valid element with a non-null BE01
            return record.Elements.Any(e => e.BE01 != null);
        }

        //FEATURES
        public static bool HasAtLeastOneFeature(SNBIRecord record) //For both Border Bridge and not border bridge
        {
            // Check if the Features collection is not null and contains at least one element
            return record.Features.Any(e => e.BF01 != null); 
        }

    //ROUTES
    public static bool HasAtLeastOneRoute(SNBIRecord record) //For "highway" features
        {
            if (record.Features == null || !record.Features.Any())
            {
                return true;
            }

            // Check each feature to determine if it's a "highway" feature without routes
            foreach (var feature in record.Features)
            {
                if (Regex.IsMatch(feature.BF01 ?? "", @"^H\d{2}$"))
                {
                    // If a "highway" feature has routes, return true immediately
                    if (feature.Routes != null && feature.Routes.Any(x => x.BRT01 != null))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //NAVIGABLE WATERWAYS
        public static bool HasAtLeastOneNavigationSet(SNBIRecord record) //For "waterway" features
        {
            if (record.Features == null || !record.Features.Any())
            {
                return true;
            }

            foreach (var feature in record.Features)
            {
                // Check if the feature is a "waterway" feature
                if (Regex.IsMatch(feature.BF01 ?? "", Constants.WaterwayFeatureRegex))
                {
                    // Return true if a "waterway" feature has a non-null BN01
                    if (feature.BN01 != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        //INSPECTIONS ?

        //POSTING EVALUATIONS - At least one Load Evaluation and Posting dataset must be submitted for all bridges
        public static bool HasAtLeastOnePostingEvaluation(SNBIRecord record)
        {
            // Check if the Posting Evaluations collection is not null and contains at least one valid element where BEP01 is not null
            return record.PostingEvaluations != null && record.PostingEvaluations.Any(x => x.BEP01 != null);
        }

        //POSTING STATUSES - At least one Load Posting Status dataset must be reported for all bridges
        public static bool HasAtLeastOnePostingStatus(SNBIRecord record)
        {
            // Check if the Posting Status collection is not null and contains at least one element
            return record.PostingStatuses != null && record.PostingStatuses.Any(x => x.BPS01 != null);
        }

       //SPAN SETS
        public static bool HasAtLeastOneSpanSet(SNBIRecord record)
        {
            // Check if the SpanSets collection is not null and contains at least one element
            return record.SpanSets != null && record.SpanSets.Any(x => x.BSP01 != null);
        }

      


        //SUBSTRUCTURE SETS
        public static bool HasAtLeastOneSubstructureSet(SNBIRecord record)
        {
            bool allSpansP01_P02 = record.SpanSets.All(span =>
                 span.BSP06 == "P01" || span.BSP06 == "P02");

            if (allSpansP01_P02) {  return true; }

            // Check if the Substructures collection is not null and contains at least one element
            return record.SubstructureSets != null && record.SubstructureSets.Any(x => x.BSB01 != null);
        }

        //WORKS
        public static bool HasAtLeastOneWorkSet(SNBIRecord record)
        {
            // Check if the Works collection is not null and contains at least one BW02 - Year Work Performed 
            return record.Works != null && record.Works.Any(x => x.BW02 != null);
        }






    }
}
