using KrtBank.Domain.Repositories;
using KrtBank.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace KrtBank.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly KrtBankDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(KrtBankDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellation) => await _dbSet.FindAsync(new object[] { id }, cancellation);

        public virtual async Task<T?> GetFirstAsync(Expression<Func<T, bool>> filter, CancellationToken cancellation) => await _dbSet.FirstOrDefaultAsync(filter, cancellation);

        public virtual async Task<IEnumerable<T>> ListAsync(Expression<Func<T, bool>>? filter, CancellationToken cancellation)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            return await query.ToListAsync(cancellation);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter, CancellationToken cancellation) => await _dbSet.AnyAsync(filter, cancellation);

        public async Task<int> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellation) => await _dbSet.CountAsync(filter, cancellation);

        public async Task AddAsync(T item, CancellationToken cancellation) => await _dbSet.AddAsync(item, cancellation);

        public void Update(T item) => _dbSet.Update(item);

        public void Delete(T item) => _dbSet.Remove(item);

        public async Task<int> SaveChangesAsync(CancellationToken cancellation) => await _context.SaveChangesAsync(cancellation);
    }
}
