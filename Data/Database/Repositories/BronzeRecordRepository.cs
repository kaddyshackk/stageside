using ComedyPull.Application.Features.DataProcessing.Interfaces;
using ComedyPull.Data.Database.Contexts;
using ComedyPull.Domain.Models;

namespace ComedyPull.Data.Database.Repositories
{
    /// <summary>
    /// A repository for interacting with BronzeRecord's.
    /// </summary>
    /// <param name="context">Injected <see cref="ComedyContext"/> instance.</param>
    public class BronzeRecordRepository(ComedyContext context) : IBronzeRecordRepository
    {
        /// <summary>
        /// Batch inserts a list of <see cref="BronzeRecord"/>.
        /// </summary>
        /// <param name="records">List of <see cref="BronzeRecord"/> entities to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task BatchInsertAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                await context.BronzeRecords.AddRangeAsync(records, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
                context.ChangeTracker.Clear();
            }
        }
    }
}