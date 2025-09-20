using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.TradesoftApi.Models;
using Servcies.ApiServcies.TradesoftApi.Models.Response;
using PreOrderContainer = Servcies.ApiServcies.TradesoftApi.Models.PreOrderContainer;

namespace Servcies.ApiServcies.TradesoftApi;

public class TradesoftDataManager : IApiDataManager<TradesoftDataManager>
{
    private TradesoftApiClient _apiClient;
    private TradesoftApiConfig _apiConfig;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public TradesoftDataManager(TradesoftApiConfig config, IMemoryCache cache)
    {
        _apiConfig = config;
        _apiClient = new TradesoftApiClient();
        _cache = cache;
        _cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetPriority(CacheItemPriority.Normal);
    }

    public TradesoftDataManager SetClient(string user, string password)
    {
        _apiClient = new TradesoftApiClient();
        return this;
    }
    
    public async Task<ProviderListResponse> GetProviderList()
    {
        const string cacheKey = "Tradesoft_ProviderList";
        if (_cache.TryGetValue(cacheKey, out ProviderListResponse cachedResponse))
        {
            return cachedResponse;
        }
        var request = new ProviderListRequest
        {
            User = _apiConfig.User,
            Password = _apiConfig.Password,
        };
        
        JObject response = _apiClient.MakeRequestPost(request, TradesoftApiUrl.BASE_URL);
        var result = response.ToObject<ProviderListResponse>();
        _cache.Set(cacheKey, result, _cacheOptions);
        
        return result;
    }
    
    public async Task<List<ProviderInfo>> GetActiveProviders()
    {
        const string cacheKey = "Tradesoft_ActiveProviders";
        if (_cache.TryGetValue(cacheKey, out List<ProviderInfo> cachedProviders))
        {
            return cachedProviders;
        }

        var response = await GetProviderList();
        var activeProviders = response.Data.Where(p => p.Active).ToList();
        _cache.Set(cacheKey, activeProviders, _cacheOptions);
        
        return activeProviders;
    }
    
    public async Task<PreOrderSearchResponse> GetPriceList(PreOrderContainer preOrderContainer)
    {
        if (preOrderContainer == null)
        {
            throw new ArgumentNullException(nameof(preOrderContainer));
        }

        if (string.IsNullOrEmpty(_apiConfig.User) || string.IsNullOrEmpty(_apiConfig.Password))
        {
            throw new InvalidOperationException("API credentials are not configured");
        }

        var request = new PreOrderSearchRequest
        {
            User = _apiConfig.User,
            Password = _apiConfig.Password,
            Container = new List<PreOrderContainer> { preOrderContainer }
        };

        JObject response = _apiClient.MakeRequestPost(request, TradesoftApiUrl.BASE_URL);
        return response?.ToObject<PreOrderSearchResponse>() ?? throw new Exception("Failed to deserialize response");
    }
    
    public async Task<MakeOrderOfflineResponse> MakeOrderOfflineAsync(MakeOrderOfflineParam makeOrderOfflineParam)
    {
        var request = new MakeOrderOfflineRequest
        {
            User = _apiConfig.User,
            Password = _apiConfig.Password,
            Param = new List<MakeOrderOfflineParam>
            {
                makeOrderOfflineParam
            }
        };
        JObject response = _apiClient.MakeRequestPost(request, TradesoftApiUrl.BASE_URL);
        return response.ToObject<MakeOrderOfflineResponse>();
    }
    
    public async Task<GetItemsStatusResponse> GetItemsStatusAsync(GetItemsStatusContainer  getItemsStatusContainer)
    {
        var request = new GetItemsStatusRequest
        {
            User = _apiConfig.User,
            Password = _apiConfig.Password,
            TimeLimit = 10,
            Container = new List<GetItemsStatusContainer>
            {
                getItemsStatusContainer
            }
        };
        
        var response = await _apiClient.MakeRequestPostAsync(request, TradesoftApiUrl.BASE_URL);
        var result = JObject.Parse(response);
        return result.ToObject<GetItemsStatusResponse>();
    }



    public async Task<bool> GetTestRequest(string user, string password)
    {
        return await _apiClient.GetTestRequest(user, password);
    }
}