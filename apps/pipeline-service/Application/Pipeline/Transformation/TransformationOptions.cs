namespace ComedyPull.Application.Pipeline.Transformation
{
    public class TransformationOptions
    {
        public int BatchMaxSize { get; init; }
        public int BatchMaxWaitSeconds { get; init; }
        public int BatchDelaySeconds { get; init; }
        public int PollingWaitSeconds { get; init; }
    }
}