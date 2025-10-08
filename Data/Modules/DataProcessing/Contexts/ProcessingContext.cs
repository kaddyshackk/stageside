using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Contexts
{
    /// <summary>
    /// Data processing context.
    /// </summary>
    /// <param name="options">Injected <see cref="DbContextOptions"/> instance.</param>
    public class ProcessingContext(DbContextOptions<ProcessingContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets or sets the SourceRecords DbSet.
        /// </summary>
        public DbSet<SourceRecord> SourceRecords { get; set; }
        
        /// <summary>
        /// Performs additional setup on table creation.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Only apply configuration for SourceRecord
            modelBuilder.ApplyConfiguration(new SourceRecordConfiguration());
        }
    }
}