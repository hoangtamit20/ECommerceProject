using System.Linq.Expressions;
using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Database.Repositories
{
    internal class RepositoryBase<TContext> : IRepositoryBase
        where TContext : DbContext
    {
        private readonly TContext _context;

        public RepositoryBase(TContext context)
        {
            _context = context;
        }


        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return transaction;
        }

        #region Query
        public IQueryable<T> GetSet<T>(Expression<Func<T, bool>>? predicate = null) where T : class
        {
            if (predicate == null)
            {
                return _context.Set<T>();
            }
            return _context.Set<T>().Where(predicate);
        }

        public IQueryable<T> GetSetAsTracking<T>(Expression<Func<T, bool>>? predicate = null) where T : class
        {
            if (predicate == null)
            {
                return _context.Set<T>().AsTracking();
            }
            return _context.Set<T>().Where(predicate).AsTracking();
        }

        #endregion

        public async Task<T> AddAsync<T>(T entity, bool clearTracker = false, CancellationToken cancellationToken = default)
            where T : class
        {
            // InitBaseEntity(entity);
            var entry = await _context.AddAsync(entity, cancellationToken);
            await SaveChangesAsync(clearTracker, cancellationToken);
            // await RefreshCacheTicks<T>();
            return entry.Entity;
        }

        public async Task<int> AddRangeAsync<T>(IEnumerable<T> entities, bool clearTracker = false,
            CancellationToken cancellationToken = default) where T : class
        {
            // foreach (var entity in entities)
            // {
            //     InitBaseEntity(entity);
            // }
            await _context.Set<T>().AddRangeAsync(entities, cancellationToken);
            var result = await SaveChangesAsync(clearTracker, cancellationToken);
            // await RefreshCacheTicks<T>();
            return result;
        }

        public async Task<T?> FindForUpdateAsync<T>(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default) where T : class
        {
            return await _context.Set<T>().AsTracking().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<T?> FindAsync<T>(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default) where T : class
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<List<T>> GetAsync<T>(Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where T : class
        {
            if (predicate == null)
            {
                return await _context.Set<T>().ToListAsync(cancellationToken);
            }
            return await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<List<R>> GetAsync<T, R>(Expression<Func<T, R>> selector, Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where T : class
        {
            if (predicate == null)
            {
                return await _context.Set<T>().Select(selector).ToListAsync(cancellationToken);
            }
            return await _context.Set<T>().Where(predicate).Select(selector).ToListAsync(cancellationToken);
        }

        // public async Task<TableInfo<U>> GetWithPagingAsync<T, U>(TableQueryableModel<T> model, Func<T, Task<U>> converter)
        //     where T : class
        // {
        //     IQueryable<T> query = model.Query;
        //     Pager pager = model.Pager;
        //     TableInfo<U> result = new TableInfo<U>();
        //     var allCount = await query.CountAsync();
        //     if (allCount == 0)
        //     {
        //         result.PageCount = 1;
        //     }
        //     else
        //     {
        //         result.PageCount = allCount % pager.Size == 0
        //             ? (allCount / pager.Size)
        //             : (allCount / pager.Size) + 1;
        //     }
        //     int skipCount = (pager.Index - 1) * pager.Size;
        //     result.TotalItemsCount = allCount;
        //     var list = await (skipCount == 0
        //         ? query.Take(pager.Size)
        //         : query.Skip(skipCount).Take(pager.Size)).ToListAsync();
        //     List<U> data = new List<U>();
        //     foreach (var item in list)
        //     {
        //         data.Add(await converter(item));
        //     }
        //     result.Items = data;
        //     return result;
        // }

        public async Task<int> DeleteAsync<T>(T entity, bool clearTracker = false, CancellationToken cancellationToken = default)
            where T : class
        {
            // CheckSimulated();
            // RuntimeContext.Logger.LogDebug($"Delete entity {typeof(T)}.");
            _context.Set<T>().Remove(entity);
            var result = await SaveChangesAsync(clearTracker, cancellationToken);
            // await RefreshCacheTicks<T>(true);
            return result;
        }

        public async Task<int> DeleteRangeAsync<T>(IEnumerable<T> entities, bool clearTracker = false,
            CancellationToken cancellationToken = default) where T : class
        {
            // CheckSimulated();
            // RuntimeContext.Logger.LogDebug($"Delete entity range {typeof(T)}.");
            _context.Set<T>().RemoveRange(entities);
            var result = await SaveChangesAsync(clearTracker, cancellationToken);
            // await RefreshCacheTicks<T>(true);
            return result;
        }


        public async Task<T> UpdateAsync<T>(T entity, bool clearTracker = false, CancellationToken cancellationToken = default) where T : class
        {
            _context.Set<T>().Update(entity: entity);
            await SaveChangesAsync(clearTracker, cancellationToken);
            return entity;
        }


        public async Task<int> SaveChangesAsync(bool clearTracker = false, CancellationToken cancellationToken = default)
        {
            // CheckSimulated();
            var result = await _context.SaveChangesAsync(cancellationToken);
            if (clearTracker)
            {
                _context.ChangeTracker.Clear();
            }
            return result;
        }

        // private void CheckSimulated()
        // {
        //     if (RuntimeContext.IsSimulated)
        //     {
        //         throw new SimulationNoAuthException();
        //     }
        // }
    }
}