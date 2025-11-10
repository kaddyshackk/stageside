using ComedyPull.Data.Models;
using ComedyPull.Domain.Scheduling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Contexts.Scheduling.Configurations
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
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(x => x.NextExecution)
                .IsRequired();
            
            builder.Property(x => x.LastExecuted);
            
            builder.HasIndex(x => x.NextExecution);
            builder.HasIndex(x => new { x.IsActive, x.NextExecution });
        }
    }
}