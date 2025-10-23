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
            
            builder.ToTable("SilverRecords");
            
            builder.Property(x => x.BatchId)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(x => x.BronzeRecordId)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.EntityType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.Data)
                .HasMaxLength(5000)
                .HasColumnType("jsonb")
                .IsRequired();
            
            builder.Property(r => r.ContentHash)
                .HasMaxLength(255)
                .IsRequired();
            
            builder.HasIndex(r => r.BatchId);
        }
    }
}