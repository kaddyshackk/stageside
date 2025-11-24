using System.Linq.Expressions;

namespace StageSide.Data.Database
{
    public interface IRepository<T> where T : class
    {
        public Task<T?> GetByIdAsync(Guid id, CancellationToken ct);
        public Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
        public Task AddAsync(T entity, CancellationToken ct);
        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct);
        public void Update(T entity);
        public void UpdateRange(IEnumerable<T> entities);
        public void Delete(T entity);
        public Task<ICollection<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
        public IQueryable<T> Query();
    }
}