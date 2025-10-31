using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Queue;
using StackExchange.Redis;

namespace ComedyPull.Data.Services
{
    public class QueueHealthMonitor(IConnectionMultiplexer redis) : IQueueHealthMonitor
    {
        private readonly IDatabase _db = redis.GetDatabase();

        private static string GetMetricsKey(string queueKey) => $"metrics:{queueKey}";

        public async Task<QueueHealth> GetQueueHealthAsync<T>(QueueConfig<T> config)
        {
            var currentDepth = await _db.ListLengthAsync(config.Key);
            var metrics = await GetQueueMetricsAsync(config);
            
            var processingRate = CalculateProcessingRate(metrics);
            var averageProcessingTime = CalculateAverageProcessingTime(metrics);
            
            return new QueueHealth
            {
                QueueName = config.Key,
                CurrentDepth = currentDepth,
                Status = DetermineHealthStatus(currentDepth),
                ProcessingRate = processingRate,
                LastUpdated = DateTime.UtcNow,
                AverageProcessingTime = averageProcessingTime,
                ErrorCount = (int)metrics.ErrorCount
            };
        }

        public async Task<IEnumerable<QueueHealth>> GetAllQueueHealthAsync()
        {
            var queueHealthTasks = new[]
            {
                GetQueueHealthAsync(Queues.Collection),
                GetQueueHealthAsync(Queues.DynamicCollection),
                GetQueueHealthAsync(Queues.Transformation),
                GetQueueHealthAsync(Queues.Processing)
            };

            return await Task.WhenAll(queueHealthTasks);
        }

        public async Task<bool> IsQueueHealthyAsync<T>(QueueConfig<T> config, int? customThreshold = null)
        {
            var currentDepth = await _db.ListLengthAsync(config.Key);
            var threshold = customThreshold ?? GetDefaultThreshold(config.Key);
            return currentDepth < threshold;
        }

        public async Task<QueueHealthStatus> GetQueueStatusAsync<T>(QueueConfig<T> config, int warningThreshold, int criticalThreshold)
        {
            var currentDepth = await _db.ListLengthAsync(config.Key);
            return currentDepth switch
            {
                _ when currentDepth >= criticalThreshold * 1.5 => QueueHealthStatus.Overloaded,
                _ when currentDepth >= criticalThreshold => QueueHealthStatus.Critical,
                _ when currentDepth >= warningThreshold => QueueHealthStatus.Warning,
                _ => QueueHealthStatus.Healthy
            };
        }

        public async Task RecordEnqueueAsync<T>(QueueConfig<T> config, int count = 1)
        {
            var metricsKey = GetMetricsKey(config.Key);
            var batch = _db.CreateBatch();
            
            await batch.HashIncrementAsync(metricsKey, "enqueue_count", count);
            await batch.HashSetAsync(metricsKey, "last_enqueue", DateTime.UtcNow.Ticks);
            await batch.KeyExpireAsync(metricsKey, TimeSpan.FromDays(7));
            
            batch.Execute();
            await Task.CompletedTask;
        }

        public async Task RecordDequeueAsync<T>(QueueConfig<T> config, int count = 1, TimeSpan? processingTime = null)
        {
            var metricsKey = GetMetricsKey(config.Key);
            var batch = _db.CreateBatch();
            
            await batch.HashIncrementAsync(metricsKey, "dequeue_count", count);
            await batch.HashIncrementAsync(metricsKey, "processed_count", count);
            await batch.HashSetAsync(metricsKey, "last_dequeue", DateTime.UtcNow.Ticks);
            
            if (processingTime.HasValue)
            {
                await batch.HashIncrementAsync(metricsKey, "total_processing_time_ms", (long)processingTime.Value.TotalMilliseconds);
            }
            
            await batch.KeyExpireAsync(metricsKey, TimeSpan.FromDays(7));
            
            batch.Execute();
            await Task.CompletedTask;
        }

        public async Task RecordErrorAsync<T>(QueueConfig<T> config)
        {
            var metricsKey = GetMetricsKey(config.Key);
            await _db.HashIncrementAsync(metricsKey, "error_count", 1);
            await _db.KeyExpireAsync(metricsKey, TimeSpan.FromDays(7));
        }

        public async Task<QueueMetrics> GetQueueMetricsAsync<T>(QueueConfig<T> config)
        {
            var metricsKey = GetMetricsKey(config.Key);
            var hash = await _db.HashGetAllAsync(metricsKey);
            
            var metrics = new QueueMetrics();
            
            foreach (var item in hash)
            {
                switch (item.Name.ToString().ToLowerInvariant())
                {
                    case "enqueue_count":
                        metrics.EnqueueCount = item.Value.HasValue ? (long)item.Value : 0;
                        break;
                    case "dequeue_count":
                        metrics.DequeueCount = item.Value.HasValue ? (long)item.Value : 0;
                        break;
                    case "error_count":
                        metrics.ErrorCount = item.Value.HasValue ? (long)item.Value : 0;
                        break;
                    case "processed_count":
                        metrics.ProcessedCount = item.Value.HasValue ? (long)item.Value : 0;
                        break;
                    case "total_processing_time_ms":
                        var processingTimeMs = item.Value.HasValue ? (long)item.Value : 0;
                        metrics.TotalProcessingTime = TimeSpan.FromMilliseconds(processingTimeMs);
                        break;
                    case "last_enqueue":
                        if (item.Value.HasValue && long.TryParse(item.Value, out var enqueueTicks))
                            metrics.LastEnqueue = new DateTime(enqueueTicks);
                        break;
                    case "last_dequeue":
                        if (item.Value.HasValue && long.TryParse(item.Value, out var dequeueTicks))
                            metrics.LastDequeue = new DateTime(dequeueTicks);
                        break;
                }
            }
            
            return metrics;
        }

        private static double CalculateProcessingRate(QueueMetrics metrics)
        {
            if (metrics.ProcessedCount == 0 || metrics.LastDequeue == default || metrics.LastEnqueue == default)
                return 0;

            var timeSpan = metrics.LastDequeue - metrics.LastEnqueue;
            return timeSpan.TotalMinutes > 0 ? metrics.ProcessedCount / timeSpan.TotalMinutes : 0;
        }

        private static TimeSpan CalculateAverageProcessingTime(QueueMetrics metrics)
        {
            return metrics.ProcessedCount > 0
                ? TimeSpan.FromMilliseconds(metrics.TotalProcessingTime.TotalMilliseconds / metrics.ProcessedCount)
                : TimeSpan.Zero;
        }

        private static QueueHealthStatus DetermineHealthStatus(long currentDepth)
        {
            return currentDepth switch
            {
                < 100 => QueueHealthStatus.Healthy,
                < 500 => QueueHealthStatus.Warning,
                < 1000 => QueueHealthStatus.Critical,
                _ => QueueHealthStatus.Overloaded
            };
        }

        private static int GetDefaultThreshold(string queueKey)
        {
            return queueKey.ToLowerInvariant() switch
            {
                var key when key.Contains("collection") => 1000,
                var key when key.Contains("dynamic") => 500,
                var key when key.Contains("transformation") => 2000,
                var key when key.Contains("processing") => 1000,
                _ => 500
            };
        }
    }
}