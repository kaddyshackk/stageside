using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataSync
{
    /// <summary>
    /// Data processing context - handles transformation and persistence of source records into domain entities.
    /// </summary>
    /// <param name="options">Injected <see cref="DbContextOptions"/> instance.</param>
    public class DataSyncContext(DbContextOptions<DataSyncContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets or sets the SourceRecords DbSet.
        /// </summary>
        public DbSet<BronzeRecord> BronzeRecords { get; set; }

        /// <summary>
        /// Performs additional setup on table creation.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BronzeRecordConfiguration());
        }
    }
}