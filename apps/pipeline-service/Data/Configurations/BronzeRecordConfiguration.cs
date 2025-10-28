using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    public class BronzeRecordConfiguration : BaseEntityConfiguration<BronzeRecord>
    {
        public new void Configure(EntityTypeBuilder<BronzeRecord> builder)
        {
            base.Configure(builder);

            builder.ToTable("BronzeRecords");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.BatchId)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.State)
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
            
            // Indices
            builder.HasIndex(r => r.BatchId);
        }
    }
}