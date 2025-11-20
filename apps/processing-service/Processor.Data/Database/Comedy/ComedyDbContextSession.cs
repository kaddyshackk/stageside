using Microsoft.EntityFrameworkCore.Storage;
using StageSide.Data.Database;
using StageSide.Domain.Models;
using StageSide.Processor.Domain.Database;

namespace StageSide.Processor.Data.Database.Comedy
{
    public class ComedyDbContextSession(ComedyDbContext context) : IComedyDbContextSession
    {
        private IRepository<Act>? _acts;
        private IRepository<Venue>? _venues;
        private IRepository<Event>? _events;
        private IRepository<EventAct>? _eventActs;
        private IDbContextTransaction? _transaction;
        
        public IRepository<Act> Acts => _acts ??= new Repository<Act>(context);
        public IRepository<Venue> Venues => _venues ??= new Repository<Venue>(context);
        public IRepository<Event> Events => _events ??= new Repository<Event>(context);
        public IRepository<EventAct> EventActs => _eventActs ??= new Repository<EventAct>(context);
        
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