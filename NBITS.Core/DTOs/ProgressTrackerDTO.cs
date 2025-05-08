namespace NBTIS.Core.DTOs
{
    public class ProgressTracker
    {
        public int ProcessedRecords { get; set; } = 0;
        public int PercentCompleted { get; set; } = 10;  // Initialize to start from 10%
    }
}
