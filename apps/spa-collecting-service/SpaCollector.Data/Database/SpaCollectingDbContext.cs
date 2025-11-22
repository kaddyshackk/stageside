using Microsoft.EntityFrameworkCore;
using StageSide.SpaCollector.Data.Database.Configurations;
using StageSide.SpaCollector.Domain.Models;

namespace StageSide.SpaCollector.Data.Database;

public class SpaCollectingDbContext(DbContextOptions<SpaCollectingDbContext> options) : DbContext(options)
{
    public DbSet<Sitemap> Sitemaps { get; set; }
    public DbSet<SpaConfig> SpaCollectionConfigs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
            
        modelBuilder.ApplyConfiguration(new SitemapConfiguration());
        modelBuilder.ApplyConfiguration(new SpaConfigConfiguration());
    }
}