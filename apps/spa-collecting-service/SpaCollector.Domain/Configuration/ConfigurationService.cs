using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StageSide.SpaCollector.Domain.Database;
using StageSide.SpaCollector.Domain.Models;
using StageSide.SpaCollector.Domain.Operations.CreateConfiguration;

namespace StageSide.SpaCollector.Domain.Configuration;

public class ConfigurationService(ISpaCollectingDbContextSession session, ILogger<ConfigurationService> logger)
{
    public async Task<SpaConfig?> CreateCollectionConfigAsync(CreateSpaConfigCommand command,
        CancellationToken ct)
    {
        try
        {
            var existing = await session.SpaConfigs.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SkuId == command.SkuId, ct);
            if (existing != null) throw new ArgumentException($"Collection configuration for sku {existing.SkuId} already exists");
            
            var config = new SpaConfig
            {
                SkuId = command.SkuId,
                MaxConcurrency = command.MaxConcurrency,
                UserAgent = command.UserAgent,
                CreatedBy = "System",
                UpdatedBy = "System"
            };
            await session.SpaConfigs.AddAsync(config, ct);

            var sitemaps = command.Sitemaps.Select(x => new Sitemap
            {
	            SpaConfigId = config.Id,
	            Url = x.Url,
	            RegexFilter = x.RegexFilter,
	            CreatedBy = "System",
	            UpdatedBy = "System"
            });
            await session.Sitemaps.AddRangeAsync(sitemaps, ct);
            
            await session.SaveChangesAsync(ct);

            return config;
        }
        catch (Exception ex)
        {
            session.Dispose();
            logger.LogError(ex, "Failed to create collection configuration.");
            throw;
        }
    }
}
