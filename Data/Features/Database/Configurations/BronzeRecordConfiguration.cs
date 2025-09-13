using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Features.Database.Configurations
{
    public class BronzeRecordConfiguration : TraceableEntityConfiguration<BronzeRecord>
    {
        public new void Configure(EntityTypeBuilder<BronzeRecord> builder)
        {
            base.Configure(builder);
            
            builder.Property(r => r.EntityType)
                .HasMaxLength(200)
                .IsRequired();
            
            builder.Property(r => r.ExternalId)
                .HasMaxLength(200)
                .IsRequired();
            
            builder.Property(r => r.RawData)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(r => r.Processed)
                .HasDefaultValue(false);

            builder.Property(r => r.ProcessedAt);
        }
    }
}