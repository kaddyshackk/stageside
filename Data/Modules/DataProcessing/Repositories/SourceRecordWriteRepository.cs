using ComedyPull.Application.Modules.DataSync;
using ComedyPull.Data.Modules.DataProcessing.Contexts;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Repositories
{
    /// <summary>
    /// A repository for interacting with SourceRecord's.
    /// </summary>
    /// <param name="contextFactory">Injected <see cref="IDbContextFactory{ComedyContext}"/> instance.</param>
    public class SourceRecordWriteRepository(IDbContextFactory<ProcessingContext> contextFactory) : ISourceRecordWriteRepository
    {
        /// <summary>
        /// Batch inserts a list of <see cref="SourceRecord"/>.
        /// </summary>
        /// <param name="records">List of <see cref="SourceRecord"/> entities to insert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task BatchInsertAsync(IEnumerable<SourceRecord> records, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                await context.SourceRecords.AddRangeAsync(records, cancellationToken);
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