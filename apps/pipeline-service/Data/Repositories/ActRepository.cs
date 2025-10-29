using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Repositories
{
    /// inheritdoc
    public class ActRepository(ComedyDbContext context) : IActRepository
    {
        /// inheritdoc
        public async Task<ICollection<Act>> ReadActsBySlugAsync(IEnumerable<string> slugs)
        {
            var enumerable = slugs.ToList();
            if (enumerable.Count == 0)
                return [];
            return await context.Acts.AsNoTracking().Where(a => enumerable.Contains(a.Slug)).ToListAsync();
        }

        /// inheritdoc
        public async Task<ICollection<Act>> GetActsBySlugAsync(IEnumerable<string> slugs)
        {
            var enumerable = slugs.ToList();
            if (enumerable.Count == 0)
                return [];
            return await context.Acts.Where(a => enumerable.Contains(a.Slug)).ToListAsync();
        }

        /// inheritdoc
        public async Task BulkCreateActsAsync(IEnumerable<Act> acts)
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                context.Acts.AddRange(acts);
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