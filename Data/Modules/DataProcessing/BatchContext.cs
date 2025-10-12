using ComedyPull.Data.Configurations;
using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Modules.DataProcessing
{
    /// <summary>
    /// Batch context - handles batch orchestration metadata operations across all processing states.
    /// </summary>
    /// <param name="options">Injected <see cref="DbContextOptions"/> instance.</param>
    public class BatchContext(DbContextOptions<BatchContext> options) : DbContext(options)
    {
        /// <summary>
        /// Gets or sets the Batches DbSet.
        /// </summary>
        public DbSet<Batch> Batches { get; set; }

        /// <summary>
        /// Performs additional setup on table creation.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BatchConfiguration());
        }
    }
}
