using Microsoft.EntityFrameworkCore;
using StageSide.Scheduler.Data.Database.Configurations;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database
{
    public class SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : DbContext(options)
    {
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Job> Jobs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
            modelBuilder.ApplyConfiguration(new JobConfiguration());
        }
    }
}