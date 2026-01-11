using TecDocApi.Domain.Entities;

namespace TecDocApi.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    // Методы записи удалены - доступ только для чтения
}

