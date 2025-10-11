using ComedyPull.Domain.Modules.DataProcessing;

namespace ComedyPull.Application.Modules.DataSync.Interfaces
{
    public interface IBronzeRecordIngestionRepository
    {
        public Task BatchInsertAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken = default);
    }
}