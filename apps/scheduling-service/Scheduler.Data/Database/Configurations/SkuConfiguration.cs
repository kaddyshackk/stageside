using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database.Configurations;

public class SkuConfiguration : BaseEntityConfiguration<Sku>
{
    public override void Configure(EntityTypeBuilder<Sku> builder)
    {
        base.Configure(builder);

        builder.ToTable("Skus");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.SourceId)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.CollectionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.CollectionConfigId)
            .IsRequired();
    }
}