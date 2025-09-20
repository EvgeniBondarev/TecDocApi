using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonRepositories.Context;
using Servcies.CacheServcies;

namespace Services.CacheServcies.Cache.EtProducerCache
{
    public class EtProducerCache : IRedisAppCache<EtProducer>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly JcEtalonContext _jcEtalonContext;
        private const string CacheKey = "EtProducers";
        private const int InitialLoadCount = 500;
        private const int IncrementCount = 100;
        private int _maxPage = 1;
        private int _maxItemsToLoad = InitialLoadCount;

        public EtProducerCache(IMemoryCache memoryCache, JcEtalonContext jcEtalonContext)
        {
            _memoryCache = memoryCache;
            _jcEtalonContext = jcEtalonContext;
        }

        private void SetCache(string key, object value, TimeSpan expiration)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration);
            _memoryCache.Set(key, value, cacheEntryOptions);
        }

        private T GetCache<T>(string key)
        {
            return _memoryCache.TryGetValue(key, out T value) ? value : default;
        }

        public async Task<List<EtProducer>> Get(int page)
        {
            var cachedEtProducers = GetCache<List<EtProducer>>(CacheKey);
            if (cachedEtProducers == null)
            {
                cachedEtProducers = (await _jcEtalonContext.EtProducers.Take(InitialLoadCount).ToListAsync())
                    .ToList();
                _maxPage = 10;
                SetCache(CacheKey, cachedEtProducers, TimeSpan.FromMinutes(10));
            }

            int requiredItemCount = page * IncrementCount;
            if (cachedEtProducers.Count < requiredItemCount)
            {
                int itemsToLoad = requiredItemCount - cachedEtProducers.Count;
                var additionalEtProducers = await _jcEtalonContext.EtProducers
                    .Skip(cachedEtProducers.Count)
                    .Take(itemsToLoad)
                    .ToListAsync();

                if (additionalEtProducers.Any())
                {
                    cachedEtProducers.AddRange(additionalEtProducers);
                    SetCache(CacheKey, cachedEtProducers, TimeSpan.FromMinutes(10));
                }

                _maxPage = Math.Max(_maxPage, page);
                _maxItemsToLoad = itemsToLoad;
            }

            return cachedEtProducers
                .Take(requiredItemCount)
                .OrderBy(e => e.Name)
                .ToList();
        }

        public async Task<List<EtProducer>> Get()
        {
            var cachedEtProducers = GetCache<List<EtProducer>>(CacheKey);
            if (cachedEtProducers != null)
                return cachedEtProducers.OrderBy(e => e.Name).ToList();

            await Update();
            cachedEtProducers = GetCache<List<EtProducer>>(CacheKey);

            if (cachedEtProducers != null)
                return cachedEtProducers.OrderBy(e => e.Name).ToList();

            throw new InvalidOperationException("Данные не найдены в кэше после обновления.");
        }

        public async Task Update()
        {
            var producers = await _jcEtalonContext.EtProducers
                .Take(_maxItemsToLoad)
                .ToListAsync();
            SetCache(CacheKey, producers, TimeSpan.FromMinutes(10));
        }

        public async Task UpdateCacheIncrementally(int producerId)
        {
            var updatedProducer = await _jcEtalonContext.EtProducers
                .FirstOrDefaultAsync(p => p.Id == producerId);
            if (updatedProducer == null) return;

            var cachedEtProducers = GetCache<List<EtProducer>>(CacheKey) ?? new List<EtProducer>();
            var existingProducerIndex = cachedEtProducers.FindIndex(p => p.Id == producerId);

            if (existingProducerIndex != -1)
            {
                cachedEtProducers[existingProducerIndex] = updatedProducer;
            }
            else
            {
                cachedEtProducers.Add(updatedProducer);
            }

            SetCache(CacheKey, cachedEtProducers, TimeSpan.FromMinutes(10));
        }

        public async Task RemoveProducerFromCache(int producerId)
        {
            var cachedEtProducers = GetCache<List<EtProducer>>(CacheKey);
            if (cachedEtProducers == null) return;

            cachedEtProducers.RemoveAll(p => p.Id == producerId);
            SetCache(CacheKey, cachedEtProducers, TimeSpan.FromMinutes(10));
        }

        public async Task RemoveProducersFromCache(int[] producerIds)
        {
            var cachedEtProducers = GetCache<List<EtProducer>>(CacheKey);
            if (cachedEtProducers == null) return;

            cachedEtProducers.RemoveAll(p => producerIds.Contains(p.Id));
            SetCache(CacheKey, cachedEtProducers, TimeSpan.FromMinutes(10));
        }
    }
}