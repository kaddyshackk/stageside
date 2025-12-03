using Microsoft.EntityFrameworkCore.Storage;
using StageSide.Data.Database;
using StageSide.SpaCollector.Domain.Database;
using StageSide.SpaCollector.Domain.Models;

namespace StageSide.SpaCollector.Data.Database.SpaCollecting;

public class SpaCollectingDbContextSession(SpaCollectingDbContext context) : ISpaCollectingDbContextSession
{
    private IRepository<Sitemap>? _sitemaps;
    private IRepository<SpaConfig>? _spaConfigs;
    private IDbContextTransaction? _transaction;

    public IRepository<Sitemap> Sitemaps => _sitemaps ??= new Repository<Sitemap>(context);
    public IRepository<SpaConfig> SpaConfigs => _spaConfigs ??= new Repository<SpaConfig>(context);

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
