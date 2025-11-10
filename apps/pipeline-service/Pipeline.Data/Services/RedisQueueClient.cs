using System.Text.Json;
using StageSide.Pipeline.Domain.Queue;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using StageSide.Pipeline.Domain.Queue.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace StageSide.Pipeline.Data.Services
{
    public class RedisQueueClient(IConnectionMultiplexer redis, IOptions<QueueOptions> options) : IQueueClient
    {
        private readonly IDatabase _db = redis.GetDatabase();
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task EnqueueAsync<T>(QueueConfig<T> config, T item)
        {
            var json = JsonSerializer.Serialize(item, _jsonOptions);
            await _db.ListRightPushAsync(config.Key, json);
        }

        public async Task<T?> DequeueAsync<T>(QueueConfig<T> config)
        {
            var json = await _db.ListLeftPopAsync(config.Key);
            
            return json.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(json!, _jsonOptions);
        }

        public async Task EnqueueBatchAsync<T>(QueueConfig<T> config, IEnumerable<T> items)
        {
            var values = items
                .Select(item => (RedisValue) JsonSerializer.Serialize(item, _jsonOptions))
                .ToArray();
            
            if (values.Length > 0)
                await _db.ListRightPushAsync(config.Key, values);
        }

        public async Task<ICollection<T>> DequeueBatchAsync<T>(
            QueueConfig<T> config,
            int maxCount,
            CancellationToken stoppingToken)
        {
            if (maxCount <= 0)
                return [];

            var pollingDelay = TimeSpan.FromMilliseconds(options.Value.BatchDequeueDelayIntervalMilliseconds);
            var maxWait = TimeSpan.FromSeconds(options.Value.BatchDequeueMaxWaitSeconds);
            var results = new List<T>();
            var deadline = DateTime.UtcNow.Add(maxWait);

            while (results.Count < maxCount && DateTime.UtcNow < deadline && !stoppingToken.IsCancellationRequested)
            {
                var remaining = maxCount - results.Count;
                var queueLength = await _db.ListLengthAsync(config.Key);
                var toDequeue = (int)Math.Min(remaining, queueLength);

                if (toDequeue == 0)
                {
                    if (results.Count > 0)
                        break;

                    try
                    {
                        await Task.Delay(pollingDelay, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    continue;
                }

                // Dequeue available items in a batch
                var batch = _db.CreateBatch();
                var tasks = new List<Task<RedisValue>>();

                for (var i = 0; i < toDequeue; i++)
                {
                    tasks.Add(batch.ListLeftPopAsync(config.Key));
                }

                batch.Execute();
                await Task.WhenAll(tasks);

                foreach (var task in tasks)
                {
                    var json = await task;
                    if (json.IsNullOrEmpty) continue;
                    var item = JsonSerializer.Deserialize<T>(json!, _jsonOptions);
                    if (item != null)
                        results.Add(item);
                }

                if (results.Count < maxCount && DateTime.UtcNow < deadline)
                {
                    try
                    {
                        await Task.Delay(pollingDelay, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return results;
        }

        public async Task<long> GetLengthAsync<T>(QueueConfig<T> config)
        {
            return await _db.ListLengthAsync(config.Key);
        }

        public async Task ClearAsync<T>(QueueConfig<T> config)
        {
            await _db.KeyDeleteAsync(config.Key);
        }
    }
}