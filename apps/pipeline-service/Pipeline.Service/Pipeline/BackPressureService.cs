using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Queue;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using Microsoft.Extensions.Options;
using StageSide.Pipeline.Service.Pipeline.Options;

namespace StageSide.Pipeline.Service.Pipeline
{
    public class BackPressureService(IQueueHealthChecker queueHealthChecker, IOptions<BackPressureOptions> backPressureOptions, IOptions<QueueOptions> queueOptions) : IBackPressureManager
    {
        public async Task<int> CalculateAdaptiveBatchSizeAsync<T>(
            QueueConfig<T> queue,
            int minBatchSize,
            int maxBatchSize)
        {
            if (!backPressureOptions.Value.EnableAdaptiveBatching)
                return maxBatchSize;

            var threshold = GetQueueThresholds(queue);

            var queueStatus = await queueHealthChecker.GetQueueStatusAsync(
                queue, threshold.Warning, threshold.Critical);

            return queueStatus switch
            {
                QueueHealthStatus.Healthy => maxBatchSize,
                QueueHealthStatus.Warning => Math.Max(maxBatchSize / 2, minBatchSize),
                QueueHealthStatus.Critical => Math.Max(maxBatchSize / 4, minBatchSize),
                QueueHealthStatus.Overloaded => minBatchSize,
                _ => maxBatchSize
            };
        }

        public async Task<int> CalculateAdaptiveDelayAsync<T>(
            QueueConfig<T> queue,
            int delaySeconds)
        {
            var threshold = GetQueueThresholds(queue);
            var queueStatus = await queueHealthChecker.GetQueueStatusAsync(
                queue, threshold.Warning, threshold.Critical);

            return queueStatus switch
            {
                QueueHealthStatus.Healthy => delaySeconds,
                QueueHealthStatus.Warning => delaySeconds * 2,
                QueueHealthStatus.Critical => delaySeconds * 4,
                QueueHealthStatus.Overloaded => delaySeconds * 8,
                _ => delaySeconds
            };
        }

        public async Task<bool> ShouldApplyBackPressureAsync<T>(QueueConfig<T> queue)
        {
            if (!backPressureOptions.Value.EnableBackPressure)
                return false;
            var threshold = GetQueueThresholds(queue);
            return !await queueHealthChecker.IsQueueHealthyAsync(queue, threshold.Normal);
        }

        public async Task<QueueHealthStatus> GetQueueHealthStatusAsync<T>(QueueConfig<T> queue)
        {
            var threshold = GetQueueThresholds(queue);
            return await queueHealthChecker.GetQueueStatusAsync(queue, threshold.Warning, threshold.Critical);
        }

        private QueueThresholds GetQueueThresholds<T>(QueueConfig<T> queue)
        {
            queueOptions.Value.Thresholds.TryGetValue(queue.Key, out var threshold);
            if (threshold is null)
            {
                throw new NullReferenceException($"Queue threshold not found for {queue.Key}");
            }
            return threshold;
        }
    }
}