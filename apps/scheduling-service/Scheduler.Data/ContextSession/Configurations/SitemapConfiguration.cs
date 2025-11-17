using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StageSide.Data.Configuration;
using StageSide.Scheduling.Models;

namespace StageSide.Scheduler.Data.ContextSession.Configurations
{
    public class SitemapConfiguration : BaseEntityConfiguration<Sitemap>
    {
        public override void Configure(EntityTypeBuilder<Sitemap> builder)
        {
            base.Configure(builder);

            builder.ToTable("Sitemaps");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.ScheduleId)
                .IsRequired();
            
            builder.Property(x => x.Url)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.RegexFilter)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();
        }
    }
}