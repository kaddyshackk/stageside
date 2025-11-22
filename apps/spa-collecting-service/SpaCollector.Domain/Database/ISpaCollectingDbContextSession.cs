using StageSide.Data.Database;
using StageSide.SpaCollector.Domain.Models;

namespace StageSide.SpaCollector.Domain.Database;

public interface ISpaCollectingDbContextSession : IContextSession, IDisposable
{
    IRepository<Sitemap> Sitemaps { get; }
    IRepository<SpaConfig> SpaConfigs { get; }
}