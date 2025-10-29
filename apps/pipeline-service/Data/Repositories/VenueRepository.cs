using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Repositories
{
    /// inheritdoc
    public class VenueRepository(ComedyDbContext context) : IVenueRepository
    {
        /// inheritdoc
        public async Task<ICollection<Venue>> GetVenuesBySlugAsync(IEnumerable<string> slugs)
        {
            var enumerable = slugs.ToList();
            if (enumerable.Count == 0)
                return [];
            return await context.Venues.Where(v => enumerable.Contains(v.Slug)).ToListAsync();
        }

        /// inheritdoc
        public async Task BulkCreateVenuesAsync(IEnumerable<Venue> venues)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                context.Venues.AddRange(venues);
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