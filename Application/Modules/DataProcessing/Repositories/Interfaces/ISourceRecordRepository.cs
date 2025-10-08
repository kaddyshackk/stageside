using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models.Processing;

namespace ComedyPull.Application.Modules.DataProcessing.Repositories.Interfaces
{
    public interface ISourceRecordRepository
    {
        Task<DataSource> GetBatchSourceAsync(string batchId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SourceRecord>> GetRecordsByBatchAsync(string batchId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}