using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.OrderViewModels;
using OzonRepositories.Context;
using OzonRepositories.Data.Bitrix;
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
public class OrderedToSellerTransactionController : Controller
{
    private readonly OzonOrderContext _context;
    private readonly AppStatusDataServcies _appStatusServcies;
    private readonly OrdersDataServcies _orderServcies;
    private readonly TransactionManager _transaction;
    private readonly TransactionCache _transactionCache;

    // 🔹 добавляем недостающие зависимости
    private readonly SupplierDataServcies _supplierDataServcies;
    private readonly IUserCacheService _userCacheService;
    private readonly CurrencyRateFetcher _currencyRateFetcher;
    private readonly BitrixStockRepository _bitrixStockRepository;
    private readonly CacheUpdater<Order> _cacheUpdater;
    private readonly OrderCache _cache;

    public OrderedToSellerTransactionController(OzonOrderContext context,  
                                                AppStatusDataServcies appStatusServcies,
                                                OrdersDataServcies orderServcies,
                                                TransactionManager transaction,
                                                TransactionCache transactionCache,
                                                SupplierDataServcies supplierDataServcies,
                                                IUserCacheService userCacheService,
                                                CurrencyRateFetcher currencyRateFetcher,
                                                BitrixStockRepository bitrixStockRepository,
                                                CacheUpdater<Order> cacheUpdater,
                                                OrderCache cache)
    {
        _context = context;
        _appStatusServcies = appStatusServcies;
        _orderServcies = orderServcies;
        _transaction = transaction;
        _transactionCache = transactionCache;

        // 🔹 инициализация новых зависимостей
        _supplierDataServcies = supplierDataServcies;
        _userCacheService = userCacheService;
        _currencyRateFetcher = currencyRateFetcher;
        _bitrixStockRepository = bitrixStockRepository;
        _cacheUpdater = cacheUpdater;
        _cache = cache;
    }
    
    public async Task<IActionResult> CreateOrderedToSellerTransaction(string ids, int page)
    {
            var filteredStatuses = _context.AppStatuses.Where(status => status.Name == "Отменен").ToList();
            ViewBag.AppStatuses = new SelectList(filteredStatuses, "Id", "Name");
            ViewBag.StatusColors = filteredStatuses.ToDictionary(s => s.Id.ToString(), s => s.GetStatusColor());

            int[] idArray = ids.Split(',').Select(int.Parse).ToArray();
            List<Order> ordersToTransaction = new List<Order>();


            var appStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus() { Name = "Заказан реализатору" });

            if (appStatus == null)
            {
                AppStatus newStatus = new AppStatus()
                {
                    Name = "Заказан реализатору"
                };
                _context.AppStatuses.Add(newStatus);
                await _context.SaveChangesAsync();
            }


            foreach (var orderId in idArray)
            {
                var concretOrder = await _orderServcies.GetOrder(orderId);
                bool isTransaction = (concretOrder.Status != "Отменен покупателем");
                bool isTransaction2 = (concretOrder.AppStatus?.Name == "Не указан" && concretOrder.Status == "Отменён");
                bool isTransaction3 = (concretOrder.AppStatus?.Name == "Отменен" && concretOrder.Status == "Отменён");

                if (isTransaction && !isTransaction2 && !isTransaction3)
                {
                    concretOrder.AppStatus = appStatus;
                    ordersToTransaction.Add(concretOrder);
                }

            }
            
            await _bitrixStockRepository.SetPricesForOrdersAsyncFromBitrix(ordersToTransaction); 
            var stockData = await _bitrixStockRepository.SetStocksForOrdersAsyncFromBitrix(ordersToTransaction);
            ViewData["StockData"] = stockData;

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
                AppStatus =  await _appStatusServcies.GetAppStatusAsync(new AppStatus() { Name = "Заказан реализатору" })
            };

            if (pageViewModel.User.UserAccessId != null)
            {
                pageViewModel.User.UserAccess = _context.UserAccess.Find(pageViewModel.User.UserAccessId);
            }
            return View(pageViewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrderedToSellerTransaction(
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

                    var appStatus = await _appStatusServcies.GetAppStatusAsync(new AppStatus() { Name = "Заказан реализатору" });
                    List<Order> ordersToTransaction = ordersToUpdate.Where(o => o.AppStatus.Id == appStatus.Id).ToList();

                    (changeCount, dateTime) = await _transaction.CreateOrderedToSellerTransaction(ordersToTransaction,
                                                                                                  userName,
                                                                                                  createAt,
                                                                                                  comment);
                    int cancelCount = ordersToUpdate.Where(o => o.AppStatus.Name == "Отменен").Count();

                    if (changeCount > 0)
                    {
                        await _cacheUpdater.Update(_cache);
                        string msg = $"Заказы добавлены в журнал<br/>" +
                                     $"<b>{changeCount}</b> заказам изменен статус на '<b>Заказан реализатору</b>', <b>{cancelCount}</b> заказов были отменены." +
                                     $"<br/>Время<b>: {dateTime}</b>" +
                                     $"<br/>Пользователь<b>: {userName}</b>" +
                                     $"<br/>Комментарий: {comment}" +
                                     $"<br/>Заказы: {string.Join(" ", orders.Select(o => o.ShipmentNumber))}";
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
                return Json(new
                {
                    redirectTo = Url.Action("IndexV2", "Orders", new { sortOrder = GetSortStateCookie(), page = page })
                });
            }
            catch (Exception ex)
            {
                TempData["ErorrResult"] = $"Не удалось провести выбранные заказы ({ex.Message})";
                return Json(new
                {
                    redirectTo = Url.Action("IndexV2", "Orders", new { sortOrder = GetSortStateCookie(), page = page })
                });

            }
        }
        
        [HttpPost]
        public IActionResult ClearSelectedIdsSession()
        {
            // Заменяем список ID на пустой
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