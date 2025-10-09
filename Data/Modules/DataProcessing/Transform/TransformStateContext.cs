using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing.Transform
{
    /// <summary>
    /// Transform state context - handles transformation of source records into application DTOs.
    /// </summary>
    /// <param name="options">Injected <see cref="DbContextOptions"/> instance.</param>
    public class TransformStateContext(DbContextOptions<TransformStateContext> options) : DbContext(options)
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

            modelBuilder.ApplyConfiguration(new SourceRecordConfiguration());
        }
    }
}