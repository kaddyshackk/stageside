using StageSide.Pipeline.Data.Contexts.Scheduling.Configurations;
using Microsoft.EntityFrameworkCore;
using StageSide.Scheduling.Models;

namespace StageSide.Pipeline.Data.Contexts.Scheduling
{
    public class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : DbContext(options)
    {
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Sitemap> Sitemaps { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new SitemapConfiguration());
        }
    }
}