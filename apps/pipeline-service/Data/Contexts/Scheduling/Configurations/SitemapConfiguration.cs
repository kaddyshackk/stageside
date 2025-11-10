using ComedyPull.Data.Models;
using ComedyPull.Domain.Scheduling.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComedyPull.Data.Contexts.Scheduling.Configurations
{
    public class SitemapConfiguration : BaseEntityConfiguration<Sitemap>
    {
        public override void Configure(EntityTypeBuilder<Sitemap> builder)
        {
            base.Configure(builder);

            builder.ToTable("JobSitemaps");

            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.JobId)
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