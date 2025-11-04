using ComedyPull.Data.Core;
using ComedyPull.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Contexts.PipelineDb.Configurations
{
    public class JobExecutionConfiguration : BaseEntityConfiguration<JobExecution>
    {
        public override void Configure(EntityTypeBuilder<JobExecution> builder)
        {
            base.Configure(builder);

            builder.ToTable("JobExecutions");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.JobId)
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