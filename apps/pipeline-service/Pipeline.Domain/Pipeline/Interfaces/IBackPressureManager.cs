using StageSide.Pipeline.Domain.Queue;

namespace StageSide.Pipeline.Domain.Pipeline.Interfaces
{
    public interface IBackPressureManager
    {
        Task<int> CalculateAdaptiveBatchSizeAsync<T>(
            QueueConfig<T> queue,
            int minBatchSize,
            int maxBatchSize);

        Task<int> CalculateAdaptiveDelayAsync<T>(
            QueueConfig<T> queue,
            int delaySeconds);

        Task<bool> ShouldApplyBackPressureAsync<T>(QueueConfig<T> queue);

        Task<QueueHealthStatus> GetQueueHealthStatusAsync<T>(QueueConfig<T> queue);
    }
}