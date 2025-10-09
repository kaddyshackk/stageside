using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Transform
{
    public class TransformStateRepository(IDbContextFactory<TransformStateContext> contextFactory) : ITransformStateRepository
    {
        public async Task<IEnumerable<SourceRecord>> GetRecordsByBatchAsync(string batchId, CancellationToken cancellationToken = default)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.SourceRecords
                .Where(r => r.BatchId == batchId)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}