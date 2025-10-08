using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using OzonDomains;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.OrderViewModels;
using OzonRepositories.Context;
using PartsInfo.HttpUtils;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.CacheServcies.Cache.UserCacheService;
using Servcies.DataServcies;
using Servcies.FiltersServcies.SortModels;
using Servcies.ParserServcies.FileProcessing;
using Servcies.PriceСlculationServcies;
using Servcies.SignalRServcies;
using Servcies.TransactionUtilsServcies;
using Services.CacheServcies.Cache;

namespace OzonOrdersWeb.Areas.Studio2.Controllers;

[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
public class PercentageController : Controller
{
     private readonly OzonOrderContext _context;
    private readonly AppStatusDataServcies _appStatusServcies;
    private readonly OrdersDataServcies _orderServcies;
    private readonly TransactionManager _transaction;
    private readonly TransactionCache _transactionCache;

    private readonly SupplierDataServcies _supplierDataServcies;
    private readonly IUserCacheService _userCacheService;
    private readonly CurrencyRateFetcher _currencyRateFetcher;
    private readonly CacheUpdater<Order> _cacheUpdater;
    private readonly OrderCache _cache;
    private readonly ProxyHttpClientService _proxyHttpClientService;
    private readonly IFileProcessingService _fileProcessingService;

    public PercentageController(OzonOrderContext context,
                                                AppStatusDataServcies appStatusServcies,
                                                OrdersDataServcies orderServcies,
                                                TransactionManager transaction,
                                                TransactionCache transactionCache,
                                                SupplierDataServcies supplierDataServcies,
                                                IUserCacheService userCacheService,
                                                CurrencyRateFetcher currencyRateFetcher,
                                                CacheUpdater<Order> cacheUpdater,
                                                OrderCache cache,
                                                ProxyHttpClientService proxyHttpClientService,
                                                IFileProcessingService fileProcessingService)
    {
        _context = context;
        _appStatusServcies = appStatusServcies;
        _orderServcies = orderServcies;
        _transaction = transaction;
        _transactionCache = transactionCache;

        _supplierDataServcies = supplierDataServcies;
        _userCacheService = userCacheService;
        _currencyRateFetcher = currencyRateFetcher;
        _cacheUpdater = cacheUpdater;
        _cache = cache;
        _proxyHttpClientService = proxyHttpClientService;
        _fileProcessingService = fileProcessingService;
    }

    public IActionResult UploadFile()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile excelFile, TransactionType selectedTransactionType)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["Error"] = "Файл не выбран или пуст";
            return RedirectToAction("UploadFile");
        }

        List<(string OrderNumber, string Article)> processedOrders;

        try
        {
            processedOrders = await _fileProcessingService.ProcessFileAsync(excelFile);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при обработке файла: {ex.Message}";
            return RedirectToAction("UploadFile");
        }

        if (!processedOrders.Any())
        {
            TempData["Error"] = "Файл не содержит подходящих данных";
            return RedirectToAction("UploadFile");
        }

        var ids = await _orderServcies.GetOrderIdsByNumbersAndArticles(processedOrders);
        if (!ids.Any())
        {
            TempData["Success"] = $"Обработано {processedOrders.Count} записей, заказов не найдено.";
            return RedirectToAction("UploadFile");
        }

        var orderIds = string.Join(",", ids);

        switch (selectedTransactionType)
        {
            case TransactionType.ShippedToClient:
                return RedirectToAction(
                    "CreateShippedToClientTransactionFromFile",
                    "ShippedToClientTransaction",      
                    new { area = "Studio2", ids = orderIds, page = 1 }
                );

            case TransactionType.Percentage:
                return RedirectToAction("Percentage", new { ids = orderIds });

            default:
                TempData["Error"] = "Неизвестный тип документа";
                return RedirectToAction("UploadFile");
        }
    }
    // GET
    public async Task<IActionResult> Percentage(string ids, int page)
    {
        var filteredStatuses = _context.AppStatuses.Where(status => status.Name == "Отменен").ToList();
        ViewBag.AppStatuses = new SelectList(filteredStatuses, "Id", "Name");
        ViewBag.StatusColors = filteredStatuses.ToDictionary(s => s.Id.ToString(), s => s.GetStatusColor());

        int[] idArray = ids.Split(',').Select(int.Parse).ToArray();
        List<Order> ordersToTransaction = new List<Order>();
        
        foreach (var orderId in idArray)
        {
            var concretOrder = await _orderServcies.GetOrder(orderId);
            concretOrder.UpdatedBy = User.Identity?.Name;
            ordersToTransaction.Add(concretOrder);
        }

        var pageViewModel = new MultiplayEditOrderViewModel()
        {
            RedirectPage = page,
            Orders = ordersToTransaction.OrderBy(ot => ot.ShipmentNumber).ToList(),
            User = await _userCacheService.GetCachedUserAsync(User),
            Suppliers = (await _supplierDataServcies.GetSuppliers()).OrderBy(s => s.Name).ToList(),
            RateEUR = await _currencyRateFetcher.GetEURRateAsync(),
            RateUSD = await _currencyRateFetcher.GetUSDRateAsync(),
            RateBYN = await _currencyRateFetcher.GetBYNRateAsync(),
            UniqueArticles = await _orderServcies.GetUniqueArticles(),
            UniqueDeliveryCitys = await _orderServcies.GetUniqueDeliveryCities(),
            UniqueNumbers = await _orderServcies.GetUniqueShipmentNumbers(),
            AppStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus() { Name = "Заказан поставщику" })
        };

        if (pageViewModel.User.UserAccessId != null)
        {
            pageViewModel.User.UserAccess = _context.UserAccess.Find(pageViewModel.User.UserAccessId);
        }

        RunBackgroundCacheTask(pageViewModel.Orders);
        return View(pageViewModel);
    }

    // фоновой прогрев кэша
    private void RunBackgroundCacheTask(List<Order> orders)
    {
        _ = Task.Run(async () =>
        {
            foreach (var order in orders)
            {
                try
                {
                    _ = await _proxyHttpClientService.GetJsonAsync(
                        $"https://api.interparts.ru/detail-full-info/{order.EtProducer.Name}/{order.Article}");

                    _ = await _proxyHttpClientService.GetJsonAsync(
                        $"https://api.interparts.ru/product-information/product/?article_number={order.Article}&manufacturer={order.EtProducer.Name}");

                    Console.WriteLine($"Кэширован для проведения: {order.EtProducer.Name}/{order.Article}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в фоне: {ex.Message}");
                }
            }
        });
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Percentage(
        List<Order> orders,
        string userName,
        string comment,
        int page,
        string deletedOrders)
    {
        if (orders == null)
        {
            TempData["ErorrResult"] = $"Не удалось провести выбранные заказы.<br>Было передано слишком большое количество записей.";
            return RedirectToAction("Index");
        }
        else
        {
            if (deletedOrders != null)
            {
                var deletedOrderIds = deletedOrders.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                orders = orders.Where(o => !deletedOrderIds.Contains(o.Id)).ToList();
            }
        }

        try
        {
            if (orders.Count != 0)
            {
                DateTime createAt = DateTime.Now;
                var changeCount = 0;
                var dateTime = "";
                List<Order> ordersToUpdate = new List<Order>();
                List<string> ordersNotFoundInOzone = [];

                foreach (var order in orders)
                {
                    ordersToUpdate.Add(await _orderServcies.TransactOrder(order));
                }
                (changeCount, dateTime) = await _transaction.CreatePercentageTransaction(ordersToUpdate,
                                                                                              userName,
                                                                                              createAt,
                                                                                              comment);
                await _transactionCache.Update();
                if (ordersNotFoundInOzone.Count > 0)
                {
                    TempData["OrdersNotFoundInOzone"] = $"Заказы не найдены в системе Ozon - {ordersNotFoundInOzone.Aggregate((x, y) => x + ", " + y)}";
                }
            }

            ClearSelectedIdsSession();

            foreach (var order in orders)
            {
                var cookieKey = $"PurchasePrice_{order.Id}";
                if (Request.Cookies.ContainsKey(cookieKey))
                {
                    Response.Cookies.Delete(cookieKey);
                }
            }

            return Json(new { redirectTo = Url.Action("Index", "Orders", new { sortOrder = GetSortStateCookie(), page = page }) });
        }
        catch (Exception ex)
        {
            TempData["ErorrResult"] = $"Не удалось провести выбранные заказы ({ex.Message})";
            return Json(new { redirectTo = Url.Action("Index", "Orders", new { sortOrder = GetSortStateCookie(), page = page }) });
        }
    }

    [HttpPost]
    public IActionResult ClearSelectedIdsSession()
    {
        HttpContext.Session.SetString("selectedIds", JsonConvert.SerializeObject(new List<int>()));
        return Ok();
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