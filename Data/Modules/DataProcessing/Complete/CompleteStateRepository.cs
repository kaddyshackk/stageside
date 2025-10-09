using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Complete
{
    public class CompleteStateRepository(CompleteStateContext context) : ICompleteStateRepository
    {
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
        
        public async Task<Comedian?> GetComedianBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await context.Comedians
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<Venue?> GetVenueBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await context.Venues
                .FirstOrDefaultAsync(v => v.Slug == slug, cancellationToken);
        }

        public void AddComedian(Comedian comedian)
        {
            context.Comedians.Add(comedian);
        }

        public void AddVenue(Venue venue)
        {
            context.Venues.Add(venue);
        }

        public void AddEvent(Event eventEntity)
        {
            context.Events.Add(eventEntity);
        }

        public void AddComedianEvent(ComedianEvent comedianEvent)
        {
            context.ComedianEvents.Add(comedianEvent);
        }
    }    
}