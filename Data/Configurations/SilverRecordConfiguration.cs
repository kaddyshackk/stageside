using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    public class SilverRecordConfiguration : BaseEntityConfiguration<SilverRecord>
    {
        public new void Configure(EntityTypeBuilder<SilverRecord> builder)
        {
            base.Configure(builder);
            
            builder.Property(x => x.BatchId)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(x => x.BronzeRecordId)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.EntityType)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.Status)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.Data)
                .HasMaxLength(5000)
                .HasColumnType("jsonb")
                .IsRequired();
            
            builder.Property(r => r.ContentHash)
                .HasMaxLength(255)
                .HasColumnType("varchar(255)")
                .IsRequired();
        }
    }
}