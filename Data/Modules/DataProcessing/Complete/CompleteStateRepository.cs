using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Domain.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Complete
{
    /// <summary>
    /// Repository for Complete state operations - handles Silver to Gold entity transformations.
    /// </summary>
    public class CompleteStateRepository : ICompleteStateRepository
    {
        private readonly IDbContextFactory<CompleteStateContext> _contextFactory;

        public CompleteStateRepository(IDbContextFactory<CompleteStateContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Silver record operations
        /// <inheritdoc />
        public async Task<IEnumerable<SilverRecord>> GetSilverRecordsByBatchId(string batchId, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var records = await context.SilverRecords
                .AsNoTracking()
                .Where(r => r.BatchId == batchId)
                .ToListAsync(cancellationToken);

            return records;
        }

        /// <inheritdoc />
        public async Task UpdateSilverRecordsAsync(IEnumerable<SilverRecord> records, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.SilverRecords.UpdateRange(records);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Comedian (Gold) operations
        /// <inheritdoc />
        public async Task<IEnumerable<Comedian>> GetComediansBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var comedians = await context.Comedians
                .AsNoTracking()
                .Where(c => slugs.Contains(c.Slug))
                .ToListAsync(cancellationToken);

            return comedians;
        }

        /// <inheritdoc />
        public async Task AddComediansAsync(IEnumerable<Comedian> comedians, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.Comedians.AddRange(comedians);
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateComediansAsync(IEnumerable<Comedian> comedians, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.Comedians.UpdateRange(comedians);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Venue (Gold) operations
        /// <inheritdoc />
        public async Task<IEnumerable<Venue>> GetVenuesBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var venues = await context.Venues
                .AsNoTracking()
                .Where(v => slugs.Contains(v.Slug))
                .ToListAsync(cancellationToken);

            return venues;
        }

        /// <inheritdoc />
        public async Task AddVenuesAsync(IEnumerable<Venue> venues, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.Venues.AddRange(venues);
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateVenuesAsync(IEnumerable<Venue> venues, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.Venues.UpdateRange(venues);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Event (Gold) operations
        /// <inheritdoc />
        public async Task<IEnumerable<Event>> GetEventsBySlugAsync(IEnumerable<string> slugs, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var events = await context.Events
                .AsNoTracking()
                .Where(e => slugs.Contains(e.Slug))
                .ToListAsync(cancellationToken);

            return events;
        }

        /// <inheritdoc />
        public async Task AddEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.Events.AddRange(events);
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateEventsAsync(IEnumerable<Event> events, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.Events.UpdateRange(events);
            await context.SaveChangesAsync(cancellationToken);
        }

        // ComedianEvent (many-to-many) operations
        /// <inheritdoc />
        public async Task AddComedianEventsAsync(IEnumerable<ComedianEvent> comedianEvents, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            context.ComedianEvents.AddRange(comedianEvents);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
