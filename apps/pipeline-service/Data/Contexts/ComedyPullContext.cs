using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Contexts
{
    /// <summary>
    /// Schema context - manages database migrations for all tables.
    /// This context is used only for migrations and schema management, not for runtime data access.
    /// </summary>
    /// <param name="options">Injected <see cref="DbContextOptions"/> instance.</param>
    public class ComedyPullContext(DbContextOptions<ComedyPullContext> options) : DbContext(options)
    {
        // All entities
        public DbSet<Act> Comedians { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<EventAct> ComedianEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new ActConfiguration());
            modelBuilder.ApplyConfiguration(new EventConfiguration());
            modelBuilder.ApplyConfiguration(new VenueConfiguration());
            modelBuilder.ApplyConfiguration(new EventActConfiguration());
        }
    }
}