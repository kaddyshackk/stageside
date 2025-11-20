using StageSide.Data.Database;
using StageSide.SpaCollector.Domain.Collection.Models;

namespace StageSide.SpaCollector.Domain.Database;

public interface ISpaCollectingDbContextSession : IContextSession, IDisposable
{
    IRepository<Sitemap> Sitemaps { get; }
    IRepository<CollectionConfig> CollectionConfigs { get; }
}