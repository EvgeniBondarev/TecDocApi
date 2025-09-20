using System.Text;
using System.Text.Json;
using PartsInfo.Cache;

namespace PartsInfo.HttpUtils;

public class ProxyHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly UrlResponseCacheService _cacheService;

    public ProxyHttpClientService(UrlResponseCacheService cacheService)
    {
        _httpClient = new HttpClient();
        _cacheService = cacheService;
    }

    public async Task<JsonDocument?> GetJsonAsync(string url)
    {
        if (_cacheService.TryGet(url, out var cachedResponse))
        {
            Console.WriteLine($"Найдено в кэше - {url}");
            return JsonDocument.Parse(cachedResponse);   
        }

        try
        {
            using var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[HTTP ERROR] URL: {url}\nStatus: {(int)response.StatusCode} ({response.StatusCode})\nResponse: {errorContent}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            _cacheService.Set(url, content, TimeSpan.FromMinutes(5));
            return JsonDocument.Parse(content);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[EXCEPTION] HttpRequestException while calling: {url}\nMessage: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] Unexpected error while calling: {url}\n{ex}");
            return null;
        }
    }
    
    public async Task<JsonDocument?> PostJsonAsync(string url, object data)
    {
        string cacheKey = $"{url}_{JsonSerializer.Serialize(data)}";
        
        if (_cacheService.TryGet(cacheKey, out var cachedResponse))
        {
            Console.WriteLine($"Найдено в кэше - {url}");
            return JsonDocument.Parse(cachedResponse);   
        }

        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[HTTP ERROR] URL: {url}\nStatus: {(int)response.StatusCode} ({response.StatusCode})\nResponse: {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _cacheService.Set(cacheKey, responseContent, TimeSpan.FromMinutes(5));
            return JsonDocument.Parse(responseContent);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[EXCEPTION] HttpRequestException while calling: {url}\nMessage: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPTION] Unexpected error while calling: {url}\n{ex}");
            return null;
        }
    }
}