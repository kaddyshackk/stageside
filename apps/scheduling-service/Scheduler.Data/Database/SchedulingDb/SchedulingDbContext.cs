using Microsoft.EntityFrameworkCore;
using StageSide.Scheduler.Data.Database.SchedulingDb.Configurations;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.SchedulingDb
{
    public class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : DbContext(options)
    {
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Sku> Skus { get; set; }
        public DbSet<Source> Sources { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
            modelBuilder.ApplyConfiguration(new JobConfiguration());
            modelBuilder.ApplyConfiguration(new SkuConfiguration());
            modelBuilder.ApplyConfiguration(new SourceConfiguration());
        }
    }
}
