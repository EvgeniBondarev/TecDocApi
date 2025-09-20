using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OzonDomains.Models;
using Servcies.DataServcies;

namespace Servcies.CacheServcies.Cache.OzonOrdersCache
{
    public class TransactionCache : IRedisAppCache<Transaction>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly TransactionDataServcies _transactionsService;
        private const string CacheKey = "Transactions";
        private const int InitialLoadCount = 100;
        private const int IncrementCount = 30;
        private int _maxPage = 1;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
        private int _maxItemsToLoad = InitialLoadCount;

        public TransactionCache(IMemoryCache memoryCache, TransactionDataServcies transactionsService)
        {
            _memoryCache = memoryCache;
            _transactionsService = transactionsService;
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

        public async Task<List<Transaction>> Get(int page)
        {
            var cachedTransactions = GetCache<List<Transaction>>(CacheKey);
            if (cachedTransactions == null)
            {
                cachedTransactions = _transactionsService.GetTransactions(0, InitialLoadCount).ToList();
                _maxPage = InitialLoadCount / IncrementCount;
                SetCache(CacheKey, cachedTransactions, _cacheDuration);
            }

            int requiredItemCount = page * IncrementCount;
            if (cachedTransactions.Count < requiredItemCount)
            {
                int itemsToLoad = requiredItemCount - cachedTransactions.Count;
                List<Transaction> additionalTransactions = _transactionsService.GetTransactions(cachedTransactions.Count, itemsToLoad).ToList();
                if (additionalTransactions.Any())
                {
                    cachedTransactions.AddRange(additionalTransactions);
                    SetCache(CacheKey, cachedTransactions, _cacheDuration);
                }
                _maxPage = Math.Max(_maxPage, page);
                _maxItemsToLoad = itemsToLoad;
            }

            return cachedTransactions.Take(requiredItemCount).ToList();
        }

        public async Task<List<Transaction>> Get()
        {
            var transactions = GetCache<List<Transaction>>(CacheKey);
            if (transactions != null)
            {
                return transactions.ToList();
            }

            throw new NotImplementedException("Данные не найдены в кэше");
        }

        public async Task<List<Transaction>> Set()
        {
            List<Transaction> newTransactions = _transactionsService.GetTransactions().ToList();
            SetCache(CacheKey, newTransactions, _cacheDuration);
            return newTransactions;
        }

        public async Task UpdateCacheIncrementally(int transactionId)
        {
            Transaction updatedTransaction = await _transactionsService.GetTransaction(transactionId);

            if (updatedTransaction != null)
            {
                var cachedTransactions = GetCache<List<Transaction>>(CacheKey);
                if (cachedTransactions != null)
                {
                    var existingTransactionIndex = cachedTransactions.FindIndex(tr => tr.Id == transactionId);

                    if (existingTransactionIndex != -1)
                    {
                        cachedTransactions[existingTransactionIndex] = updatedTransaction;
                    }
                    else
                    {
                        cachedTransactions.Add(updatedTransaction);
                    }

                    SetCache(CacheKey, cachedTransactions, _cacheDuration);
                }
            }
        }

        public async Task Update()
        {
            List<Transaction> cachedOrders = _transactionsService.GetTransactions(0, _maxItemsToLoad).ToList();
            SetCache(CacheKey, cachedOrders, _cacheDuration);
        }
    }
}