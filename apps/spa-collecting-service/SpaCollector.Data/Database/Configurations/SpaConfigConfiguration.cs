using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.SpaCollector.Domain.Models;

namespace StageSide.SpaCollector.Data.Database.Configurations;

public class SpaConfigConfiguration : BaseEntityConfiguration<SpaConfig>
{
    public override void Configure(EntityTypeBuilder<SpaConfig> builder)
    {
        base.Configure(builder);

        builder.ToTable("SpaConfigs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SkuId);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(255);

        builder.Property(x => x.MaxConcurrency)
            .IsRequired();
    }
}