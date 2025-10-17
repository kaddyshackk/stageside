using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Modules.Common;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ComedyPull.Data.Modules.Common
{
    /// <summary>
    /// Schema context - manages database migrations for all tables.
    /// This context is used only for migrations and schema management, not for runtime data access.
    /// </summary>
    /// <param name="options">Injected <see cref="DbContextOptions"/> instance.</param>
    public class SchemaContext(DbContextOptions<SchemaContext> options) : DbContext(options)
    {
        public DbSet<BronzeRecord> BronzeRecords { get; set; }
        public DbSet<SilverRecord> SilverRecords { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Comedian> Comedians { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<ComedianEvent> ComedianEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BronzeRecordConfiguration());
            modelBuilder.ApplyConfiguration(new SilverRecordConfiguration());
            modelBuilder.ApplyConfiguration(new BatchConfiguration());
            modelBuilder.ApplyConfiguration(new ComedianConfiguration());
            modelBuilder.ApplyConfiguration(new EventConfiguration());
            modelBuilder.ApplyConfiguration(new VenueConfiguration());
            modelBuilder.ApplyConfiguration(new ComedianEventConfiguration());
        }
    }
}