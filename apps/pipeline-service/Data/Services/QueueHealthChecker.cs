using ComedyPull.Application.Pipeline;
using ComedyPull.Domain.Queue.Interfaces;
using ComedyPull.Domain.Queue.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ComedyPull.Data.Services
{
    public class QueueHealthChecker(IConnectionMultiplexer redis, IOptions<QueueOptions> options) : IQueueHealthChecker
    {
        private readonly IDatabase _db = redis.GetDatabase();

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

        public async Task<bool> IsQueueHealthyAsync<T>(QueueConfig<T> config, int? customThreshold = null)
        {
            var currentDepth = await _db.ListLengthAsync(config.Key);
            var threshold = customThreshold ?? GetThreshold(config.Key);
            return currentDepth < threshold;
        }

        private int GetThreshold(string queueKey)
        {
            options.Value.Thresholds.TryGetValue(queueKey, out var threshold);
            if (threshold is null)
            {
                throw new NullReferenceException($"Queue threshold not found for {queueKey}");
            }
            return threshold.Normal;
        }
    }
}