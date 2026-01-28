using System.Linq.Expressions;

namespace KrtBank.Domain.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellation);
        Task<T?> GetFirstAsync(Expression<Func<T, bool>> filter, CancellationToken cancellation);
        Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>>? filter, CancellationToken cancellation);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellation);
        Task<int> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellation);

        Task AddAsync(T item, CancellationToken cancellation);
        void Update(T item);
        void Delete(T item);
        Task<int> SaveChangesAsync(CancellationToken cancellation);
    }
}
