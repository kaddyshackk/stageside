namespace ComedyPull.Application.Pipeline
{
    public class QueueOptions
    {
        public int BatchDequeueMaxWaitSeconds { get; init; }
        public int BatchDequeueDelayIntervalMilliseconds { get; init; }
    }
}