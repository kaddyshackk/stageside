using ComedyPull.Domain.Modules.DataProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Configurations
{
    public class BronzeRecordConfiguration : BaseEntityConfiguration<BronzeRecord>
    {
        public new void Configure(EntityTypeBuilder<BronzeRecord> builder)
        {
            base.Configure(builder);
            
            builder.Property(x => x.BatchId)
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