using StageSide.Pipeline.Data.Services;
using StageSide.Pipeline.Domain.Scheduling.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using StageSide.Data.ContextSession;
using StageSide.Scheduling.Models;

namespace StageSide.Pipeline.Data.Contexts.Scheduling
{
    public class SchedulingContextSession(SchedulingDbContext context) : ISchedulingContextSession
    {
        private IRepository<Schedule>? _schedules;
        private IRepository<Job>? _jobs;
        private IRepository<Sitemap>? _sitemaps;
        private IDbContextTransaction? _transaction;

        public IRepository<Schedule> Schedules => _schedules ??= new Repository<Schedule>(context);
        public IRepository<Job> Jobs => _jobs ??= new Repository<Job>(context);
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