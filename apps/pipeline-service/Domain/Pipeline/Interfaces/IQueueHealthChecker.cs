using ComedyPull.Domain.Models.Queue;

namespace ComedyPull.Domain.Interfaces.Service
{
    public interface IQueueHealthChecker
    {
        Task<QueueHealthStatus> GetQueueStatusAsync<T>(QueueConfig<T> config, int warningThreshold, int criticalThreshold);
        Task<bool> IsQueueHealthyAsync<T>(QueueConfig<T> config, int? customThreshold = null);
    }
}