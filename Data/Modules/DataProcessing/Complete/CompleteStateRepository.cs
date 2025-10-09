using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Complete
{
    public class CompleteStateRepository(IDbContextFactory<CompleteStateContext> contextFactory) : ICompleteStateRepository
    {
        private CompleteStateContext? _context;

        public async Task<IEnumerable<SourceRecord>> GetRecordsByBatchAsync(string batchId, CancellationToken cancellationToken = default)
        {
            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.SourceRecords
                .Where(r => r.BatchId == batchId)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_context != null)
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _context.DisposeAsync();
                _context = null;
            }
        }
        
        public async Task<Comedian?> GetComedianBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            _context ??= await contextFactory.CreateDbContextAsync(cancellationToken);
            return await _context.Comedians
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<Venue?> GetVenueBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            _context ??= await contextFactory.CreateDbContextAsync(cancellationToken);
            return await _context.Venues
                .FirstOrDefaultAsync(v => v.Slug == slug, cancellationToken);
        }

        public async Task AddComedian(Comedian comedian)
        {
            _context ??= await contextFactory.CreateDbContextAsync();
            _context.Comedians.Add(comedian);
        }

        public async Task AddVenue(Venue venue)
        {
            _context ??= await contextFactory.CreateDbContextAsync();
            _context.Venues.Add(venue);
        }

        public async Task AddEvent(Event eventEntity)
        {
            _context ??= await contextFactory.CreateDbContextAsync();
            _context.Events.Add(eventEntity);
        }

        public async Task AddComedianEvent(ComedianEvent comedianEvent)
        {
            _context ??= await contextFactory.CreateDbContextAsync();
            _context.ComedianEvents.Add(comedianEvent);
        }
    }    
}