using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TecDocApi.Domain.Interfaces;
using TecDocApi.Infrastructure.Data;

namespace TecDocApi.Infrastructure.Repositories;

public class TecDocRepository<T> : ITecDocRepository<T> where T : class
{
    protected readonly TecDocContext _context;
    protected readonly DbSet<T> _dbSet;

    public TecDocRepository(TecDocContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual IQueryable<T> GetAllAsNoTracking()
    {
        return _dbSet.AsNoTracking();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _dbSet.AsNoTracking().CountAsync(cancellationToken);
        
        return await _dbSet.AsNoTracking().CountAsync(predicate, cancellationToken);
    }
}

