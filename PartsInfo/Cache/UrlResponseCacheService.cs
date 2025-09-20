using Microsoft.Extensions.Caching.Memory;

namespace PartsInfo.Cache;

public class UrlResponseCacheService
{
    private readonly IMemoryCache _cache;

    public UrlResponseCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet(string url, out string content)
    {
        return _cache.TryGetValue(url, out content!);
    }

    public void Set(string url, string content, TimeSpan duration)
    {
        _cache.Set(url, content, duration);
    }
}