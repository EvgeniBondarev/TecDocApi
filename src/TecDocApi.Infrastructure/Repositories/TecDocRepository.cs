using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TecDocApi.Domain.Interfaces;
using TecDocApi.Infrastructure.Data;

namespace TecDocApi.Infrastructure.Repositories;

public class TecDocRepository<T> : ITecDocRepository<T> where T : class
{
    protected readonly DbSet<T> DbSet;

    public TecDocRepository(TecDocContext context)
    {
        DbSet = context.Set<T>();
    }

    public virtual IQueryable<T> GetAllAsNoTracking()
    {
        return DbSet.AsNoTracking();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await DbSet.AsNoTracking().CountAsync(cancellationToken);
        
        return await DbSet.AsNoTracking().CountAsync(predicate, cancellationToken);
    }
}

