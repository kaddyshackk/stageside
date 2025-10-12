using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing
{
    /// <summary>
    /// Repository for managing Batch entity operations across all processing states.
    /// </summary>
    public class BatchRepository : IBatchRepository
    {
        private readonly IDbContextFactory<BatchContext> _contextFactory;

        public BatchRepository(IDbContextFactory<BatchContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <inheritdoc />
        public async Task<Batch> GetBatchById(string batchId, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

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
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

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
        public async Task UpdateBatchStatusById(string batchId, ProcessingStatus status, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var batch = await context.Batches
                .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

            if (batch == null)
            {
                throw new InvalidOperationException($"Batch with ID '{batchId}' not found.");
            }

            batch.Status = status;
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
