namespace ComedyPull.Domain.Interfaces
{
    public interface IDataSession
    {
        public Task<int> SaveChangesAsync(CancellationToken stoppingToken);
        public Task BeginTransactionAsync(CancellationToken stoppingToken);
        public Task CommitTransactionAsync(CancellationToken stoppingToken);
        public Task RollbackTransactionAsync(CancellationToken stoppingToken);
    }
}