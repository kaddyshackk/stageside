using StageSide.Pipeline.Domain.Queue.Models;

namespace StageSide.Pipeline.Domain.Queue.Interfaces
{
    public interface IQueueHealthChecker
    {
        Task<QueueHealthStatus> GetQueueStatusAsync<T>(QueueConfig<T> config, int warningThreshold, int criticalThreshold);
        Task<bool> IsQueueHealthyAsync<T>(QueueConfig<T> config, int? customThreshold = null);
    }
}