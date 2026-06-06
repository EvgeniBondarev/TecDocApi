using Microsoft.Extensions.Caching.Memory;

namespace TecDocApi.Application.Caching;

/// <summary>
/// Кэш для неизменяемых данных TecDoc (БД и индексы не обновляются).
/// </summary>
public static class StaticDataCache
{
    /// <summary>
    /// Длительность HTTP ResponseCache — 365 дней.
    /// </summary>
    public const int ResponseCacheDurationSeconds = 365 * 24 * 60 * 60;

    public const int DefaultEntrySize = 1;

    public static MemoryCacheEntryOptions CreateEntryOptions(int size = DefaultEntrySize) =>
        new()
        {
            Priority = CacheItemPriority.NeverRemove,
            Size = size
        };

    public static void Set<T>(IMemoryCache cache, string key, T value, int size = DefaultEntrySize) =>
        cache.Set(key, value, CreateEntryOptions(size));
}
