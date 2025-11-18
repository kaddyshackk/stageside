using Microsoft.EntityFrameworkCore.Storage;
using StageSide.Data.Database;
using StageSide.Scheduler.Domain.Database;
using StageSide.Scheduler.Domain.Models;

namespace StageSide.Scheduler.Data.Database
{
    public class SchedulingDbContextSession(SchedulingDbContext context) : ISchedulingDbContextSession
    {
        private IRepository<Schedule>? _schedules;
        private IRepository<Job>? _jobs;
        private IRepository<Source>? _source;
        private IRepository<Sku>? _sku;
        private IDbContextTransaction? _transaction;

        public IRepository<Schedule> Schedules => _schedules ??= new Repository<Schedule>(context);
        public IRepository<Job> Jobs => _jobs ??= new Repository<Job>(context);
        public IRepository<Source> Sources => _source ??= new Repository<Source>(context);
        public IRepository<Sku> Skus => _sku ??= new Repository<Sku>(context);
        
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