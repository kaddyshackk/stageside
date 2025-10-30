namespace ComedyPull.Application.Pipeline.Processing
{
    public class ProcessingOptions
    {
        public int BatchMaxSize { get; init; }

        public int BatchMaxWaitSeconds { get; init; }
        public int BatchDelaySeconds { get; init; }
        public int PollingWaitSeconds { get; init; }
    }
}