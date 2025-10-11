using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces
{
    public interface ITransformStateRepository
    {
        Task<Batch> GetBatchById(string batchId, CancellationToken cancellationToken);
        Task UpdateBatchStateById(string batchId, ProcessingState state, CancellationToken cancellationToken);
        Task UpdateBatchStatusById(string batchId, ProcessingStatus status, CancellationToken cancellationToken);
        Task<IEnumerable<BronzeRecord>> GetBronzeRecordsByBatchId(string batchId, CancellationToken cancellationToken);
        Task CreateSilverRecordsAsync(IEnumerable<SilverRecord> silverRecords, CancellationToken cancellationToken);
        Task UpdateBronzeRecordsAsync(IEnumerable<BronzeRecord> bronzeRecords, CancellationToken cancellationToken);
    }
}