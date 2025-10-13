using ComedyPull.Domain.Modules.Common;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.Public.Events.GetEventBySlug
{
    public class GetEventBySlugContext(DbContextOptions<GetEventBySlugContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets or sets the Events DbSet.
        /// </summary>
        public DbSet<Event> Events { get; set; }
    }
}