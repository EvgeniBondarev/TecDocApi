namespace Servcies.CacheServcies;

public interface IRedisAppCache<T>
{
    Task Update();
}