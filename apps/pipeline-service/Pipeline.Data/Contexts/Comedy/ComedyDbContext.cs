using StageSide.Pipeline.Data.Contexts.Comedy.Configurations;
using StageSide.Pipeline.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace StageSide.Pipeline.Data.Contexts.Comedy
{
    /// <summary>
    /// Schema context - manages database migrations for all tables.
    /// This context is used only for migrations and schema management, not for runtime data access.
    /// </summary>
    /// <param name="options">Injected <see cref="DbContextOptions"/> instance.</param>
    public class ComedyDbContext(DbContextOptions<ComedyDbContext> options) : DbContext(options)
    {
        public DbSet<Act> Acts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<EventAct> EventActs { get; set; }

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