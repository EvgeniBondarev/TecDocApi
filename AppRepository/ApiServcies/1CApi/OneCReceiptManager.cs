using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using OzonDomains.Models;
using Servcies.ApiServcies;
using Servcies.ApiServcies._1CApi;
using Servcies.ApiServcies._1CApi.DTO;
using Servcies.ApiServcies._1CApi.Models;
using Servcies.DataServcies;

public class OneCReceiptManager : IApiDataManager<OneCReceiptManager>
{
    private OneCApiClient _apiClient;
    private OneCApiConfig _apiConfig;
    private readonly IMemoryCache _cache;
    private readonly WarehouseMappingDataServcies _warehouseMappingDataServcies;

    public OneCReceiptManager(OneCApiConfig config, 
        IMemoryCache cache, 
        WarehouseMappingDataServcies warehouseMappingDataServcies)
    {
        _apiConfig = config;
        _apiClient = new OneCApiClient(config.User, config.Password);
        _cache = cache;
        _warehouseMappingDataServcies = warehouseMappingDataServcies;
    }
    
    public async Task<List<OneCResponse>> CreateReceipts(List<Order> ordersToReceipt, 
                                                         ShippedBySupplierTransactionDto shippedBySupplierTransactionDto)
    {
        if (ordersToReceipt == null || !ordersToReceipt.Any())
        {
            throw new ArgumentException("Список заказов не может быть пустым.");
        }
        
        var distinctWarehouses = ordersToReceipt
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
        
        var distinctSuppliers = ordersToReceipt
            .Where(o => o.Supplier != null && !string.IsNullOrEmpty(o.Supplier.INNCode))
            .GroupBy(o => o.Supplier.INNCode)
            .ToList();

        if (distinctSuppliers.Count > 1)
        {
            var suppliersNames = string.Join(", ", distinctWarehouses.Select(g => g.Key));
            throw new ArgumentException($"Заказы имеют различных поставщиков: {suppliersNames}");
        }

        
        var mappingWarehouse = await _warehouseMappingDataServcies.GetByOzonName(distinctWarehouses[0].Key);
        if (mappingWarehouse == null)
        {
            throw new ArgumentException($"Связь складов не найдена для {distinctWarehouses[0].Key}.");
        }
        var warehouseName = mappingWarehouse.BitrixWarehouseName;
        
        var supplierName = distinctSuppliers[0].Key;
        if (supplierName.IsNullOrEmpty())
        {
            throw new ArgumentException($"Поставщик не найден по ИНН.");
        }

        var ordersByClient = ordersToReceipt
            .Where(o => o.OzonClient != null)
            .GroupBy(o => o.OzonClient)
            .ToDictionary(g => g.Key, g => g.ToList());

        if (!ordersByClient.Any())
        {
            throw new ArgumentException("Не найдено заказов с валидными данными клиента Ozon.");
        }

        var results = new List<OneCResponse>();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
        List<ReceiptOfGoodsRequest> modelsToTransfer = [];

        foreach (var (client, orders) in ordersByClient)
        {
            if (string.IsNullOrEmpty(client.INNCode) || string.IsNullOrEmpty(client.WarehouseName))
            {
                throw new ArgumentException($"Клиент '{client.Name}' не имеет обязательных данных (ИНН или склад).");
            }
            
            var products = orders
                .Where(o => !string.IsNullOrEmpty(o.Article) && o.Quantity.HasValue && o.Quantity.Value > 0)
                .Select(o => new ReceiptProduct
                {
                    Article = o.Article,
                    Quantity = o.Quantity.Value,
                    Price = o.PurchasePrice ?? 0,
                    Sum = (o.PurchasePrice ?? 0) * o.Quantity.Value,
                    SumNDS = (o.PurchasePrice ?? 0) * o.Quantity.Value * (o.Supplier.IsVatApplicable ? 0.20m : null) // проверка НДС
                })
                .ToList();


            if (!products.Any())
            {
                throw new ArgumentException($"Для клиента '{client.Name}' не найдено товаров с валидными данными.");
            }

            var model = new ReceiptOfGoodsRequest()
            {
                    Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz)
                            .ToString("dd.MM.yyyy HH:mm:ss", new System.Globalization.CultureInfo("ru-RU")),
                    INNorganization = client.INNCode,
                    Innbuyer = supplierName,
                    Subdivisions  = client.WarehouseName,
                    SenderRecipient  = warehouseName,
                    Comment = shippedBySupplierTransactionDto.TransactionComment,
                    NDS = orders.FirstOrDefault().Supplier.IsVatApplicable ? "да" : "",
                    DateVh = TimeZoneInfo.ConvertTimeFromUtc(shippedBySupplierTransactionDto.DateVh, tz)
                        .ToString("dd.MM.yyyy HH:mm:ss", new System.Globalization.CultureInfo("ru-RU")),
                    NumberVh = shippedBySupplierTransactionDto.NumberVh,
                    Contract = shippedBySupplierTransactionDto.Contract,
                    Products = products 
            };
            modelsToTransfer.Add(model);
        }

        try
        {
            foreach (var model in modelsToTransfer )
            {
                results.Add(await CreateReceiptOfGoodsAsync(model));
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
    
    public async Task<OneCResponse> CreateReceiptOfGoodsAsync(ReceiptOfGoodsRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Запрос не может быть пустым");

        if (string.IsNullOrEmpty(_apiConfig.User) || string.IsNullOrEmpty(_apiConfig.Password))
            throw new InvalidOperationException("Учетные данные для API 1C не настроены");

        if (string.IsNullOrEmpty(request.Date))
            throw new ArgumentException("Дата является обязательным полем");

        if (string.IsNullOrEmpty(request.INNorganization))
            throw new ArgumentException("ИНН организации является обязательным полем");

        if (string.IsNullOrEmpty(request.Subdivisions))
            throw new ArgumentException("Подразделение является обязательным полем");

        if (string.IsNullOrEmpty(request.SenderRecipient))
            throw new ArgumentException("Отправитель/получатель является обязательным полем");

        if (string.IsNullOrEmpty(request.Innbuyer))
            throw new ArgumentException("ИНН покупателя является обязательным полем");

        if (request.Products == null || !request.Products.Any())
            throw new ArgumentException("Необходим хотя бы один товар в документе");

        var result = new OneCResponse();
        try
        {
            var response = await _apiClient.MakeRequestPostAsync(request, OneCApiUrl.RECEIPT_OF_GOODS_URL);
            result.Success = true;
            result.Message = "Документ поступления успешно создан";
            result.TransactionId = response;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Ошибка при создании документа поступления: {ex.Message}";
            result.TransactionId = string.Empty;
        }

        return result;
    }

    public OneCReceiptManager SetClient(string user, string password)
    {
        _apiClient = new OneCApiClient(user, password);
        _apiConfig.User = user;
        _apiConfig.Password = password;
        return this;
    }
    
    public Task<bool> GetTestRequest(string clientId, string apiKey)
    {
        throw new NotImplementedException();
    }
    
}