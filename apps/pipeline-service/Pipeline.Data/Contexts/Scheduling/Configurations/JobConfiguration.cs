using StageSide.Pipeline.Data.Models;
using StageSide.Pipeline.Domain.Scheduling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StageSide.Pipeline.Data.Contexts.Scheduling.Configurations
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