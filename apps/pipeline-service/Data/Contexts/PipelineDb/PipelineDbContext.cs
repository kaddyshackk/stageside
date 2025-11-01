using ComedyPull.Data.Contexts.PipelineDb.Configurations;
using ComedyPull.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace ComedyPull.Data.Contexts.PipelineDb
{
    public class PipelineDbContext(DbContextOptions<PipelineDbContext> options) : DbContext(options)
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobExecution> JobExecutions { get; set; }
        public DbSet<JobSitemap> JobSitemaps { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new JobExecutionConfiguration());
            modelBuilder.ApplyConfiguration(new JobSitemapConfiguration());
        }
    }
}