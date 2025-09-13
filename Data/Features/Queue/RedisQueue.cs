using System.Text.Json;
using ComedyPull.Application.Enums;
using ComedyPull.Application.Features.Ingest.Interfaces;
using ComedyPull.Domain.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ComedyPull.Data.Features.Queue
{
    /// <summary>
    /// Redis List implementation of bronze record queue.
    /// </summary>
    public class RedisQueue<T>(
        IConnectionMultiplexer redis,
        ILogger<RedisQueue<T>> logger,
        QueueKey key
    )
        : IQueue<T>
    {
        private readonly IDatabase _database = redis.GetDatabase();

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Enqueues a bronze record for processing.
        /// </summary>
        public async Task EnqueueAsync(T record, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(record, _jsonOptions);
            await _database.ListLeftPushAsync(key.GetEnumDescription(), json);
        }

        /// <summary>
        /// Dequeues bronze records from the queue in batches.
        /// </summary>
        public async Task<List<T>> DequeueAsync(int batchSize, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var records = new List<T>();
            
            try
            {
                // Blocking read for first item
                var firstItem = await _database.ListRightPopAsync(key.GetEnumDescription());
                if (!firstItem.HasValue)
                {
                    return records;
                }

                // Deserialize first item
                var firstRecord = JsonSerializer.Deserialize<T>(firstItem!, _jsonOptions);
                if (firstRecord != null)
                {
                    records.Add(firstRecord);
                }

                // Get remaining items
                var remainingItems = Math.Min(batchSize - 1, await GetQueueLengthAsync(cancellationToken));
                if (remainingItems > 0)
                {
                    var additionalItems = await _database.ListRightPopAsync(key.GetEnumDescription(), remainingItems);
                    foreach (var item in additionalItems)
                    {
                        if (!item.HasValue) continue;
                        var record = JsonSerializer.Deserialize<T>(item!, _jsonOptions);
                        if (record != null)
                        {
                            records.Add(record);
                        }
                    }
                }

                logger.LogDebug("Dequeued {Count} bronze records from queue", records.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dequeue bronze records from queue");
                throw;
            }

            return records;
        }

        /// <summary>
        /// Gets the approximate number of items in the queue.
        /// </summary>
        public async Task<long> GetQueueLengthAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _database.ListLengthAsync(key.GetEnumDescription());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get queue length");
                return 0;
            }
        }
    }
}