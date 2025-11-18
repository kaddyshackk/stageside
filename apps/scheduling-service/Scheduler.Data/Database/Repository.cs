using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StageSide.Data.ContextSession;

namespace StageSide.Scheduler.Data.Database
{
    public class Repository<T>(DbContext context) : IRepository<T>
        where T : class
    {
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken stoppingToken)
        {
            return await _dbSet.FindAsync(id, stoppingToken);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken stoppingToken)
        {
            return await _dbSet.ToListAsync(stoppingToken);
        }

        public async Task AddAsync(T entity, CancellationToken stoppingToken)
        {
            await _dbSet.AddAsync(entity, stoppingToken);
        }
        
        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken stoppingToken)
        {
            await _dbSet.AddRangeAsync(entities, stoppingToken);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        
        public void UpdateRange(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<ICollection<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken stoppingToken)
        {
            return await _dbSet.Where(predicate).ToListAsync(stoppingToken);
        }

        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }
}