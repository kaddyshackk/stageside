using System.Collections.Concurrent;
using ComedyPull.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Application.Queue
{
    /// <summary>
    /// In-memory implementation of queue for processing records.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    public class InMemoryQueue<T>(ILogger<InMemoryQueue<T>> logger) : IQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new();
        private readonly SemaphoreSlim _semaphore = new(0);

        /// <summary>
        /// Enqueues an item for processing.
        /// </summary>
        public Task EnqueueAsync(T record, CancellationToken cancellationToken = default)
        {
            _queue.Enqueue(record);
            _semaphore.Release();
            logger.LogDebug("Enqueued item to in-memory queue. Queue length: {QueueLength}", _queue.Count);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dequeues items from the queue in batches.
        /// </summary>
        public async Task<List<T>> DequeueAsync(int batchSize, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var records = new List<T>();
            try
            {
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
                
                try
                {
                    await _semaphore.WaitAsync(combinedCts.Token);
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    return records;
                }

                if (_queue.TryDequeue(out var firstItem))
                {
                    records.Add(firstItem);
                }

                for (var i = 1; i < batchSize; i++)
                {
                    if (_queue.TryDequeue(out var item))
                    {
                        records.Add(item);
                        await _semaphore.WaitAsync(0, combinedCts.Token);
                    }
                    else
                    {
                        break;
                    }
                }

                logger.LogDebug("Dequeued {Count} items from in-memory queue. Remaining: {Remaining}", 
                    records.Count, _queue.Count);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("Dequeue operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dequeue items from in-memory queue");
                throw;
            }

            return records;
        }

        /// <summary>
        /// Gets the approximate number of items in the queue.
        /// </summary>
        public Task<long> GetQueueLengthAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((long)_queue.Count);
        }
    }
}