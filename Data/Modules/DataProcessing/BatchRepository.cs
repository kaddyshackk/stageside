using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing
{
    /// <summary>
    /// Repository for managing Batch entity operations across all processing states.
    /// </summary>
    public class BatchRepository(IDbContextFactory<BatchContext> contextFactory) : IBatchRepository
    {
        /// <inheritdoc />
        public async Task<Batch> GetBatchById(string batchId, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var batch = await context.Batches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

            if (batch == null)
            {
                throw new InvalidOperationException($"Batch with ID '{batchId}' not found.");
            }

            return batch;
        }

        /// <inheritdoc />
        public async Task UpdateBatchStateById(string batchId, ProcessingState state, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var batch = await context.Batches
                .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

            if (batch == null)
            {
                throw new InvalidOperationException($"Batch with ID '{batchId}' not found.");
            }

            batch.State = state;
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Batch> CreateBatch(DataSource source, DataSourceType sourceType, string createdBy, CancellationToken cancellationToken)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            var batch = new Batch
            {
                Source = source,
                SourceType = sourceType,
                State = ProcessingState.Created,
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = createdBy,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedBy = createdBy
            };

            context.Batches.Add(batch);
            await context.SaveChangesAsync(cancellationToken);

            return batch;
        }
    }
}
