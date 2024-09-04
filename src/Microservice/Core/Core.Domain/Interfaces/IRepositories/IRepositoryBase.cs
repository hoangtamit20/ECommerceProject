using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Domain
{
    public interface IRepositoryBase
    {
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        IQueryable<T> GetSetAsTracking<T>(Expression<Func<T, bool>>? predicate = null) where T : class;
        IQueryable<T> GetSet<T>(Expression<Func<T, bool>>? predicate = null) where T : class;
        Task<T> UpdateAsync<T>(T entity, bool clearTracker = false, CancellationToken cancellationToken = default) where T : class;
        Task<T?> FindForUpdateAsync<T>(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default) where T : class;
        Task<T?> FindAsync<T>(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default) where T : class;
        Task<T> AddAsync<T>(T entity, bool clearTracker = false, CancellationToken cancellationToken = default) where T : class;
        Task<int> AddRangeAsync<T>(IEnumerable<T> entities, bool clearTracker = false,
            CancellationToken cancellationToken = default) where T : class;
        Task<int> DeleteRangeAsync<T>(IEnumerable<T> entities, bool clearTracker = false,
            CancellationToken cancellationToken = default) where T : class;
        Task<int> DeleteAsync<T>(T entity, bool clearTracker = false, CancellationToken cancellationToken = default)
            where T : class;
        Task<List<R>> GetAsync<T, R>(Expression<Func<T, R>> selector, Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where T : class;
        Task<List<T>> GetAsync<T>(Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where T : class;
        Task<int> SaveChangesAsync(bool clearTracker = false, CancellationToken cancellationToken = default);
        
    }
}