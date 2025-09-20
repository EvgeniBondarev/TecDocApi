using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.ZzapApi.Models.Request;
using Servcies.ApiServcies.ZzapApi.Models.Response;

namespace Servcies.ApiServcies.ZzapApi;

public class ZzapDataManager : IApiDataManager<ZzapDataManager>
{
    private readonly ZzapApiClient _apiClient;
    private readonly ZzapApiConfig _apiConfig;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private IApiDataManager<ZzapDataManager> _apiDataManagerImplementation;

    public ZzapDataManager(ZzapApiConfig config, IMemoryCache cache)
    {
        _apiConfig = config;
        _apiClient = new ZzapApiClient();
        _cache = cache;
        _cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetPriority(CacheItemPriority.Normal);
    }

    public async Task<SearchResponse> SearchParts(string searchText, string partNumber, string manufacturer, int regionCode)
    {
        var cacheKey = $"Zzap_Search_{searchText}_{partNumber}_{manufacturer}_{regionCode}";
        if (_cache.TryGetValue(cacheKey, out SearchResponse cachedResponse))
        {
            return cachedResponse;
        }

        var request = new SearchRequest
        {
            Login = _apiConfig.Login,
            Password = _apiConfig.Password,
            ApiKey = _apiConfig.ApiKey,
            SearchText = searchText,
            PartNumber = partNumber,
            ManufacturerClass = manufacturer,
            RegionCode = regionCode
        };

        JObject response = await _apiClient.MakeRequestAsync(request, $"{ZzapApiUrl.BASE_URL}GetSearchResultLight");

        var result = JsonConvert.DeserializeObject<SearchResponse>(response.ToString(), new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new SearchResponseConverter() }
        });

        // Проверяем статус и кэшируем успешный ответ
        var statusProperty = response["error"]?.ToString();
        if (string.IsNullOrEmpty(statusProperty))
        {
            _cache.Set(cacheKey, result, _cacheOptions);
            result.Status = "success";
        }
        else
        {
             result.Status = "error";
             result.Message = statusProperty;
        }
        
        return result;
    }
    
    public async Task<GetRegionsResponse> GetRegions()
    {
        const string cacheKey = "Zzap_Regions";

        if (_cache.TryGetValue(cacheKey, out GetRegionsResponse cachedResponse))
            return cachedResponse;

        var request = new GetRegionsRequest
        {
            Login = _apiConfig.Login,
            Password = _apiConfig.Password,
            ApiKey = _apiConfig.ApiKey
        };

        JObject response = await _apiClient.MakeRequestAsync(request, $"{ZzapApiUrl.BASE_URL}GetRegionsV2");

        var result = JsonConvert.DeserializeObject<GetRegionsResponse>(response.ToString());

        if (string.IsNullOrEmpty(result?.error))
        {
            _cache.Set(cacheKey, result, _cacheOptions);
        }
        return result;
    }

    public Task<bool> GetTestRequest(string clientId, string apiKey)
    {
        return _apiClient.GetTestRequest(_apiConfig.Login, _apiConfig.Password);
    }

    public ZzapDataManager SetClient(string clientId, string apiKey)
    {
        throw new NotImplementedException();
    }
}
