using TecDocApi.Domain.Entities;
using TecDocApi.Domain.Interfaces;
using TecDocApi.Infrastructure.Data;

namespace TecDocApi.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(Repository<>).MakeGenericType(type);
            var repositoryInstance = Activator.CreateInstance(repositoryType, _context);
            _repositories[type] = repositoryInstance!;
        }
        
        return (IRepository<T>)_repositories[type];
    }

    // Методы записи удалены - доступ только для чтения

    public void Dispose()
    {
        _context.Dispose();
    }
}

