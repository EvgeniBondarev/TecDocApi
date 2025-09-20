using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.AbcpApi.Models.Request;

namespace Servcies.ApiServcies.AbcpApi;

public class AbcpDataManager : IApiDataManager<AbcpDataManager>
{
    private AbcpApiClient _apiClient;
    private AbcpApiConfig _apiConfig;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private Dictionary<string, string>? _distributorsDict { get; set; }

    public AbcpDataManager(AbcpApiConfig config, IMemoryCache cache)
    {
        _apiConfig = config;
        _apiClient = new AbcpApiClient();
        _cache = cache;
        _cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetPriority(CacheItemPriority.Normal);
    }

    public AbcpDataManager SetClient(string userLogin, string userPsw)
    {
        _apiClient = new AbcpApiClient();
        _apiConfig.UserLogin = userLogin;
        _apiConfig.UserPsw = userPsw;
        return this;
    }
    
    public async Task<List<ArticleResponse>> SearchArticles(List<SearchItem> searchItems)
    {
        var cacheKey = $"Abcp_SearchArticles_{string.Join("_", searchItems.Select(s => $"{s.Brand}_{s.Number}"))}";

        if (_cache.TryGetValue(cacheKey, out List<ArticleResponse> cachedResponse))
        {
            return cachedResponse;
        }

        var request = new SearchArticlesRequest
        {
            UserLogin = _apiConfig.UserLogin,
            UserPsw = _apiConfig.UserPsw,
            Search = searchItems
        };

        var url = AbcpApiUrl.SearchBatchUrl(_apiConfig.Domain); // <--- Убедись, что URL указывает на /search/batch
        JObject response = _apiClient.MakeRequestPost(request, url);

        var result = response["data"]?.ToObject<List<ArticleResponse>>() ?? new List<ArticleResponse>();
        _cache.Set(cacheKey, result, _cacheOptions);

        return result;
    }
    
    public async Task<Dictionary<string, string>> GetDistributorsDictionary()
    {
        if (_distributorsDict != null)
            return _distributorsDict;
        
        var distributors = await _apiClient.GetDistributorsShortInfo(
            _apiConfig.UserLogin, 
            _apiConfig.UserPsw, 
            _apiConfig.Domain);

        _distributorsDict = distributors.ToDictionary(d => d.Id, d => d.Name);
        return _distributorsDict;
    }

    public async Task<bool> GetTestRequest(string userLogin, string userPsw)
    {
        return await _apiClient.GetTestRequest(userLogin, userPsw);
    }
}