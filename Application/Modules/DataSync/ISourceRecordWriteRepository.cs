using ComedyPull.Domain.Models.Processing;

namespace ComedyPull.Application.Modules.DataSync
{
    /// <summary>
    /// A repository for interacting with BronzeRecord's.
    /// </summary>
    public interface ISourceRecordWriteRepository
    {
        /// <summary>
        /// Batch inserts a list of <see cref="SourceRecord"/>.
        /// </summary>
        /// <param name="records">Bronze records to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task BatchInsertAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken);
    }
}