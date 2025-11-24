using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.Configurations
{
    public class ScheduleConfiguration : BaseEntityConfiguration<Schedule>
    {
        public override void Configure(EntityTypeBuilder<Schedule> builder)
        {
            base.Configure(builder);

            builder.ToTable("Schedules");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.SkuId)
                .IsRequired();
            
            builder.Property(x => x.SourceId)
                .IsRequired();
            
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CronExpression)
                .HasMaxLength(50);
            
            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.NextExecution)
                .IsRequired();
            
            builder.Property(x => x.LastExecuted);
            
            builder.HasOne(s => s.Sku)
                .WithMany(s => s.Schedules)
                .HasForeignKey(s => s.SkuId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasIndex(x => x.NextExecution);
            builder.HasIndex(x => new { x.IsActive, x.NextExecution });
        }
    }
}