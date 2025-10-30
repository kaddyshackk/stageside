using System.Text.Json;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Models.Queue;
using StackExchange.Redis;

namespace ComedyPull.Data.Services
{
    public class RedisQueueClient(IConnectionMultiplexer redis) : IQueueClient
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
            TimeSpan? maxWait = null,
            TimeSpan? pollingWait = null,
            CancellationToken cancellationToken = default)
        {
            if (maxCount <= 0)
                return [];

            var pollingDelay = pollingWait ?? TimeSpan.FromMilliseconds(100);
            var results = new List<T>();
            var deadline = maxWait.HasValue
                ? DateTime.UtcNow.Add(maxWait.Value)
                : DateTime.MaxValue;

            while (results.Count < maxCount && DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
            {
                var remaining = maxCount - results.Count;
                var queueLength = await _db.ListLengthAsync(config.Key);
                var toDequeue = (int)Math.Min(remaining, queueLength);

                if (toDequeue == 0)
                {
                    if (!maxWait.HasValue || results.Count > 0)
                        break;

                    try
                    {
                        await Task.Delay(pollingDelay, cancellationToken);
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

                // If we got items but not enough and have a timeout, wait a bit before checking again
                if (results.Count < maxCount && maxWait.HasValue && DateTime.UtcNow < deadline)
                {
                    try
                    {
                        await Task.Delay(pollingDelay, cancellationToken);
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