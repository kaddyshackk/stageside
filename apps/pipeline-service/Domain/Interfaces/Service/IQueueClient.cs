using ComedyPull.Domain.Models.Queue;

namespace ComedyPull.Domain.Interfaces.Service
{
    public interface IQueueClient
    {
        Task EnqueueAsync<T>(QueueConfig<T> config, T item);
        
        Task<T?> DequeueAsync<T>(QueueConfig<T> config);
        
        Task EnqueueBatchAsync<T>(QueueConfig<T> config, IEnumerable<T> items);
        
        Task<IEnumerable<T>> DequeueBatchAsync<T>(QueueConfig<T> config, int count);
        
        Task<long> GetLengthAsync<T>(QueueConfig<T> config);
        
        Task ClearAsync<T>(QueueConfig<T> config);
    }
}