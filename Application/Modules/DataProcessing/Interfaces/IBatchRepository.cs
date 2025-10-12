using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Interfaces
{
    /// <summary>
    /// Repository for managing Batch entity operations across all processing states.
    /// </summary>
    public interface IBatchRepository
    {
        /// <summary>
        /// Retrieves a batch by its unique identifier.
        /// </summary>
        /// <param name="batchId">The unique identifier of the batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The batch entity.</returns>
        Task<Batch> GetBatchById(string batchId, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the processing state of a batch.
        /// </summary>
        /// <param name="batchId">The unique identifier of the batch.</param>
        /// <param name="state">The new processing state.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateBatchStateById(string batchId, ProcessingState state, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the processing status of a batch.
        /// </summary>
        /// <param name="batchId">The unique identifier of the batch.</param>
        /// <param name="status">The new processing status.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateBatchStatusById(string batchId, ProcessingStatus status, CancellationToken cancellationToken);
    }
}
