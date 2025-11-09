using ComedyPull.Data.Services;
using ComedyPull.Domain.Interfaces.Data;
using ComedyPull.Domain.Jobs;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComedyPull.Data.Contexts.PipelineDb
{
    public class PipelineDataSession(PipelineDbContext context) : IPipelineDataSession
    {
        private IRepository<Job>? _jobs;
        private IRepository<Execution>? _executions;
        private IRepository<Sitemap>? _sitemaps;
        private IDbContextTransaction? _transaction;

        public IRepository<Job> Jobs => _jobs ??= new Repository<Job>(context);
        public IRepository<Execution> Executions => _executions ??= new Repository<Execution>(context);
        public IRepository<Sitemap> Sitemaps => _sitemaps ??= new Repository<Sitemap>(context);
        
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
}