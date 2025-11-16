namespace StageSide.Pipeline.Domain.Queue
{
    public record QueueHealth
    {
        public string QueueName { get; init; } = string.Empty;
        public long CurrentDepth { get; init; }
        public long MaxDepth { get; init; }
        public QueueHealthStatus Status { get; init; }
        public double ProcessingRate { get; init; }
        public DateTime LastUpdated { get; init; }
        public TimeSpan AverageProcessingTime { get; init; }
        public int ErrorCount { get; init; }
    }
}