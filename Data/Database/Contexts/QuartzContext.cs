using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Database.Contexts
{
    /// <summary>
    /// Quartz scheduling context.
    /// </summary>
    public class QuartzContext(DbContextOptions<QuartzContext> options) : DbContext(options)
    {
        /// <summary>
        /// Performs additional setup on table creation.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.HasDefaultSchema("quartz");
        }
    }
}