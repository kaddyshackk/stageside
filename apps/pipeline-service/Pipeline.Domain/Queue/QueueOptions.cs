using StageSide.Pipeline.Domain.Queue.Models;

namespace StageSide.Pipeline.Domain.Queue
{
    public class QueueOptions
    {
        public required int BatchDequeueMaxWaitSeconds { get; init; }
        public required int BatchDequeueDelayIntervalMilliseconds { get; init; }
        
        public required Dictionary<string, QueueThresholds> Thresholds { get; init; }
    }
}