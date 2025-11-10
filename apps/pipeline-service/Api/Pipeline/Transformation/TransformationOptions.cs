namespace ComedyPull.Api.Pipeline.Transformation
{
    public class TransformationOptions
    {
        public int DelayIntervalSeconds { get; init; }
        public int MinBatchSize { get; init; }
        public int MaxBatchSize { get; init; }
    }
}