using ComedyPull.Domain.Models;

namespace ComedyPull.Application.Features.DataProcessing.Interfaces
{
    /// <summary>
    /// A repository for interacting with BronzeRecord's.
    /// </summary>
    public interface IBronzeRecordRepository
    {
        /// <summary>
        /// Batch inserts a list of <see cref="BronzeRecord"/>.
        /// </summary>
        /// <param name="records">Bronze records to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task BatchInsertAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken);
    }
}