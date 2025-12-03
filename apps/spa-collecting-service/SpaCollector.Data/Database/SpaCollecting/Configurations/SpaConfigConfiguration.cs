using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.SpaCollector.Domain.Models;

namespace StageSide.SpaCollector.Data.Database.SpaCollecting.Configurations;

public class SpaConfigConfiguration : BaseEntityConfiguration<SpaConfig>
{
    public override void Configure(EntityTypeBuilder<SpaConfig> builder)
    {
        base.Configure(builder);

        builder.ToTable("spa_configs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
	        .HasColumnName("id");

        builder.Property(x => x.SkuId)
	        .HasColumnName("sku_id");

        builder.Property(x => x.UserAgent)
	        .HasColumnName("user_agent")
            .HasMaxLength(255);

        builder.Property(x => x.MaxConcurrency)
	        .HasColumnName("max_concurrency")
            .IsRequired();
        
        builder.HasIndex(x => x.SkuId)
	        .IsUnique()
	        .HasDatabaseName("ix_spa_configs_sku_id");
    }
}
