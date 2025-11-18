using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.Configurations
{
    public class JobConfiguration : BaseEntityConfiguration<Job>
    {
        public override void Configure(EntityTypeBuilder<Job> builder)
        {
            base.Configure(builder);

            builder.ToTable("Jobs");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.ScheduleId)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(x => x.StartedAt);

            builder.Property(x => x.CompletedAt);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(255);
        }
    }
}