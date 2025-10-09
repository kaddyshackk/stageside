using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Complete
{
    public class CompleteStateContext(DbContextOptions<CompleteStateContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets or sets the SourceRecords DbSet.
        /// </summary>
        public DbSet<SourceRecord> SourceRecords { get; set; }
        
        /// <summary>
        /// Gets or sets the Comedians DbSet.
        /// </summary>
        public DbSet<Comedian> Comedians { get; set; }

        /// <summary>
        /// Gets or sets the Events DbSet.
        /// </summary>
        public DbSet<Event> Events { get; set; }

        /// <summary>
        /// Gets or sets the Venues DbSet.
        /// </summary>
        public DbSet<Venue> Venues { get; set; }

        /// <summary>
        /// Gets or sets the ComedianEvents DbSet.
        /// </summary>
        public DbSet<ComedianEvent> ComedianEvents { get; set; }
        
        /// <summary>
        /// Performs additional setup on table creation.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new SourceRecordConfiguration());
            modelBuilder.ApplyConfiguration(new ComedianConfiguration());
            modelBuilder.ApplyConfiguration(new EventConfiguration());
            modelBuilder.ApplyConfiguration(new VenueConfiguration());
            modelBuilder.ApplyConfiguration(new ComedianEventConfiguration());
        }
    }
}