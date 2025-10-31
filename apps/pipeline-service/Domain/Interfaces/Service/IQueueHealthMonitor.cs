using ComedyPull.Domain.Models.Queue;

namespace ComedyPull.Domain.Interfaces.Service
{
    public interface IQueueHealthMonitor
    {
        Task<QueueHealth> GetQueueHealthAsync<T>(QueueConfig<T> config);
        Task<IEnumerable<QueueHealth>> GetAllQueueHealthAsync();
        Task<bool> IsQueueHealthyAsync<T>(QueueConfig<T> config, int? customThreshold = null);
        Task<QueueHealthStatus> GetQueueStatusAsync<T>(QueueConfig<T> config, int warningThreshold, int criticalThreshold);
        Task RecordEnqueueAsync<T>(QueueConfig<T> config, int count = 1);
        Task RecordDequeueAsync<T>(QueueConfig<T> config, int count = 1, TimeSpan? processingTime = null);
        Task RecordErrorAsync<T>(QueueConfig<T> config);
        Task<QueueMetrics> GetQueueMetricsAsync<T>(QueueConfig<T> config);
    }
}