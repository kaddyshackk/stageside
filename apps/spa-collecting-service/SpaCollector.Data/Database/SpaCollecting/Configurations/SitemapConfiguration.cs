using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.SpaCollector.Domain.Models;

namespace StageSide.SpaCollector.Data.Database.SpaCollecting.Configurations
{
    public class SitemapConfiguration : BaseEntityConfiguration<Sitemap>
    {
        public override void Configure(EntityTypeBuilder<Sitemap> builder)
        {
            base.Configure(builder);

            builder.ToTable("Sitemaps");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
	            .HasColumnName("id");
            
            builder.Property(x => x.SkuId)
	            .HasColumnName("sku_id")
                .IsRequired();
            
            builder.Property(x => x.Url)
	            .HasColumnName("url")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.RegexFilter)
	            .HasColumnName("regex_filter")
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
	            .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();
        }
    }
}
