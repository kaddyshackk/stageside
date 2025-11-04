using ComedyPull.Domain.Queue;

namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface IQueueClient
    {
        Task EnqueueAsync<T>(QueueConfig<T> config, T item);

        Task<T?> DequeueAsync<T>(QueueConfig<T> config);

        Task EnqueueBatchAsync<T>(QueueConfig<T> config, IEnumerable<T> items);

        Task<ICollection<T>> DequeueBatchAsync<T>(
            QueueConfig<T> config,
            int maxCount,
            CancellationToken stoppingToken);

        Task<long> GetLengthAsync<T>(QueueConfig<T> config);

        Task ClearAsync<T>(QueueConfig<T> config);
    }
}