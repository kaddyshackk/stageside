using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Data.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing
{
    /// <summary>
    /// Repository for Transform state operations - handles Bronze to Silver record transformations.
    /// </summary>
    public class TransformStateRepository(IDbContextFactory<ComedyPullContext> contextFactory)
        : ITransformStateRepository
    {
        /// <inheritdoc />
        public async Task<IEnumerable<BronzeRecord>> GetBronzeRecordsByBatchId(Guid batchId, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var records = await context.BronzeRecords
                .AsNoTracking()
                .Where(r => r.BatchId == batchId)
                .ToListAsync(cancellationToken);

            return records;
        }

        /// <inheritdoc />
        public async Task CreateSilverRecordsAsync(IEnumerable<SilverRecord> silverRecords, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            context.SilverRecords.AddRange(silverRecords);
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateBronzeRecordsAsync(IEnumerable<BronzeRecord> bronzeRecords, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            context.BronzeRecords.UpdateRange(bronzeRecords);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
