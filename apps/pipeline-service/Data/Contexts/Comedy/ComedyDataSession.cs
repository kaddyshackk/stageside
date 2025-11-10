using ComedyPull.Data.Services;
using ComedyPull.Domain.Interfaces;
using ComedyPull.Domain.Models;
using ComedyPull.Domain.Pipeline.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ComedyPull.Data.Contexts.Comedy
{
    public class ComedyDataSession(ComedyDbContext context) : IComedyDataSession
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