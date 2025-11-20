using Microsoft.EntityFrameworkCore.Storage;
using StageSide.Data.Database;
using StageSide.SpaCollector.Domain.Collection.Models;
using StageSide.SpaCollector.Domain.Database;

namespace StageSide.SpaCollector.Data.Database;

public class SpaCollectingDbContextSession(SpaCollectingDbContext context) : ISpaCollectingDbContextSession
{
    private IRepository<Sitemap>? _sitemaps;
    private IRepository<CollectionConfig>? _collectionConfigs;
    private IDbContextTransaction? _transaction;

    public IRepository<Sitemap> Sitemaps => _sitemaps ??= new Repository<Sitemap>(context);
    public IRepository<CollectionConfig> CollectionConfigs => _collectionConfigs ??= new Repository<CollectionConfig>(context);

    public async Task<int> SaveChangesAsync(CancellationToken stoppingToken)
    {
        return await context.SaveChangesAsync(stoppingToken);
    }

    public async Task BeginTransactionAsync(CancellationToken stoppingToken)
    {
        _transaction = await context.Database.BeginTransactionAsync(stoppingToken);
    }

    public async Task CommitTransactionAsync(CancellationToken stoppingToken)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Transaction has not been initiated");
        await _transaction.CommitAsync(stoppingToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken stoppingToken)
    {
        if (_transaction == null)
            throw new InvalidOperationException("Transaction has not been initiated");
        await _transaction.RollbackAsync(stoppingToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}