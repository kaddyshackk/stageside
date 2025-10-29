using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Repositories
{
    /// inheritdoc
    public class EventRepository(ComedyDbContext context) : IEventRepository
    {
        /// inheritdoc
        public async Task<ICollection<Event>> GetEventsBySlugAsync(IEnumerable<string> slugs)
        {
            var enumerable = slugs.ToList();
            if (enumerable.Count == 0)
                return [];
            return await context.Events.Where(e => enumerable.Contains(e.Slug)).ToListAsync();
        }

        /// inheritdoc
        public async Task BulkCreateEventsAsync(IEnumerable<Event> events)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                context.Events.AddRange(events);
                await context.SaveChangesAsync();
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// inheritdoc
        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}