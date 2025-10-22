using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces
{
    /// <summary>
    /// Repository for Transform state operations - handles Bronze to Silver record transformations.
    /// </summary>
    public interface ITransformStateRepository
    {
        /// <summary>
        /// Retrieves all Bronze records associated with a specific batch.
        /// </summary>
        /// <param name="batchId">The unique identifier of the batch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of Bronze records.</returns>
        Task<IEnumerable<BronzeRecord>> GetBronzeRecordsByBatchId(Guid batchId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates new Silver records in the database.
        /// </summary>
        /// <param name="silverRecords">The Silver records to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task CreateSilverRecordsAsync(IEnumerable<SilverRecord> silverRecords, CancellationToken cancellationToken);

        /// <summary>
        /// Updates existing Bronze records in the database.
        /// </summary>
        /// <param name="bronzeRecords">The Bronze records to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateBronzeRecordsAsync(IEnumerable<BronzeRecord> bronzeRecords, CancellationToken cancellationToken);
    }
}