using ComedyPull.Application.Modules.DataProcessing.Repositories.Interfaces;
using ComedyPull.Data.Modules.DataSync.Contexts;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataSync.Repositories
{
    public class ComedyRepository(ComedyContext context) : IComedyRepository
    {
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

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
