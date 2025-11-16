namespace StageSide.Pipeline.Service.Pipeline.Options
{
    public class ProcessingOptions
    {
        public int DelayIntervalSeconds { get; init; }
        public int MinBatchSize { get; init; }
        public int MaxBatchSize { get; init; }
    }
}