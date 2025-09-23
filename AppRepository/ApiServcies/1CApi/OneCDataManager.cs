using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using OzonDomains.Models;
using Servcies.ApiServcies._1CApi.Models;

namespace Servcies.ApiServcies._1CApi;

public class OneCDataManager : IApiDataManager<OneCDataManager>
{
    private OneCApiClient _apiClient;
    private OneCApiConfig _apiConfig;
    private readonly IMemoryCache _cache;

    public OneCDataManager(OneCApiConfig config, IMemoryCache cache)
    {
        _apiConfig = config;
        _apiClient = new OneCApiClient(config.User, config.Password);
        _cache = cache;
    }

    public OneCDataManager SetClient(string user, string password)
    {
        _apiClient = new OneCApiClient(user, password);
        _apiConfig.User = user;
        _apiConfig.Password = password;
        return this;
    }

    public async Task<List<MovementOfGoodsResponse>> TransferStock(List<Order> ordersToTransfer)
    {
        if (ordersToTransfer == null || !ordersToTransfer.Any())
        {
            throw new ArgumentException("Список заказов не может быть пустым.");
        }
        
        // Проверяем, что все заказы имеют один и тот же склад отгрузки
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

        var warehouseName = distinctWarehouses[0].Key;

        // Группируем заказы по клиентам
        var ordersByClient = ordersToTransfer
            .Where(o => o.OzonClient != null)
            .GroupBy(o => o.OzonClient)
            .ToDictionary(g => g.Key, g => g.ToList());

        if (!ordersByClient.Any())
        {
            throw new ArgumentException("Не найдено заказов с валидными данными клиента Ozon.");
        }

        var results = new List<MovementOfGoodsResponse>();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

        foreach (var (client, orders) in ordersByClient)
        {
            // Пропускаем клиентов без обязательных данных
            if (string.IsNullOrEmpty(client.INNCode) || string.IsNullOrEmpty(client.WarehouseName))
            {
                var errorResponse = new MovementOfGoodsResponse
                {
                    Success = false,
                    Message = $"Клиент '{client.Name}' не имеет обязательных данных (ИНН или склад).",
                    TransactionId = null
                };
                results.Add(errorResponse);
                continue;
            }

            try
            {
                // Берем все товары без группировки - каждый товар передается отдельно
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
                    var errorResponse = new MovementOfGoodsResponse
                    {
                        Success = false,
                        Message = $"Для клиента '{client.Name}' не найдено товаров с валидными данными.",
                        TransactionId = null
                    };
                    results.Add(errorResponse);
                    continue;
                }

                // Создаем модель запроса для клиента
                var model = new MovementOfGoodsRequest()
                {
                    Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz)
                            .ToString("dd.MM.yyyy HH:mm:ss", new System.Globalization.CultureInfo("ru-RU")),
                    INNorganization = client.INNCode,
                    SenderRecipient = client.WarehouseName,
                    WarehouseSender = warehouseName,
                    Comment = $"Перемещение для клиента {client.Name}. Заказы: {string.Join(", ", orders.Select(o => o.ShipmentNumber))}",
                    Products = products // Передаем товары без группировки
                };

                // Отправляем запрос в 1С
                var result = await CreateMovementOfGoodsAsync(model);
                results.Add(result);
            }
            catch (Exception ex)
            {
                var errorResponse = new MovementOfGoodsResponse
                {
                    Success = false,
                    Message = $"Ошибка при обработке заказов клиента '{client.Name}': {ex.Message}",
                    TransactionId = null
                };
                results.Add(errorResponse);
            }
        }

        return results;
    }
    public async Task<MovementOfGoodsResponse> CreateMovementOfGoodsAsync(MovementOfGoodsRequest request)
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

        var result = new MovementOfGoodsResponse();
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