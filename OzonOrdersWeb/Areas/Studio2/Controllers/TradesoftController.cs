using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using OzonDomains.Models;
using OzonDomains.Models.OrderCarts;
using OzonOrdersWeb.Areas.Studio2.ViewModels.CartViewModels;
using OzonRepositories.Data;
using RestEase;
using Servcies.ApiServcies.AbcpApi;
using Servcies.ApiServcies.AbcpApi.Models.Request;
using Servcies.ApiServcies.AvdApiConfig;
using Servcies.ApiServcies.TradesoftApi;
using Servcies.ApiServcies.TradesoftApi.Models;
using Servcies.ApiServcies.ZzapApi;
using Servcies.DataServcies;
using Servcies.FiltersServcies.SortModels;
using Servcies.PriceСlculationServcies;
using Servcies.SignalRServcies;

namespace OzonOrdersWeb.Controllers;


[Area("Studio2")]
public class TradesoftController : Controller
{
    private readonly TradesoftDataManager _dataManager;
    private readonly MaxiPartsConfig _maxiPartsConfig;
    private readonly OrdersDataServcies _orderServcies;
    private readonly OrderItemRepository _orderItemRepository;
    private readonly OrderCartRepository _orderCartRepository;
    private readonly ZzapDataManager _zzapDataManager;
    private readonly AbcpDataManager _abcpDataManager;
    private Dictionary<string, string>? _distributorsDict;
    private readonly SupplierDataServcies _supplierDataServices;
    private readonly AvdDataManager _avdDataManager;
    private readonly CurrencyRateFetcher _currencyRateFetcher;
    public TradesoftController(TradesoftDataManager dataManager, 
                               MaxiPartsConfig maxiPartsConfig,
                               OrdersDataServcies orderServcies,
                               OrderItemRepository orderItemRepository,
                               OrderCartRepository orderCartRepository,
                               ZzapDataManager zzapDataManager,
                               AbcpDataManager abcpDataManager,
                               SupplierDataServcies supplierDataServices,
                               AvdDataManager avdDataManager,
                               CurrencyRateFetcher currencyRateFetcher)
    {
        _dataManager = dataManager;
        _maxiPartsConfig = maxiPartsConfig;
        _orderServcies = orderServcies;
        _orderItemRepository = orderItemRepository;
        _orderCartRepository = orderCartRepository;
        _zzapDataManager = zzapDataManager;
        _abcpDataManager = abcpDataManager;
        _supplierDataServices = supplierDataServices;
        _avdDataManager = avdDataManager;
        _currencyRateFetcher = currencyRateFetcher;
    }

    public async Task<IActionResult> GetTradesoftData(int orderId)
    {
        try
        {
            Order order = await _orderServcies.GetOrder(orderId);
            if (order == null)
            {
                return NotFound("Order not found");
            }

            // Проверка на null для Article и EtProducer
            if (string.IsNullOrEmpty(order.Article))
            {
                return BadRequest("Article is empty");
            }
            
            if (order.EtProducer == null || string.IsNullOrEmpty(order.EtProducer.Name))
            {
                return BadRequest("Producer information is missing");
            }

            var providers = await _dataManager.GetProviderList();
            if (providers?.Data == null || providers.Data.Count == 0)
            {
                return NotFound("No providers found");
            }
            foreach (var provider in providers.Data)
            {
                if (provider == null) continue;
                
                provider.ProductPreOrderSearch = await _dataManager.GetPriceList(new PreOrderContainer
                {
                    Provider = provider.Name,
                    Login = _maxiPartsConfig.User,
                    Password = _maxiPartsConfig.Password,
                    Code = order.Article,
                    Producer = order.EtProducer.Name
                });
            }
            var firstProvider = providers.Data.FirstOrDefault();
            if (firstProvider?.ProductPreOrderSearch?.Container == null || 
                firstProvider.ProductPreOrderSearch.Container.Count == 0)
            {
                return NotFound("No price data available");
            }

            var firstContainer = firstProvider.ProductPreOrderSearch.Container[0];
            if (firstContainer?.Items == null)
            {
                return NotFound("No items in price data");
            }
            
            foreach (var item in firstContainer.Items)
            {
                if (item == null) continue;
                
                item.SiteUrl = "https://www.maxi.parts/";
                item.Title = "MAXI.PARTS";
                if (item.MinQuantity != 0)
                {
                    item.PriceDescription = $"Минимальное количество в заказе - {item.MinQuantity}";
                }
            }

            return Ok(await _supplierDataServices.SetAdditionalTerm(firstContainer.Items));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message} / {ex.StackTrace} / {ex.Source}");
        }
    }
    
    public async Task<IActionResult> GetTradesoftDataForBitrix(string article, string producer)
    {
        try
        {
            var providers = await _dataManager.GetProviderList();
            if (providers?.Data == null || providers.Data.Count == 0)
            {
                return NotFound("No providers found");
            }
            foreach (var provider in providers.Data)
            {
                if (provider == null) continue;
                
                provider.ProductPreOrderSearch = await _dataManager.GetPriceList(new PreOrderContainer
                {
                    Provider = provider.Name,
                    Login = _maxiPartsConfig.User,
                    Password = _maxiPartsConfig.Password,
                    Code = article,
                    Producer = producer
                });
            }
            var firstProvider = providers.Data.FirstOrDefault();
            if (firstProvider?.ProductPreOrderSearch?.Container == null || 
                firstProvider.ProductPreOrderSearch.Container.Count == 0)
            {
                return NotFound("No price data available");
            }

            var firstContainer = firstProvider.ProductPreOrderSearch.Container[0];
            if (firstContainer?.Items == null)
            {
                return NotFound("No items in price data");
            }
            
            foreach (var item in firstContainer.Items)
            {
                if (item == null) continue;
                
                item.SiteUrl = "https://www.maxi.parts/";
                item.Title = "MAXI.PARTS";
                if (item.MinQuantity != 0)
                {
                    item.PriceDescription = $"Минимальное количество в заказе - {item.MinQuantity}";
                }
                item.CostPriceFormatted = $"{Decimal.Parse(item.Price, CultureInfo.InvariantCulture) * await _currencyRateFetcher.GetEURRateAsync():F2} ₽";
                item.CostPrice = item.Price;
            }

            return Ok(await _supplierDataServices.SetAdditionalTerm(firstContainer.Items));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message} / {ex.StackTrace} / {ex.Source}");
        }
    }

    public async Task<IActionResult> GetZzapData(int orderId, int regionCode=1)
    {
        try
        {
            Order order = await _orderServcies.GetOrder(orderId);
            if (order == null || order.EtProducer == null)
            {
                return NotFound($"Заказ с ID {orderId} не найден или не содержит данных о производителе.");
            }

            string partNumber = order.Article;
            string manufacturer = order.EtProducer.Name;
            var searchResult = await _zzapDataManager.SearchParts(
                searchText: partNumber,
                partNumber: partNumber,
                manufacturer: manufacturer,
                regionCode: regionCode
            );
            
            if (searchResult.Status == "success")
            {
                return Ok(await _supplierDataServices.SetAdditionalTerm(searchResult.Data.ToPreOrderItems()));
            }
            else
            {
                return BadRequest($"Ошибка от Zzap API: {searchResult.Message}");
            }
        }
        catch (Exception ex)
        {
            // Ловим любые другие исключения
            return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
        }
    }
    
    public async Task<IActionResult> GetZzapDataForBitrix(string article, string producer, int regionCode=1)
    {
        try
        {
            var searchResult = await _zzapDataManager.SearchParts(
                searchText: article,
                partNumber: article,
                manufacturer: producer,
                regionCode: regionCode
            );
            
            if (searchResult.Status == "success")
            {
                var result = await _supplierDataServices.SetAdditionalTerm(searchResult.Data.ToPreOrderItems());
                foreach (var item in result)
                {
                    item.CostPriceFormatted = $"{item.Price} ₽";
                    item.CostPrice = item.Price;
                }
                return Ok(result);
            }
            else
            {
                return BadRequest($"Ошибка от Zzap API: {searchResult.Message}");
            }
        }
        catch (Exception ex)
        {
            // Ловим любые другие исключения
            return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
        }
    }
    
    public async Task<IActionResult> GetAbcpData(int orderId)
    {
        try
        {
            Order order = await _orderServcies.GetOrder(orderId);
            if (order == null || order.EtProducer == null)
            {
                return NotFound($"Заказ с ID {orderId} не найден или не содержит данных о производителе.");
            }

            var searchItems = new List<SearchItem>
            {
                new SearchItem
                {
                    Number = order.Article,
                    Brand = order.EtProducer.Name
                }
            };

            var distributorsDict = await _abcpDataManager.GetDistributorsDictionary();
            var articles = await _abcpDataManager.SearchArticles(searchItems);
            foreach (var article in articles)
            {
                if (distributorsDict.TryGetValue(article.DistributorId.ToString(), out var publicName))
                {
                    article.DistributorCode = publicName;
                }
                else
                {
                    article.DistributorCode = article.DistributorId.ToString();
                }
            }
            
            return Ok(await _supplierDataServices.SetAdditionalTerm(articles.ToPreOrderItems()));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Ошибка при поиске артикулов",
                Details = ex.Message
            });
        }
        
    }
    
    public async Task<IActionResult> GetAbcpDataForBitrix(string article, string producer)
    {
        try
        {
            var searchItems = new List<SearchItem>
            {
                new SearchItem
                {
                    Number = article,
                    Brand = producer
                }
            };

            var distributorsDict = await _abcpDataManager.GetDistributorsDictionary();
            var articles = await _abcpDataManager.SearchArticles(searchItems);
            foreach (var art in articles)
            {
                if (distributorsDict.TryGetValue(art.DistributorId.ToString(), out var publicName))
                {
                    art.DistributorCode = publicName;
                }
                else
                {
                    art.DistributorCode = art.DistributorId.ToString();
                }
            }

            var result = await _supplierDataServices.SetAdditionalTerm(articles.ToPreOrderItems());
            foreach (var item in result)
            {
                item.CostPriceFormatted = $"{item.Price} ₽";
                item.CostPrice = item.Price;

            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Ошибка при поиске артикулов",
                Details = ex.Message
            });
        }
        
    }
    
    public async Task<IActionResult> GetAvdData(int orderId)
    {
        try
        {
            Order order = await _orderServcies.GetOrder(orderId);
            if (order == null || order.EtProducer == null)
            {
                return NotFound($"Заказ с ID {orderId} не найден или не содержит данных о производителе.");
            }
            var result = await _avdDataManager.GetOriginalPriceAsync(order.Article, 
                                                                                    order.EtProducer.Name.ToUpper());
            return Ok(result.ToPreOrderItems());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Ошибка при поиске артикулов",
                Details = ex.Message
            });
        }
    }
    
    public async Task<IActionResult> GetAvdDataForBitrix(int orderId)
    {
        try
        {
            Order order = await _orderServcies.GetOrder(orderId);
            if (order == null || order.EtProducer == null)
            {
                return NotFound($"Заказ с ID {orderId} не найден или не содержит данных о производителе.");
            }
            var items = await _avdDataManager.GetOriginalPriceAsync(order.ProductKey?.Split('=')[0], 
                catalog:order.EtProducer?.Name.ToUpper());
            var result = items.ToPreOrderItems();
            foreach (var item in result)
            {
                item.CostPriceFormatted = $"{item.Price} ₽";
                item.CostPrice = item.Price;
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Ошибка при поиске артикулов",
                Details = ex.Message
            });
        }
    }
    
    public async Task<IActionResult> GetOneAbcpData(int orderId)
    {
        try
        {
            Order order = await _orderServcies.GetOrder(orderId);
            if (order == null || order.EtProducer == null)
            {
                return NotFound($"Заказ с ID {orderId} не найден или не содержит данных о производителе.");
            }

            var searchItems = new List<SearchItem>
            {
                new SearchItem
                {
                    Number = order.Article,
                    Brand = order.EtProducer.Name
                }
            };

            var articles = await _abcpDataManager.SearchArticles(searchItems);
            var result = articles.Where(ar => ar.Weight != 0).FirstOrDefault();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Ошибка при поиске артикулов",
                Details = ex.Message
            });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdateItemsStatus([FromBody] List<OrderItemStatusUpdateRequest> items)
    {
        if (items == null || !items.Any()) return BadRequest("Нет данных");

        var request = new GetItemsStatusContainer
        {
            Provider = "war_provider_maxi_parts",
            Login = _maxiPartsConfig.User,
            Password = _maxiPartsConfig.Password,
            Items = items.Select(x => x.Code).ToList()
        };

        var response = await _dataManager.GetItemsStatusAsync(request);

        if (response?.Container == null) return BadRequest("Пустой ответ от API");

        var container = response.Container.FirstOrDefault();
        if (container?.Items == null) return Ok();

        foreach (var item in items)
        {
            if (container.Items.TryGetValue(item.Code, out var statusItem) && string.IsNullOrEmpty(statusItem.Error))
            {
                var cartItem = await _orderItemRepository.GetAsync(int.Parse(item.Id));
                if (cartItem != null)
                {
                    cartItem.ItemStatus = new ItemStatus
                    {
                        Name = statusItem.StateName
                    };

                    await _orderItemRepository.Update(cartItem);
                }
            }
        }
        return Ok();
    }
    
    public async Task<IActionResult> GetRegions()
    {
        var regions = await _zzapDataManager.GetRegions();
        return Ok(regions);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateItemsStatusAndRedirect(int itemId, int page)
    {
        var item = await _orderCartRepository.GetItemId(itemId);
        
        var request = new GetItemsStatusContainer
        {
            Provider = "war_provider_maxi_parts",
            Login = _maxiPartsConfig.User,
            Password = _maxiPartsConfig.Password,
            Items = [item.OrderItemCode]
        };

        var response = await _dataManager.GetItemsStatusAsync(request);

        if (response?.Container == null) return BadRequest("Пустой ответ от API");

        var container = response.Container.FirstOrDefault();
        if (container?.Items == null) return Ok();
        
        if (container.Items.TryGetValue(item.OrderItemCode, out var statusItem) && string.IsNullOrEmpty(statusItem.Error))
        {
            var cartItem = await _orderItemRepository.GetAsync(itemId);
            if (cartItem != null)
            {
                cartItem.ItemStatus = new ItemStatus
                {
                    Name = statusItem.StateName
                };

                await _orderItemRepository.Update(cartItem);
            }
        }
        return RedirectToRoute(new 
        { 
            area = "Studio2",
            controller = "Orders",
            action = "Index",
            sortOrder = GetSortStateCookie(),
            page = page
        });
    }
    
    public async Task<IActionResult> GetProviders()
    {
        try
        {
            var result = await _dataManager.GetProviderList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    public async Task<IActionResult> GetActiveProviders()
    {
        try
        {
            var result = await _dataManager.GetActiveProviders();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    public async Task<IActionResult> GetPrices(string code, string produser)
    {
        try
        {
            var result = await _dataManager.GetPriceList(new PreOrderContainer
            {
                Provider = "war_provider_maxi_parts",
                Login = _maxiPartsConfig.User,
                Password = _maxiPartsConfig.Password,
                Code = code,
                Producer = produser
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    private OrderSortState GetSortStateCookie()
    {
        var sortStateCookie = Request.Cookies["SortState"];
        if (!string.IsNullOrEmpty(sortStateCookie) && Enum.TryParse<OrderSortState>(sortStateCookie, out var savedSortState))
        {
            return savedSortState;
        }
        return OrderSortState.StandardState;
    }
}
