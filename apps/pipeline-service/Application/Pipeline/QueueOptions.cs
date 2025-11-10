using ComedyPull.Domain.Queue.Models;

namespace ComedyPull.Application.Pipeline
{
    public class QueueOptions
    {
        public required int BatchDequeueMaxWaitSeconds { get; init; }
        public required int BatchDequeueDelayIntervalMilliseconds { get; init; }
        
        public required Dictionary<string, QueueThresholds> Thresholds { get; init; }
    }
}