using StageSide.Pipeline.Data.Contexts.Scheduling.Configurations;
using StageSide.Pipeline.Domain.Scheduling.Models;
using Microsoft.EntityFrameworkCore;

namespace StageSide.Pipeline.Data.Contexts.Scheduling
{
    public class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : DbContext(options)
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Execution> Executions { get; set; }
        public DbSet<Sitemap> Sitemaps { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new ExecutionConfiguration());
            modelBuilder.ApplyConfiguration(new SitemapConfiguration());
        }
    }
}