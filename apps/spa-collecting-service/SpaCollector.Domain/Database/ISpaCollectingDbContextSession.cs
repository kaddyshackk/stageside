using StageSide.SpaCollector.Domain.Collection;
using StageSide.Data.Database;

namespace StageSide.SpaCollector.Domain.Database;

public interface ISpaCollectingDbContextSession : IContextSession, IDisposable
{
    IRepository<Sitemap> Sitemaps { get; }
    IRepository<CollectionConfig> CollectionConfigs { get; }
}