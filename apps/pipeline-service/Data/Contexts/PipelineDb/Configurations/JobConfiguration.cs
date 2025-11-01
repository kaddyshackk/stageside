using ComedyPull.Data.Models;
using ComedyPull.Domain.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Contexts.PipelineDb.Configurations
{
    public class JobConfiguration : BaseEntityConfiguration<Job>
    {
        public override void Configure(EntityTypeBuilder<Job> builder)
        {
            base.Configure(builder);

            builder.ToTable("Jobs");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Source)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(x => x.Sku)
                .HasConversion<string>()
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.CronExpression)
                .HasMaxLength(50);
            
            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.LastExecuted);
            
            builder.Property(x => x.NextExecution)
                .IsRequired();
            
            builder.HasIndex(x => x.NextExecution);
            builder.HasIndex(x => new { x.IsActive, x.NextExecution });
        }
    }
}