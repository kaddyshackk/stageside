using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Data.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataSync
{
    public class BronzeRecordIngestionRepository(IDbContextFactory<ComedyPullContext> contextFactory) : IBronzeRecordIngestionRepository
    {
        public async Task BatchInsertAsync(IEnumerable<BronzeRecord> records, CancellationToken cancellationToken = default)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            context.BronzeRecords.AddRange(records);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}