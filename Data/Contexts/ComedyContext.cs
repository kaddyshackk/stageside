using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Contexts
{
    /// <summary>
    /// Default DbContext configuration.
    /// </summary>
    public class ComedyContext : DbContext
    {
        /// <summary>
        /// Gets or sets the Comedians DbSet.
        /// </summary>
        public DbSet<Comedian> Comedians { get; set; }
        
        /// <summary>
        /// Gets or sets the ComedianEvents DbSet.
        /// </summary>
        public DbSet<ComedianEvent> ComedianEvents { get; set; }
        
        /// <summary>
        /// Gets or sets the Events DbSet.
        /// </summary>
        public DbSet<Event> Events { get; set; }
        
        /// <summary>
        /// Gets or sets the Venues DbSet.
        /// </summary>
        public DbSet<Venue> Venues { get; set; }

        /// <summary>
        /// Performs additional setup on table creation.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ComedyContext).Assembly);
        }
    }
}