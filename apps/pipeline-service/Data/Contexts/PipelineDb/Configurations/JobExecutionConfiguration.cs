using ComedyPull.Data.Models;
using ComedyPull.Domain.Models.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Contexts.PipelineDb.Configurations
{
    public class JobExecutionConfiguration : BaseEntityConfiguration<JobExecution>
    {
        public new void Configure(EntityTypeBuilder<JobExecution> builder)
        {
            base.Configure(builder);

            builder.ToTable("JobExecutions");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.JobId)
                .IsRequired();
            
            builder.Property(x => x.StartedAt)
                .IsRequired();
            
            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CompletedAt)
                .IsRequired();
            
            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(255)
                .IsRequired();
            
            builder.Property(x => x.ProcessedUrls)
                .IsRequired();
            
            builder.Property(x => x.TotalUrls)
                .IsRequired();
        }
    }
}