using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.SpaCollector.Domain.Collection.Models;

namespace StageSide.SpaCollector.Data.Database.Configurations;

public class CollectionConfigConfiguration : BaseEntityConfiguration<CollectionConfig>
{
    public override void Configure(EntityTypeBuilder<CollectionConfig> builder)
    {
        base.Configure(builder);

        builder.ToTable("CollectionConfigs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SkuId);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(255);

        builder.Property(x => x.MaxConcurrency)
            .IsRequired();
    }
}