using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Domain.Core.Events;
using ComedyPull.Domain.Core.Events.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Core
{
    /// inheritdoc
    public class EventRepository(ComedyDbContext context) : IEventRepository
    {
        /// inheritdoc
        public async Task<ICollection<Event>> GetEventsBySlugAsync(IEnumerable<string> slugs, CancellationToken stoppingToken)
        {
            var enumerable = slugs.ToList();
            if (enumerable.Count == 0)
                return [];
            return await context.Events.Where(e => enumerable.Contains(e.Slug)).ToListAsync(stoppingToken);
        }

        /// inheritdoc
        public async Task BulkCreateEventsAsync(IEnumerable<Event> events, CancellationToken stoppingToken)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                context.Events.AddRange(events);
                await context.SaveChangesAsync(stoppingToken);
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// inheritdoc
        public async Task SaveChangesAsync(CancellationToken stoppingToken)
        {
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}