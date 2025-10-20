using ComedyPull.Domain.Enums;
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
        /// Creates a new batch.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="sourceType">The data source type.</param>
        /// <param name="createdBy">The user who created the batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created batch entity.</returns>
        Task<Batch> CreateBatch(DataSource source, DataSourceType sourceType, string createdBy, CancellationToken cancellationToken);
    }
}
