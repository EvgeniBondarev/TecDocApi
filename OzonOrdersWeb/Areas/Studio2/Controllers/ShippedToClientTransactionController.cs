using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.OrderViewModels;
using OzonRepositories.Context;
using OzonRepositories.Data.Bitrix;
using PartsInfo.HttpUtils;
using Servcies.ApiServcies._1CApi;
using Servcies.ApiServcies._1CApi.Models;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.CacheServcies.Cache.UserCacheService;
using Servcies.DataServcies;
using Servcies.FiltersServcies.SortModels;
using Servcies.PriceСlculationServcies;
using Servcies.SignalRServcies;
using Servcies.TransactionUtilsServcies;
using Services.CacheServcies.Cache;


[Authorize(Roles = "User,Admin")]
[Area("Studio2")]
public class ShippedToClientTransactionController : Controller
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
    private readonly OneCTransferManager _oneCTransferManager;

    public ShippedToClientTransactionController(
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
        ProxyHttpClientService proxyHttpClientService,
        OneCTransferManager oneCTransferManager)
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
        _oneCTransferManager = oneCTransferManager;
    }

    // ==============================
    // GET: CreateShippedToClientTransaction
    // ==============================
    public async Task<IActionResult> CreateShippedToClientTransaction(string ids, int page)
    {
        var filteredStatuses = _context.AppStatuses.Where(s => s.Name == "Отменен").ToList();
        ViewBag.AppStatuses = new SelectList(filteredStatuses, "Id", "Name");
        ViewBag.StatusColors = filteredStatuses.ToDictionary(s => s.Id.ToString(), s => s.GetStatusColor());

        int[] idArray = ids.Split(',').Select(int.Parse).ToArray();
        List<Order> ordersToTransaction = new();
        string errorReason = null;

        var appStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus { Name = "Отгружен клиенту" });
        if (appStatus == null)
        {
            appStatus = new AppStatus { Name = "Отгружен клиенту" };
            _context.AppStatuses.Add(appStatus);
            await _context.SaveChangesAsync();
        }

        foreach (var orderId in idArray)
        {
            var order = await _orderServcies.GetOrder(orderId);
            if (order.AppStatus?.Name == "Заказан поставщику")
            {
                order.AppStatus = appStatus;
                order.UpdatedBy = User.Identity?.Name;
                ordersToTransaction.Add(order);
            }
            else
            {
                errorReason = $"Заказ {order.ShipmentNumber} имеет статус '{order.AppStatus?.Name}', а должен быть 'Заказан поставщику'.";
            }
        }

        if (!ordersToTransaction.Any())
        {
            errorReason ??= "Нет заказов со статусом 'Заказан клиенту'.";
        }
        else
        {
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
    // POST: CreateShippedToClientTransaction
    // ==============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateShippedToClientTransaction(
            List<Order> orders,
            string userName,
            string comment,
            int page,
            string deletedOrders,
            bool processIn1C = false)
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

                    var appStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus() { Name = "Отгружен клиенту" });
                    var oldStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus() { Name = "Заказан поставщику" });
                    var backStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus() { Name = "Утерян поставщиком" });
                    if (backStatus == null)
                    {
                        AppStatus newStatus = new AppStatus()
                        {
                            Name = "Утерян поставщиком"
                        };
                        _context.AppStatuses.Add(newStatus);
                        await _context.SaveChangesAsync();
                        backStatus = newStatus;
                    }
                    List<Order> ordersToTransaction = ordersToUpdate.Where(o => o.AppStatus.Id == appStatus.Id && o.Quantity > 0).ToList();
                    List<Order> ordersToBack = ordersToUpdate.Where(o => o.Quantity <= 0).ToList();
                    foreach (var order in ordersToBack)
                    {
                        order.AppStatus = backStatus;
                    }
                    int ordersToBackResult = await _orderServcies.UpdateOrders(ordersToBack);
                    

                    string oneCResult = "";
                    IEnumerable<string> oneCHash = [];
                    List<OneCResponse> transferResult = new List<OneCResponse>();
                    if (processIn1C)
                    {
                        try
                        {
                            transferResult = await _oneCTransferManager.TransferStock(ordersToTransaction);
                        }
                        catch (Exception e)
                        {
                            string msg = $"<div class='alert alert-danger'>" +
                                         $"<strong>⚠️ Ошибка обработки в 1С</strong><br/>" +
                                         $"{e.Message}</div>";
                            foreach (var item in ordersToTransaction)
                            {
                                item.AppStatus = oldStatus;
                            }
                            int ordersToOldResult = await _orderServcies.UpdateOrders(ordersToTransaction);
                            if (ordersToOldResult > 0)
                            {
                                msg += $"<div class='alert alert-info'>" +
                                       $"<strong>📊 Обновление статусов</strong><br/>" +
                                       $"Статус 'Заказан поставщику' установлен для <strong>{ordersToOldResult}</strong> заказов" +
                                       $"</div>";
                            }
                            TempData["TransactionResult"] = msg;
                            return Json(new { redirectTo = Url.Action("Index", "Orders", new { sortOrder = GetSortStateCookie(), page }) });
                        }
                        
                        var successfulTransfers = transferResult.Where(tr => tr.Success).ToList();
                        var failedTransfers = transferResult.Where(tr => !tr.Success).ToList();

                        if (failedTransfers.Count > 0)
                        {
                            // Есть ошибки
                            var errorMessages = failedTransfers.Select(tr => tr.Message).Distinct().Take(3);
                            var errorCount = failedTransfers.Count;
                            var successCount = successfulTransfers.Count;
                            var totalCount = transferResult.Count;

                            string msg = $"<div class='alert alert-warning'>" +
                                         $"<strong>⚠️ Частичный результат обработки в 1С</strong><br/>" +
                                         $"Успешно: {successCount} из {totalCount} документов<br/>" +
                                         $"С ошибками: {errorCount} документов<br/>" +
                                         $"<small>Ошибки: {string.Join("; ", errorMessages)}" +
                                         (errorCount > 3 ? "..." : "") + "</small>" +
                                         "</div>";

                            
                            
                            foreach (var item in ordersToTransaction)
                            {
                                item.AppStatus = oldStatus;
                            }
                            int ordersToOldResult = await _orderServcies.UpdateOrders(ordersToTransaction);
                            if (ordersToOldResult > 0)
                            {
                                msg += $"<div class='alert alert-info'>" +
                                              $"<strong>📊 Обновление статусов</strong><br/>" +
                                              $"Статус 'Заказан поставщику' установлен для <strong>{ordersToOldResult}</strong> заказов" +
                                              $"</div>";
                            }
                            TempData["TransactionResult"] = msg;
                            return Json(new { redirectTo = Url.Action("Index", "Orders", new { sortOrder = GetSortStateCookie(), page }) });
                        }
                        else
                        {
                            oneCHash = successfulTransfers.Select(h => h.TransactionId);
                            var documentCount = successfulTransfers.Count;
        
                            oneCResult = $"<div class='alert alert-success'>" +
                                         $"<strong>✅ Успешно создано в 1С</strong><br/>" +
                                         $"Количество документов: {documentCount}<br/>" +
                                         $"Номера транзакций: <code>{string.Join(", ", oneCHash)}</code>" +
                                         $"</div>";
                        }
                    }
                    else
                    {
                        oneCResult = "<div class='alert alert-info'><strong>ℹ️ Информация</strong><br/>Создание документов в 1С не требовалось</div>";
                    }

                    if (ordersToBackResult > 0)
                    {
                        oneCResult += $"<div class='alert alert-info'>" +
                                      $"<strong>📊 Обновление статусов</strong><br/>" +
                                      $"Статус 'Утерян реализатором' установлен для <strong>{ordersToBackResult}</strong> заказов" +
                                      $"</div>";
                    }
                    await NotificationService.NotifyAllAsync(oneCResult);
                    (changeCount, dateTime) = await _transaction.CreateShippedToClientTransaction(ordersToTransaction,
                                                                                                  userName,
                                                                                                  createAt,
                                                                                                  comment + string.Join(", ", oneCHash));
                    int cancelCount = ordersToUpdate.Where(o => o.AppStatus.Name == "Отменен").Count();

                    if (changeCount > 0)
                    {   
                        await _cacheUpdater.Update(_cache);
                        string msg = $"Заказы добавлены в журнал<br/>" +
                                     $"<b>{changeCount}</b> заказам изменен статус на '<b>Отгружен клиенту</b>', <b>{cancelCount}</b> заказов были отменены." +
                                     $"<br/>Время<b>: {dateTime}</b>" +
                                     $"<br/>Пользователь<b>: {userName}</b>" +
                                     $"<br/>Комментарий: {comment}" +
                                     $"<br/>Заказы: {string.Join(" ", orders.Select(o => o.ShipmentNumber))}"+
                                     $"<br/>{oneCResult}";
                        await NotificationService.NotifyAllAsync(msg);
                        TempData["TransactionResult"] = msg;

                    }
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
                
                return Json(new { redirectTo = Url.Action("Index", "Orders", new { sortOrder = GetSortStateCookie(), page }) });
            }
            catch (Exception ex)
            {
                TempData["ErorrResult"] = $"Не удалось провести выбранные заказы ({ex.Message})";
                return Json(new { redirectTo = Url.Action("Index", "Orders", new { sortOrder = GetSortStateCookie(), page }) });

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
