using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.OrderViewModels;
using OzonRepositories.Context;
using OzonRepositories.Data.Bitrix;
using PartsInfo.HttpUtils;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.CacheServcies.Cache.UserCacheService;
using Servcies.DataServcies;
using Servcies.FiltersServcies.SortModels;
using Servcies.PriceСlculationServcies;
using Servcies.SignalRServcies;
using Servcies.TransactionUtilsServcies;
using Services.CacheServcies.Cache;

namespace OzonOrdersWeb.Areas.Studio2.Controllers.Transactions;

[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
public class ShippedBySupplierTransactionController : Controller
{
    private readonly OzonOrderContext _context;
    private readonly AppStatusDataServcies _appStatusServcies;
    private readonly OrdersDataServcies _orderServcies;
    private readonly TransactionManager _transaction;
    private readonly TransactionCache _transactionCache;

    private readonly SupplierDataServcies _supplierDataServcies;
    private readonly IUserCacheService _userCacheService;
    private readonly CurrencyRateFetcher _currencyRateFetcher;
    private readonly BitrixStockRepository _bitrixStockRepository;
    private readonly CacheUpdater<Order> _cacheUpdater;
    private readonly OrderCache _cache;
    private readonly ProxyHttpClientService _proxyHttpClientService;

    public ShippedBySupplierTransactionController(
        OzonOrderContext context,
        AppStatusDataServcies appStatusServcies,
        OrdersDataServcies orderServcies,
        TransactionManager transaction,
        TransactionCache transactionCache,
        SupplierDataServcies supplierDataServcies,
        IUserCacheService userCacheService,
        CurrencyRateFetcher currencyRateFetcher,
        BitrixStockRepository bitrixStockRepository,
        CacheUpdater<Order> cacheUpdater,
        OrderCache cache,
        ProxyHttpClientService proxyHttpClientService)
    {
        _context = context;
        _appStatusServcies = appStatusServcies;
        _orderServcies = orderServcies;
        _transaction = transaction;
        _transactionCache = transactionCache;
        _supplierDataServcies = supplierDataServcies;
        _userCacheService = userCacheService;
        _currencyRateFetcher = currencyRateFetcher;
        _bitrixStockRepository = bitrixStockRepository;
        _cacheUpdater = cacheUpdater;
        _cache = cache;
        _proxyHttpClientService = proxyHttpClientService;
    }

    // ==============================
    // GET: CreateShippedBySupplierTransaction
    // ==============================
    public async Task<IActionResult> CreateShippedBySupplierTransaction(string ids, int page)
    {
        var filteredStatuses = _context.AppStatuses.Where(s => s.Name == "Отменен").ToList();
        ViewBag.AppStatuses = new SelectList(filteredStatuses, "Id", "Name");
        ViewBag.StatusColors = filteredStatuses.ToDictionary(s => s.Id.ToString(), s => s.GetStatusColor());

        int[] idArray = ids.Split(',').Select(int.Parse).ToArray();
        List<Order> ordersToTransaction = new();
        string errorReason = null;

        var appStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus { Name = "Отгружен поставщиком" });
        if (appStatus == null)
        {
            appStatus = new AppStatus { Name = "Отгружен поставщиком" };
            _context.AppStatuses.Add(appStatus);
            await _context.SaveChangesAsync();
        }

        foreach (var orderId in idArray)
        {
            var order = await _orderServcies.GetOrder(orderId);
            if (order.AppStatus?.Name == "Заказан поставщику")
            {
                order.AppStatus = appStatus;
                ordersToTransaction.Add(order);
            }
            else
            {
                errorReason = $"Заказ {order.ShipmentNumber} имеет статус '{order.AppStatus?.Name}', а должен быть 'Заказан поставщику'.";
            }
        }

        if (!ordersToTransaction.Any())
        {
            errorReason ??= "Нет заказов со статусом 'Заказан поставщику'.";
        }
        else
        {
            var distinctSuppliers = ordersToTransaction.GroupBy(o => o.Supplier?.Name).ToList();
            if (distinctSuppliers.Count > 1)
            {
                errorReason = "Выбраны заказы с разными поставщиками: " +
                              string.Join(", ", distinctSuppliers.SelectMany(g => g.Select(o => $"{o.ShipmentNumber} (поставщик: {g.Key})")));
                ordersToTransaction.Clear();
            }

            var distinctWarehouses = ordersToTransaction.GroupBy(o => o.ShipmentWarehouse?.Name).ToList();
            if (distinctWarehouses.Count > 1)
            {
                errorReason = "Выбраны заказы с разными складами: " +
                              string.Join(", ", distinctWarehouses.SelectMany(g => g.Select(o => $"{o.ShipmentNumber} (склад: {g.Key})")));
                ordersToTransaction.Clear();
            }
        }

        ViewData["ErrorReason"] = errorReason;

        var pageViewModel = new MultiplayEditOrderViewModel
        {
            RedirectPage = page,
            Orders = ordersToTransaction.OrderBy(o => o.ShipmentNumber).ToList(),
            User = await _userCacheService.GetCachedUserAsync(User),
            Suppliers = (await _supplierDataServcies.GetSuppliers()).OrderBy(s => s.Name).ToList(),
            RateEUR = await _currencyRateFetcher.GetEURRateAsync(),
            RateUSD = await _currencyRateFetcher.GetUSDRateAsync(),
            RateBYN = await _currencyRateFetcher.GetBYNRateAsync(),
            UniqueArticles = await _orderServcies.GetUniqueArticles(),
            UniqueDeliveryCitys = await _orderServcies.GetUniqueDeliveryCities(),
            UniqueNumbers = await _orderServcies.GetUniqueShipmentNumbers(),
            AppStatus = appStatus
        };

        if (pageViewModel.User.UserAccessId != null)
            pageViewModel.User.UserAccess = _context.UserAccess.Find(pageViewModel.User.UserAccessId);

        RunBackgroundCacheTask(pageViewModel.Orders);
        return View(pageViewModel);
    }

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

    // ==============================
    // POST: CreateShippedBySupplierTransaction
    // ==============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateShippedBySupplierTransaction(
        List<Order> orders,
        string userName,
        string comment,
        int page,
        string deletedOrders)
    {
        if (orders == null)
        {
            TempData["ErorrResult"] = "Не удалось провести выбранные заказы.<br>Было передано слишком большое количество записей.";
            return RedirectToAction("Index");
        }

        if (!string.IsNullOrEmpty(deletedOrders))
        {
            var deletedIds = deletedOrders.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            orders = orders.Where(o => !deletedIds.Contains(o.Id)).ToList();
        }

        try
        {
            if (orders.Count > 0)
            {
                DateTime createAt = DateTime.Now;
                int changeCount = 0;
                string dateTime = "";
                List<Order> ordersToUpdate = new();
                List<string> notFoundInOzon = new();

                foreach (var order in orders)
                    ordersToUpdate.Add(await _orderServcies.TransactOrder(order));

                var appStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus { Name = "Отгружен поставщиком" });
                var ordersToTransaction = ordersToUpdate.Where(o => o.AppStatus.Id == appStatus.Id).ToList();

                (changeCount, dateTime) = await _transaction.CreateShippedBySupplierTransaction(
                    ordersToTransaction, userName, createAt, comment);

                int cancelCount = ordersToUpdate.Count(o => o.AppStatus.Name == "Отменен");

                if (changeCount > 0)
                {
                    await _cacheUpdater.Update(_cache);
                    string msg = $"Заказы добавлены в журнал<br/>" +
                                 $"<b>{changeCount}</b> заказам изменен статус на '<b>Отгружен поставщиком</b>', <b>{cancelCount}</b> заказов были отменены." +
                                 $"<br/>Время<b>: {dateTime}</b>" +
                                 $"<br/>Пользователь<b>: {userName}</b>" +
                                 $"<br/>Комментарий: {comment}" +
                                 $"<br/>Заказы: {string.Join(" ", orders.Select(o => o.ShipmentNumber))}";
                    await NotificationService.NotifyAllAsync(msg);
                    TempData["TransactionResult"] = msg;
                }

                await _transactionCache.Update();

                if (notFoundInOzon.Any())
                    TempData["OrdersNotFoundInOzone"] = "Заказы не найдены в системе Ozon - " + string.Join(", ", notFoundInOzon);
            }

            ClearSelectedIdsSession();

            foreach (var order in orders)
            {
                var cookieKey = $"PurchasePrice_{order.Id}";
                if (Request.Cookies.ContainsKey(cookieKey))
                    Response.Cookies.Delete(cookieKey);
            }

            return Json(new { redirectTo = Url.Action("IndexV2", "Orders", new { sortOrder = GetSortStateCookie(), page }) });
        }
        catch (Exception ex)
        {
            TempData["ErorrResult"] = $"Не удалось провести выбранные заказы ({ex.Message})";
            return Json(new { redirectTo = Url.Action("IndexV2", "Orders", new { sortOrder = GetSortStateCookie(), page }) });
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
        var cookie = Request.Cookies["SortState"];
        if (!string.IsNullOrEmpty(cookie) && Enum.TryParse<OrderSortState>(cookie, out var saved))
            return saved;
        return OrderSortState.StandardState;
    }
}
