namespace ComedyPull.Application.Queue
{
    /// <summary>
    /// Interface for queuing bronze records for batch processing.
    /// </summary>
    public interface IQueue<T>
    {
        /// <summary>
        /// Enqueues an item for processing.
        /// </summary>
        /// <param name="record">The <see cref="T"/> to enqueue.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task EnqueueAsync(T record, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dequeues items from the queue.
        /// </summary>
        /// <param name="batchSize">Maximum number of records to dequeue.</param>
        /// <param name="timeout">Timeout for blocking operation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of <see cref="T"/> items, may be empty if timeout occurs.</returns>
        Task<List<T>> DequeueAsync(int batchSize, TimeSpan timeout, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the approximate number of items in the queue.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The number of items in the queue.</returns>
        Task<long> GetQueueLengthAsync(CancellationToken cancellationToken = default);
    }
}