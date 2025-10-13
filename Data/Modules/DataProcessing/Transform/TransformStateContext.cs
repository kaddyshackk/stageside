using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Modules.DataProcessing;
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
        /// Gets or sets the BronzeRecords DbSet.
        /// </summary>
        public DbSet<BronzeRecord> BronzeRecords { get; set; }
        
        /// <summary>
        /// Gets or sets the SilverRecords DbSet.
        /// </summary>
        public DbSet<SilverRecord> SilverRecords { get; set; }
        
        /// <summary>
        /// Performs additional setup on table creation.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BronzeRecordConfiguration());
            modelBuilder.ApplyConfiguration(new SilverRecordConfiguration());
        }
    }
}