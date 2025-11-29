using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.SchedulingDb.Configurations
{
    public class ScheduleConfiguration : BaseEntityConfiguration<Schedule>
    {
        public override void Configure(EntityTypeBuilder<Schedule> builder)
        {
            base.Configure(builder);

            builder.ToTable("schedules");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Id)
	            .HasColumnName("id");
            
            builder.Property(x => x.SkuId)
	            .HasColumnName("sku_id")
                .IsRequired();
            
            builder.Property(x => x.SourceId)
	            .HasColumnName("source_id")
                .IsRequired();
            
            builder.Property(x => x.Name)
	            .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.CronExpression)
	            .HasColumnName("cron_expression")
                .HasMaxLength(50);

            builder.Property(x => x.NextExecution)
	            .HasColumnName("next_execution")
                .IsRequired();

            builder.Property(x => x.LastExecution)
	            .HasColumnName("last_execution");
            
            builder.Property(x => x.IsActive)
	            .HasColumnName("is_active")
	            .HasDefaultValue(true)
	            .IsRequired();
            
            builder.HasOne(s => s.Sku)
                .WithMany(s => s.Schedules)
                .HasForeignKey(s => s.SkuId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(s => s.Source)
	            .WithMany(s => s.Schedules)
	            .HasForeignKey(s => s.SourceId)
	            .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasIndex(x => x.NextExecution);
            builder.HasIndex(x => new { x.IsActive, x.NextExecution });
        }
    }
}
