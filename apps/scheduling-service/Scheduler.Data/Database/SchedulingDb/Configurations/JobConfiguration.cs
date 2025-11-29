using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.SchedulingDb.Configurations
{
    public class JobConfiguration : BaseEntityConfiguration<Job>
    {
        public override void Configure(EntityTypeBuilder<Job> builder)
        {
            base.Configure(builder);

            builder.ToTable("jobs");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Id)
	            .HasColumnName("id");
            
            builder.Property(x => x.ScheduleId)
	            .HasColumnName("schedule_id")
                .IsRequired();

            builder.Property(x => x.Status)
	            .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.StartedAt)
	            .HasColumnName("started_at");

            builder.Property(x => x.CompletedAt)
	            .HasColumnName("completed_at");

            builder.Property(x => x.ErrorMessage)
	            .HasColumnName("error_message")
                .HasMaxLength(255);
            
            builder.HasOne(s => s.Schedule)
	            .WithMany(s => s.Jobs)
	            .HasForeignKey(s => s.ScheduleId)
	            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
