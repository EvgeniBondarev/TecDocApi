using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using OzonDomains.Models;
using Servcies.ApiServcies._1CApi.Models;
using Servcies.DataServcies;

namespace Servcies.ApiServcies._1CApi;

public class OneCTransferManager : IApiDataManager<OneCTransferManager>
{
    private OneCApiClient _apiClient;
    private OneCApiConfig _apiConfig;
    private readonly IMemoryCache _cache;
    private readonly WarehouseMappingDataServcies _warehouseMappingDataServcies;

    public OneCTransferManager(OneCApiConfig config, 
                            IMemoryCache cache, 
                            WarehouseMappingDataServcies warehouseMappingDataServcies)
    {
        _apiConfig = config;
        _apiClient = new OneCApiClient(config.User, config.Password);
        _cache = cache;
        _warehouseMappingDataServcies = warehouseMappingDataServcies;
    }

    public OneCTransferManager SetClient(string user, string password)
    {
        _apiClient = new OneCApiClient(user, password);
        _apiConfig.User = user;
        _apiConfig.Password = password;
        return this;
    }

    public async Task<List<OneCResponse>> TransferStock(List<Order> ordersToTransfer)
    {
        if (ordersToTransfer == null || !ordersToTransfer.Any())
        {
            throw new ArgumentException("Список заказов не может быть пустым.");
        }
        
        var distinctWarehouses = ordersToTransfer
            .Where(o => o.ShipmentWarehouse != null && !string.IsNullOrEmpty(o.ShipmentWarehouse.Name))
            .GroupBy(o => o.ShipmentWarehouse.Name)
            .ToList();

        if (distinctWarehouses.Count > 1)
        {
            var warehouseNames = string.Join(", ", distinctWarehouses.Select(g => g.Key));
            throw new ArgumentException($"Заказы имеют различные склады отгрузки: {warehouseNames}");
        }

        if (distinctWarehouses.Count == 0)
        {
            throw new ArgumentException("Не указан склад отгрузки для заказов.");
        }

        
        var mappingWarehouse = await _warehouseMappingDataServcies.GetByOzonName(distinctWarehouses[0].Key);
        if (mappingWarehouse == null)
        {
            throw new ArgumentException($"Связь складов не найдена для {distinctWarehouses[0].Key}.");
        }
        var warehouseName = mappingWarehouse.BitrixWarehouseName;

        var ordersByClient = ordersToTransfer
            .Where(o => o.OzonClient != null)
            .GroupBy(o => o.OzonClient)
            .ToDictionary(g => g.Key, g => g.ToList());

        if (!ordersByClient.Any())
        {
            throw new ArgumentException("Не найдено заказов с валидными данными клиента Ozon.");
        }

        var results = new List<OneCResponse>();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        List<MovementOfGoodsRequest> modelsToTransfer = [];

        foreach (var (client, orders) in ordersByClient)
        {
            if (string.IsNullOrEmpty(client.INNCode) || string.IsNullOrEmpty(client.WarehouseName))
            {
                throw new ArgumentException($"Клиент '{client.Name}' не имеет обязательных данных (ИНН или склад).");
            }
            
            var products = orders
                    .Where(o => !string.IsNullOrEmpty(o.Article) && o.Quantity.HasValue && o.Quantity.Value > 0)
                    .Select(o => new MovementProduct
                    {
                        Article = o.Article,
                        Quantity = o.Quantity.Value
                    })
                    .ToList();

            if (!products.Any())
            {
                throw new ArgumentException($"Для клиента '{client.Name}' не найдено товаров с валидными данными.");
            }

            var model = new MovementOfGoodsRequest()
            {
                    Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz)
                            .ToString("dd.MM.yyyy HH:mm:ss", new System.Globalization.CultureInfo("ru-RU")),
                    INNorganization = client.INNCode,
                    SenderRecipient = client.WarehouseName,
                    WarehouseSender = warehouseName,
                    Comment = $"Перемещение для клиента {client.Name}. Заказы: {string.Join(", ", orders.Select(o => o.ShipmentNumber))}",
                    Products = products 
            };
            modelsToTransfer.Add(model);
        }

        try
        {
            foreach (var model in modelsToTransfer )
            {
                results.Add(await CreateMovementOfGoodsAsync(model));
            }
        }
        catch (Exception ex)
        {
            var errorResponse = new OneCResponse
            {
                Success = false,
                Message = $"Ошибка при обработке заказов клиента {ex.Message}",
                TransactionId = null
            };
            results.Add(errorResponse);
        }
        return results;
    }
    public async Task<OneCResponse> CreateMovementOfGoodsAsync(MovementOfGoodsRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Запрос не может быть пустым");
        }

        if (string.IsNullOrEmpty(_apiConfig.User) || string.IsNullOrEmpty(_apiConfig.Password))
        {
            throw new InvalidOperationException("Учетные данные для API 1C не настроены");
        }
        if (string.IsNullOrEmpty(request.Date))
            throw new ArgumentException("Дата является обязательным полем");
    
        if (string.IsNullOrEmpty(request.INNorganization))
            throw new ArgumentException("ИНН организации является обязательным полем");
    
        if (request.Products == null || !request.Products.Any())
            throw new ArgumentException("Необходим хотя бы один товар в документе");

        var result = new OneCResponse();
        try
        {
            var response = await _apiClient.MakeRequestPostAsync(request, OneCApiUrl.MOVEMENT_OF_GOODS_URL);
            result.Success = true;
            result.Message = "Документ перемещения успешно создан";
            result.TransactionId = response;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Ошибка при создании документа перемещения: {ex.Message}";
            result.TransactionId = string.Empty;
        }
        return result;
    }
    

    public async Task<bool> TestConnectionAsync()
    {
        return await _apiClient.GetTestRequest(_apiConfig.User, _apiConfig.Password);
    }

    public async Task<bool> GetTestRequest(string user, string password)
    {
        return await _apiClient.GetTestRequest(user, password);
    }
}