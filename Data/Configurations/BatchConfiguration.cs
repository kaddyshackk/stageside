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
            
            builder.Property(t => t.Source)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(t => t.SourceType)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.State)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.Status)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
        }
    }
}