using Newtonsoft.Json;
using Servcies.ApiServcies._1CApi;
using Servcies.ApiServcies._1CApi.Models;

namespace Servcies.ApiServcies;

public class OneCDataManager
{
    private readonly OneCApiClient _apiClient;
    private readonly OneCApiConfig _config;

    public OneCDataManager(OneCApiConfig config)
    {
        _config = config;
        _apiClient = new OneCApiClient(config.User, config.Password);
    }

    public async Task<List<WarehouseResponse>> GetWarehouseListAsync()
    {
        var response = await _apiClient.MakeRequestGetAsync(OneCApiUrl.GET_WAREHOUSES_URL);

        if (string.IsNullOrWhiteSpace(response))
            throw new Exception("Ответ от API 1C пуст.");

        try
        {
            var warehouses = JsonConvert.DeserializeObject<List<WarehouseResponse>>(response);

            if (warehouses == null || warehouses.Count == 0)
                throw new Exception("Не удалось получить список складов из 1C.");

            return warehouses;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Ошибка десериализации ответа API 1C: {ex.Message}");
        }
    }
}