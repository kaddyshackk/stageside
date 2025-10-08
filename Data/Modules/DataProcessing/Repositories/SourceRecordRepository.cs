using ComedyPull.Application.Modules.DataProcessing.Repositories.Interfaces;
using ComedyPull.Data.Modules.DataProcessing.Contexts;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Repositories
{
    public class SourceRecordRepository(ProcessingContext context) : ISourceRecordRepository
    {
        public async Task<DataSource> GetBatchSourceAsync(string batchId, CancellationToken cancellationToken = default)
        {
            var source = await context.SourceRecords
                .Where(r => r.BatchId == batchId)
                .Select(r => r.Source)
                .FirstOrDefaultAsync(cancellationToken);

            if (source == default(DataSource))
            {
                throw new InvalidOperationException($"No records found for batch {batchId}");
            }

            return source;
        }

        public async Task<IEnumerable<SourceRecord>> GetRecordsByBatchAsync(string batchId, CancellationToken cancellationToken = default)
        {
            return await context.SourceRecords
                .Where(r => r.BatchId == batchId)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}