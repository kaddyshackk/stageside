using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    public class BatchConfiguration : BaseEntityConfiguration<Batch>
    {
        public new void Configure(EntityTypeBuilder<Batch> builder)
        {
            base.Configure(builder);

            builder.ToTable("Batches");
            
            builder.Property(t => t.Source)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(t => t.SourceType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.State)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}