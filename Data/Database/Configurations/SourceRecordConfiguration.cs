using ComedyPull.Domain.Models.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Database.Configurations
{
    public class SourceRecordConfiguration : TraceableEntityConfiguration<SourceRecord>
    {
        public new void Configure(EntityTypeBuilder<SourceRecord> builder)
        {
            base.Configure(builder);
            
            builder.Property(x => x.BatchId)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.EntityType)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)")
                .IsRequired();
            
            builder.Property(r => r.RecordType)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)")
                .IsRequired();
            
            builder.Property(r => r.Status)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.Stage)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)")
                .IsRequired();
            
            builder.Property(r => r.RawData)
                .HasMaxLength(5000)
                .HasColumnType("jsonb")
                .IsRequired();

            builder.Property(r => r.ProcessedData)
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