using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using OzonDomains;
using OzonDomains.Models;
using OzonDomains.Models.BitrixModels;
using OzonOrdersWeb.Areas.Studio2.ViewModels.Bitrix;
using OzonOrdersWeb.Areas.Studio2.Views;
using OzonRepositories.Data;
using OzonRepositories.Data.Bitrix;
using Servcies.ApiServcies.OzonApi;
using Servcies.ApiServcies.OzonApi.Filters;
using Servcies.ApiServcies.YandexApi;
using Servcies.DataServcies;
using Servcies.ImportProductPricesServcies;
using Servcies.PriceСlculationServcies;

[Area("Studio2")]
[Authorize(Roles = "User,Admin")]
public class BitrixStockController : Controller
{
    private readonly BitrixStockRepository _repository;
    private readonly EtProducerRepository _etProducerRepository;
    private readonly OzonClientServcies _ozonClientServcies;
    private OzonApiDataManager _ozonApiDataManager;
    private  List<OzonClient> _ozonClients = new List<OzonClient>();
    private CurrencyRateFetcher _currencyRateFetcher;
    private SupplierDataServcies _supplierDataServcies;
    private readonly IPriceHistoryDataService _priceHistoryService;
    private readonly WarehouseMappingDataServcies _warehouseMappingDataServcies;
    private readonly IMemoryCache _memoryCache;
    private readonly ImportProductPricesManager _importProductPricesManager;
    public BitrixStockController(BitrixStockRepository repository,
                                 EtProducerRepository etProducerRepository,
                                 OzonClientServcies ozonClientServcies,
                                 CurrencyRateFetcher currencyRateFetcher,
                                 SupplierDataServcies supplierDataServcies,
                                 IPriceHistoryDataService priceHistoryService,
                                 WarehouseMappingDataServcies warehouseMappingDataServcies,
                                 IMemoryCache memoryCache,
                                 ImportProductPricesManager importProductPricesManager)
    {
        _repository = repository;
        _etProducerRepository = etProducerRepository;
        _ozonClientServcies = ozonClientServcies;
        _currencyRateFetcher = currencyRateFetcher;
        _supplierDataServcies = supplierDataServcies;
        _priceHistoryService = priceHistoryService;
        _warehouseMappingDataServcies = warehouseMappingDataServcies;
        _memoryCache = memoryCache;
        _importProductPricesManager = importProductPricesManager;
    }
    
    public async Task<IActionResult> Index([FromQuery] RemainingStockFilter filter)
    {
        if (filter.Page <= 0) filter.Page = 1;
        if (filter.PageSize <= 0) filter.PageSize = 20;
        
        await SetClients();
        var result = await _repository.GetRemainingStockAsync(filter);
        
        var suppliers = result.Items
            .Where(r => !string.IsNullOrWhiteSpace(r.Supplier))
            .Select(r => r.Supplier!)
            .Distinct()
            .ToList();
        
        var prefixes = await _etProducerRepository.GetMarketPrefixesByNamesAsync(suppliers);
        
        var productIds = result.Items.Select(x => x.ProductId).ToList();
        var priceHistories = await _priceHistoryService.GetLastByBitrixIdsAsync(productIds);
        
        foreach (var item in result.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.Supplier) && prefixes.TryGetValue(item.Supplier, out var prefix))
            {
                item.MarketPrefix = prefix;
            }
            
            if (priceHistories.TryGetValue(item.ProductId, out var priceHistory))
            {
                item.PriceHistory = priceHistory;
            }
        }
        
        if (filter.RegistrationPriceFrom.HasValue)
        {
            result.Items = result.Items
                .Where(x => x.PriceHistory != null && x.PriceHistory.RegistrationPrice >= filter.RegistrationPriceFrom.Value)
                .ToList();
        }
        
        if (filter.RegistrationPriceTo.HasValue)
        {
            result.Items = result.Items
                .Where(x => x.PriceHistory != null && x.PriceHistory.RegistrationPrice <= filter.RegistrationPriceTo.Value)
                .ToList();
        }
        
        if (filter.LoadOzonWarehouses)
        {
            result.Items = await LoadOzonWarehousesAsync(result.Items, filter.LoadOzonWarehouses);

            if (!string.IsNullOrEmpty(filter.OzonStoreTitle))
            {
                result.Items = result.Items
                    .Where(x => x.OzonStores.Any(s => s.Title.Contains(filter.OzonStoreTitle, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            if (filter.OzonAmountFrom.HasValue)
            {
                result.Items = result.Items
                    .Where(x => x.OzonStores.Any(s => s.Amount >= filter.OzonAmountFrom.Value))
                    .ToList();
            }
            if (filter.OzonAmountTo.HasValue)
            {
                result.Items = result.Items
                    .Where(x => x.OzonStores.Any(s => s.Amount <= filter.OzonAmountTo.Value))
                    .ToList();
            }
        }

        var vm = new RemainingStockViewModel
        {
            Result = result,
            Filter = filter,
            Suppliers = await _supplierDataServcies.GetSuppliersWithUrlAsync(),
            AllSuppliers = await _supplierDataServcies.GetSuppliers(),
            WarehousesmMappings = await _warehouseMappingDataServcies.GetWarehouseMappings(),
            ActiveStores = await _repository.GetActiveStoresAsync(),
            RateEUR = await _currencyRateFetcher.GetEURRateAsync(),
            RateUSD = await _currencyRateFetcher.GetUSDRateAsync(),
            RateBYN = await _currencyRateFetcher.GetBYNRateAsync(),
        };
        return View(vm);
    }
    
    private async Task<List<RemainingStockBitrix>> LoadOzonWarehousesAsync(
    List<RemainingStockBitrix> items, 
    bool loadOzonWarehouses)
    {
        if (!loadOzonWarehouses || items == null || !items.Any())
            return items;

        var articles = items
            .Where(x => !string.IsNullOrEmpty(x.ArticleWithKey))
            .Select(x => x.ArticleWithKey)
            .ToArray();

        if (!articles.Any())
            return items;

        // 2. offer_id -> список sku для всех клиентов
        var offerSkuDict = new Dictionary<string, HashSet<string>>();
        foreach (var client in _ozonClients)
        {
            _ozonApiDataManager.SetClient(client.DecryptClientId, client.DecryptApiKey);
            var stockResponse = await _ozonApiDataManager.GetProductStocks(articles);
            if (stockResponse?["items"] is not JArray itemsArray) 
                continue;

            foreach (var item in itemsArray)
            {
                var offerId = item["offer_id"]?.ToString();
                var sku = item["stocks"]?.FirstOrDefault()?["sku"]?.ToString();

                if (!string.IsNullOrEmpty(offerId) && !string.IsNullOrEmpty(sku))
                {
                    if (!offerSkuDict.ContainsKey(offerId))
                        offerSkuDict[offerId] = new HashSet<string>();
                    offerSkuDict[offerId].Add(sku);
                }
            }
        }

        if (!offerSkuDict.Any())
            return items;

        // 3. Получаем все SKU
        var skuArray = offerSkuDict.Values.SelectMany(x => x).Distinct().ToArray();
        var allWarehouses = new List<(WarehouseStock Stock, string ClientName)>();

        foreach (var client in _ozonClients)
        {
            _ozonApiDataManager.SetClient(client.DecryptClientId, client.DecryptApiKey);
            var whResponse = await _ozonApiDataManager.GetProductWarehousesForBitrix(skuArray, Array.Empty<string>());
            if (whResponse?["result"] is JArray whArray)
            {
                var warehouses = whArray.ToObject<List<WarehouseStock>>();
                allWarehouses.AddRange(
                    warehouses.Select(w => (w, client.Name))
                );
            }
        }

        // 4. Присоединяем к элементам
        foreach (var item in items)
        {
            if (offerSkuDict.TryGetValue(item.ArticleWithKey, out var skus))
            {
                var stockByStores = allWarehouses
                    .Where(w => skus.Contains(w.Stock.sku.ToString()) && w.Stock.present > 0)
                    .Select(w => new StockByStore
                    {
                        StoreId = (int)w.Stock.warehouse_id,
                        Title = w.Stock.warehouse_name,
                        Amount = w.Stock.present,
                        ClientName = w.ClientName
                    })
                    .OrderBy(s => s.Title) 
                    .ToList();

                item.OzonStores = stockByStores;
            }
        }

        return items;
    }



    
    public async Task<IActionResult> GetOzonPrices(string article)
    {
        await SetClients();

        var allItems = new JArray();

        foreach (var client in _ozonClients)
        {
            _ozonApiDataManager.SetClient(client.DecryptClientId, client.DecryptApiKey);

            JObject result = await _ozonApiDataManager.GetProductPricesByArticles(new[] { article });
            var stocks = await _ozonApiDataManager.GetProductStocks(new[] { article });
            List<WarehouseOzon> warehouseList = await GetOrCreateWarehouseCache(client.DecryptClientId);

            if (result != null && result["items"] != null && result["items"].HasValues)
            {
                foreach (var item in result["items"])
                {
                    var commissions = item["commissions"];
                    var priceObj = item["price"];

                    if (commissions != null && priceObj != null)
                    {
                        decimal fboDeliv = commissions["fbo_deliv_to_customer_amount"]?.Value<decimal>() ?? 0;
                        decimal fbsDirectMin = commissions["fbs_direct_flow_trans_min_amount"]?.Value<decimal>() ?? 0;
                        decimal fbsDirectMax = commissions["fbs_direct_flow_trans_max_amount"]?.Value<decimal>() ?? 0;
                        decimal fbsFirstMileMin = commissions["fbs_first_mile_min_amount"]?.Value<decimal>() ?? 0;
                        decimal fbsFirstMileMax = commissions["fbs_first_mile_max_amount"]?.Value<decimal>() ?? 0;
                        decimal salesPercent = commissions["sales_percent_fbs"]?.Value<decimal>() ?? 0;
                        decimal itemPrice = priceObj["price"]?.Value<decimal>() ?? 0;
                        decimal acquiring = item["acquiring"]?.Value<decimal>() ?? 0;
                        double index = 0.91;
                        double marketingPrice = priceObj["marketing_price"]?.Value<double>() ?? 0;

                        decimal ozonReward = itemPrice * salesPercent / 100;

                        decimal maxCommission = ozonReward + acquiring + fbsFirstMileMax + fbsDirectMax + fboDeliv;
                        decimal minCommission = ozonReward + acquiring + fbsFirstMileMin + fbsDirectMin + fboDeliv;

                        double truePrice = _importProductPricesManager.GetTruePrice(marketingPrice, index);

                        if (priceObj is JObject priceJObject)
                        {
                            priceJObject["max_commission"] = Math.Round(maxCommission, 2);
                            priceJObject["min_commission"] = Math.Round(minCommission, 2);
                            priceJObject["ozon_reward"] = Math.Round(ozonReward, 2);
                            priceJObject["true_price"] = Math.Round(truePrice, 2);
                        }
                    }

                    var wrapped = new JObject
                    {
                        ["Article"] = article,
                        ["Client"] = client.Name,
                        ["Item"] = JToken.FromObject(item)
                    };

                    var warehouseInfo = await _ozonApiDataManager.GetWarehouseInfo(stocks, article, client, warehouseList);
                    if (warehouseInfo != null)
                    {
                        wrapped["WarehouseInfo"] = warehouseInfo;
                    }

                    allItems.Add(wrapped);
                }
            }
        }
        return Content(allItems.ToString(), "application/json");
    }
    
   

    private async Task<List<WarehouseOzon>> GetOrCreateWarehouseCache(string clientId)
    {
        var cacheKey = $"WarehouseList_{clientId}";
        if (_memoryCache.TryGetValue(cacheKey, out List<WarehouseOzon> cachedWarehouses))
        {
            return cachedWarehouses;
        }
        var warehouseList = await _ozonApiDataManager.GetWarehouseList();
    
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));
    
        _memoryCache.Set(cacheKey, warehouseList, cacheOptions);
    
        return warehouseList;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProductStocks([FromBody] UpdateProductStocksViewModel updateProductStocksViewModel)
    {
        await SetClients();
        _memoryCache.Remove( $"WarehouseList_{updateProductStocksViewModel.OzonClient}");

        var ozonClient = _ozonClients.Where(o => o.Name == updateProductStocksViewModel.OzonClient).FirstOrDefault();
        if (ozonClient == null)
            return NotFound("Клиент не найден");

        try
        {
            _ozonApiDataManager.SetClient(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
            var result = await _ozonApiDataManager.UpdateProductStocks(new List<StockItemForUpdate>
            {
                new StockItemForUpdate
                {
                    OfferId = updateProductStocksViewModel.OfferId,
                    ProductId = updateProductStocksViewModel.ProductId,
                    Stock = updateProductStocksViewModel.Stock,
                    WarehouseId = updateProductStocksViewModel.WarehouseId
                }
            });
            if (result?.Result == null || result.Result.Count == 0)
            {
                return BadRequest("Ozon API не вернул результат.");
            }
            var failedUpdates = result.Result.Where(r => r.Errors != null && r.Errors.Any()).ToList();
            if (failedUpdates.Any())
            {
                return BadRequest(new
                {
                    Message = "Обновление остатков выполнено с ошибками",
                    Errors = failedUpdates.SelectMany(f => f.Errors)
                });
            }

            if (result.Result.First().Updated)
            {
                return Ok(new
                {
                    Message = "Остатки успешно обновлены",
                    Data = result.Result
                });
            }
            else
            {
                return Ok(new
                {
                    Message = "Не удалось обработать остатки",
                    Data = result.Result
                });
            }
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Ошибка при обработке запроса", Details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProductPrice([FromBody] UpdateProductPriceRequest request)
    {
        await SetClients();
        var targetClient = _ozonClients.FirstOrDefault(o => o.Name == request.ClientName);
        if (targetClient == null)
            return NotFound("Клиент не найден");

        var result = await _importProductPricesManager.UpdateProductPrices(request.Article, 
                                                                                            targetClient,
                                                                                            request.ConstantIndex,
                                                                                            request.Percent);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> SetProductPrice([FromBody] SetProductPriceRequest request)
    {
        await SetClients();

        var targetClient = _ozonClients.FirstOrDefault(o => o.Name == request.ClientName);
        if (targetClient == null)
            return NotFound("Клиент не найден");
        
        var result = await _importProductPricesManager.SetProductPrices(request.Article, 
                                                                                             targetClient,
                                                                                             request.YourPrice,
                                                                                             request.OldPrice,
                                                                                             request.MinPrice,
                                                                                             request.CostPrice);
        return Ok(result);
    }

    
    [HttpPost]
    public async Task<IActionResult> ToggleActiveStatus([FromBody] int productId)
    {
        try
        { 
            var result = await _repository.ToggleElementActiveStatus(productId);
        
            if (result == null)
            {
                return NotFound(new { success = false, message = "Элемент не найден" });
            }

            return Ok(new { 
                success = true, 
                message = "Статус успешно обновлен",
                isActive = result.Value
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                success = false, 
                message = "Ошибка при обновлении статуса",
                error = ex.Message 
            });
        }
    }

    
    private async Task SetClients()
    {
        if (_ozonClients.Count == 0)
        {
            _ozonClients = (await _ozonClientServcies.GetOzonClients()).Where(o => o.ClientType == ClientType.OZON).ToList();
            _ = PreloadWarehouseCachesAsync();
        }
        _ozonApiDataManager = new OzonApiDataManager(_ozonClients[0].DecryptClientId, _ozonClients[0].DecryptApiKey);
    }

    private async Task PreloadWarehouseCachesAsync()
    {
        try
        {
            foreach (var client in _ozonClients)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var cacheKey = $"WarehouseList_{client.DecryptClientId}";

                        if (_memoryCache.TryGetValue(cacheKey, out _))
                        {
                            Console.WriteLine($"Склады уже в кэше для {client.Name}, пропускаем...");
                            return;
                        }

                        var tempManager = _ozonApiDataManager.SetClient(client.DecryptClientId, client.DecryptApiKey);
                        var warehouses = await tempManager.GetWarehouseList();

                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                        _memoryCache.Set(cacheKey, warehouses, cacheOptions);
                        Console.WriteLine($"Склады добавлены в кэш для {client.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Background cache preload failed for client {client.Name}: {ex.Message}");
                    }
                });

                await Task.Delay(100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cache preloading failed: {ex.Message}");
        }
    }
}