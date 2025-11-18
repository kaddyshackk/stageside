using StageSide.Data.ContextSession;
using StageSide.SpaCollector.Domain.Collection;

namespace StageSide.SpaCollector.Domain.Database;

public interface ISpaCollectingDbContextSession : IContextSession, IDisposable
{
    IRepository<Sitemap> Sitemaps { get; }
    IRepository<CollectionConfig> CollectionConfigs { get; }
}