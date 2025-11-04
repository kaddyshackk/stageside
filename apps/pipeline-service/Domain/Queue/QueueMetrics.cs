namespace ComedyPull.Domain.Queue
{
    public record QueueMetrics
    {
        public long EnqueueCount { get; set; }
        public long DequeueCount { get; set; }
        public long ErrorCount { get; set; }
        public DateTime LastEnqueue { get; set; }
        public DateTime LastDequeue { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        public long ProcessedCount { get; set; }
    }
}