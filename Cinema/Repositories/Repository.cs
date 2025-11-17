using Cinema.Data;
using Cinema.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cinema.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool tracking = false,
            CancellationToken cancellationToken = default,
            object? includes = null) // ← التعديل
        {
            IQueryable<T> query = _dbSet;
            if (!tracking) query = query.AsNoTracking();
            if (filter != null) query = query.Where(filter);

            // معالجة includes لو كانت Expression<Func<T, object>> واحدة أو List
            if (includes != null)
            {
                if (includes is Expression<Func<T, object>> singleInclude)
                {
                    query = query.Include(singleInclude);
                }
                else if (includes is IEnumerable<Expression<Func<T, object>>> multipleIncludes)
                {
                    foreach (var include in multipleIncludes)
                        query = query.Include(include);
                }
            }

            // fallback على includeProperties القديم لو حابب تستخدمه
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(prop.Trim());
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool tracking = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            if (!tracking) query = query.AsNoTracking();
            if (filter != null) query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(prop.Trim());
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool tracking = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;
            if (!tracking) query = query.AsNoTracking();
            if (filter != null) query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var prop in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(prop.Trim());
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAllEntitiesAsync(CancellationToken cancellationToken = default)
        {
            var entities = await _dbSet.ToListAsync(cancellationToken);
            _dbSet.RemoveRange(entities);
        }
    }
}
