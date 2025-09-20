using ConsoleApp1;
using Microsoft.Extensions.Caching.Memory;
using TreeGrouping.Application.DbService;
using TreeGrouping.Application.DbService.Models;

namespace TreeGrouping.Application.CategoryService;

public class CategoryCacheService
{
    private readonly IDatabaseService _dbService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public CategoryCacheService(IDatabaseService dbService, IMemoryCache cache)
    {
        _dbService = dbService;
        _cache = cache;
    }

    public async Task<List<CategoryModel>> GetCategoriesFromCache(string cacheKey, StoredProcedureType storedProcedureType)
    {
        if (!_cache.TryGetValue(cacheKey, out List<CategoryModel> categories))
        {
            categories = (await _dbService.ExecuteStoredProcedureAsync(storedProcedureType)).ToList();
            _cache.Set(cacheKey, categories, _cacheDuration);
        }
        return categories;
    }
    public async Task<List<CategoryModel>> UpdateCache(string cacheKey, StoredProcedureType storedProcedureType)
    {
        var updatedCategories = (await _dbService.ExecuteStoredProcedureAsync(storedProcedureType)).ToList();
        _cache.Set(cacheKey, updatedCategories, _cacheDuration);
        return updatedCategories;
    }
}
