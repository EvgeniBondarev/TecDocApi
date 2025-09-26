using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OServcies.FiltersServcies.FilterModels;
using OzonDomains;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.TransactionViewModels;
using OzonRepositories.Context;
using OzonRepositories.Context.Identity;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.CacheServcies.Cache.UniqueValuesCache;
using Servcies.CacheServcies.Cache.UserCacheService;
using Servcies.DataServcies;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.FiltersServcies.FilterModels;
using Servcies.FiltersServcies.SortModels;
using Servcies.ParserServcies.FielParsers;
using Services.CacheServcies.Cache;
using Services.CacheServcies.Cache.OzonOrdersCache;
using System;

namespace OzonOrdersWeb.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Area("Studio2")]
    public class TransactionController : Controller
    {
        private readonly TransactionDataServcies _transactionDataServcies;
        private readonly OrdersDataServcies _orderServcies;
        private readonly TransactionCache _cache;
        private readonly OrderCache _orderCache;
        private readonly TransactionDataFilterManager _dataFilterManager;
        private readonly OrderDataFilterManager _orderDataFilterManager;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly AppStatusDataServcies _appStatusServcies;
        private readonly SupplierDataServcies _supplierDataServcies;
        private readonly CacheUpdater<Transaction> _cacheUpdater;
        private readonly OzonClientServcies _ozonClientServcies;
        private readonly WarehouseDataServcies _warehouseDataServcies;
        private readonly OzonOrderContext _context;
        private readonly ExcelExporter _excelExporter;
        private readonly IUniqueValuesCache _uniqueValuesCache;
        private readonly IUserCacheService _userCacheService;
        private readonly PdfService _pdfService;

        public TransactionController(TransactionDataServcies transactionDataServcies,
                                    OrdersDataServcies ordersDataServcies,
                                    TransactionCache cache,
                                    OrderCache orderCache,
                                    TransactionDataFilterManager dataFilterManager,
                                    UserManager<CustomIdentityUser> userManager,
                                    AppStatusDataServcies appStatusDataServcies,
                                    SupplierDataServcies supplierDataServcies,
                                    OrderDataFilterManager orderDataFilterManager,
                                    CacheUpdater<Transaction> cacheUpdater,
                                    OzonClientServcies ozonClientServcies,
                                    WarehouseDataServcies warehouseDataServcies,
                                    OzonOrderContext ozonOrderContext,
                                    ExcelExporter excelExporter,
                                    IUniqueValuesCache uniqueValuesCache,
                                    IUserCacheService userCacheService,
                                    PdfService pdfService
                                    )
        {
            _transactionDataServcies = transactionDataServcies;
            _orderServcies = ordersDataServcies;
            _cache = cache;
            _orderCache = orderCache;
            _dataFilterManager = dataFilterManager;
            _userManager = userManager;
            _appStatusServcies = appStatusDataServcies;
            _supplierDataServcies = supplierDataServcies;
            _orderDataFilterManager = orderDataFilterManager;
            _cacheUpdater = cacheUpdater;
            _ozonClientServcies = ozonClientServcies;
            _warehouseDataServcies = warehouseDataServcies;
            _context = ozonOrderContext;
            _excelExporter = excelExporter;
            _uniqueValuesCache = uniqueValuesCache;
            _userCacheService = userCacheService;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index(OrderSortState sortOrder = OrderSortState.StandardState, int page = 1)
        {
            SaveSortStateCookie(sortOrder);

            var transactions = await _cache.Get(page);
            var orderData = await _orderCache.Get(page);

            ViewBag.AppStatuses = new SelectList((await _appStatusServcies.GetAppStatuses()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Suppliers = new SelectList((await _supplierDataServcies.GetSuppliers()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Clients = new SelectList((await _ozonClientServcies.GetOzonClients()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Warehouses = new SelectList((await _warehouseDataServcies.GetWarehouses()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Statuses = new SelectList(orderData.Select(s => s.Status).Distinct().Select(s => new { Name = s }).ToList().OrderBy(a => a.Name), "Name", "Name");

            var filterDataString = HttpContext.Request.Cookies["TransactionFilterData"];
            var filterData = new TransactionFilterModel();

            var orderFilterDataString = HttpContext.Request.Cookies["OrderFilterData"];
            var orderFilterData = new OrderFilterModel();

            if (!string.IsNullOrEmpty(filterDataString))
            {
                filterData = JsonConvert.DeserializeObject<TransactionFilterModel>(filterDataString);
            }

            if (!string.IsNullOrEmpty(orderFilterDataString))
            {
                orderFilterData = JsonConvert.DeserializeObject<OrderFilterModel>(orderFilterDataString);
            }
            
            if (!string.IsNullOrEmpty(filterDataString) || !string.IsNullOrEmpty(orderFilterDataString))
            {
                transactions = await _dataFilterManager.FilterByFilterData(transactions, orderFilterData, filterData);

                foreach (var trans in transactions)
                {
                    trans.Orders = await _orderDataFilterManager.FilterTransactionsOrdersByFilterData(trans.Orders.ToList(), orderFilterData);
                }

                transactions = transactions.Where(e => e.Orders.Count > 0).ToList();
            }

            if (sortOrder == OrderSortState.StandardState && GetSortStateCookie() != null)
            {
                sortOrder = GetSortStateCookie();
            }

            SetSortOrderViewData(sortOrder);

            foreach (var trans in transactions)
            {
                trans.Orders = (await ApplySortOrder(trans.Orders, sortOrder)).ToList();
            }
            

            int pageSize = Int32.TryParse(Request.Cookies["PageSize"], out var size)
                ? size
                : 15;


            var pageViewModel = new TransactionViewModel<Transaction, TransactionFilterModel>(transactions.OrderBy(tr => tr.CreatedDateTime), page, pageSize, filterData)
            {
                UniqueArticles = await _uniqueValuesCache.GetUniqueArticles(),
                UniqueDeliveryCitys = await _uniqueValuesCache.GetUniqueDeliveryCitys(),
                UniqueNumbers = await _uniqueValuesCache.GetUniqueShipmentNumbers(),

                OrderFilterModel = orderFilterData,
                UserNames = _userManager.Users.Select(u => u.UserName).ToList(),
                TransactionTypes = [(TransactionType.OrderedToSupplier, "Заказ поставщику"), 
                                    (TransactionType.ShippedBySupplier, "Отгружен поставщиком"),
                                    (TransactionType.ShippedToSeller, "Отгружен реализатору"), 
                                    (TransactionType.OrderedToSeller, "Заказан реализатору")],
                User = await _userCacheService.GetCachedUserAsync(User),
            };

            if (pageViewModel.User.UserAccessId != null)
            {
                pageViewModel.User.UserAccess = _context.UserAccess.Find(pageViewModel.User.UserAccessId);
            }

            ViewData["DisplayName"] = pageViewModel.TransactionTypes.FirstOrDefault(t => t.Item1 == filterData.Type).Item2;

            return View(pageViewModel);
        }

        public async Task<IActionResult> GetTransaction(int transactionId)
        {
            List<Transaction> transactions = [await _transactionDataServcies.GetTransaction(transactionId)];
            var orderData = await _orderCache.Get();
            ViewBag.AppStatuses = new SelectList((await _appStatusServcies.GetAppStatuses()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Suppliers = new SelectList((await _supplierDataServcies.GetSuppliers()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Clients = new SelectList((await _ozonClientServcies.GetOzonClients()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Warehouses = new SelectList((await _warehouseDataServcies.GetWarehouses()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Statuses = new SelectList(orderData.Select(s => s.Status).Distinct().Select(s => new { Name = s }).ToList().OrderBy(a => a.Name), "Name", "Name");

            int pageSize = Int32.TryParse(Request.Cookies["PageSize"], out var size)
                ? size
                : 15;

            var pageViewModel = new TransactionViewModel<Transaction, TransactionFilterModel>(transactions.OrderBy(tr => tr.CreatedDateTime), 1, pageSize, new TransactionFilterModel())
            {
                UniqueArticles = await _uniqueValuesCache.GetUniqueArticles(),
                UniqueDeliveryCitys = await _uniqueValuesCache.GetUniqueDeliveryCitys(),
                UniqueNumbers = await _uniqueValuesCache.GetUniqueShipmentNumbers(),

                OrderFilterModel = new OrderFilterModel(),
                UserNames = _userManager.Users.Select(u => u.UserName).ToList(),
                TransactionTypes = [(TransactionType.OrderedToSupplier, "Заказ поставщику"),
                            (TransactionType.ShippedBySupplier, "Отгружен постащиком")],
                User = await _userCacheService.GetCachedUserAsync(User),
            };

            if (pageViewModel.User.UserAccessId != null)
            {
                pageViewModel.User.UserAccess = _context.UserAccess.Find(pageViewModel.User.UserAccessId);
            }
            
            return View("Index", pageViewModel);
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(TransactionFilterModel filterData, OrderFilterModel orderFilterData, int page = 1)
        {
            var transactions = await _cache.Get(page);
            var orderData = await _orderCache.Get();

            ViewBag.AppStatuses = new SelectList((await _appStatusServcies.GetAppStatuses()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Suppliers = new SelectList((await _supplierDataServcies.GetSuppliers()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Clients = new SelectList((await _ozonClientServcies.GetOzonClients()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Warehouses = new SelectList((await _warehouseDataServcies.GetWarehouses()).OrderBy(a => a.Name), "Name", "Name");
            ViewBag.Statuses = new SelectList(orderData.Select(s => s.Status).Distinct().Select(s => new { Name = s }).ToList().OrderBy(a => a.Name), "Name", "Name");


            var serializedFilterData = JsonConvert.SerializeObject(filterData);
            HttpContext.Response.Cookies.Append("TransactionFilterData", serializedFilterData);

            var serializedoOrderFilterData = JsonConvert.SerializeObject(orderFilterData);
            HttpContext.Response.Cookies.Append("OrderFilterData", serializedoOrderFilterData);

            transactions = await _dataFilterManager.FilterByFilterData(transactions, orderFilterData, filterData);

            foreach (var trans in transactions)
            {
                trans.Orders = await _orderDataFilterManager.FilterTransactionsOrdersByFilterData(trans.Orders.ToList(), orderFilterData);
            }

            transactions = transactions.Where(e => e.Orders.Count > 0).ToList();

            int pageSize = Int32.TryParse(Request.Cookies["PageSize"], out var size)
                ? size
                : 15;

            var pageViewModel = new TransactionViewModel<Transaction, TransactionFilterModel>(transactions.OrderBy(tr => tr.CreatedDateTime), page, pageSize, filterData)
            {
                UniqueArticles = await _uniqueValuesCache.GetUniqueArticles(),
                UniqueDeliveryCitys = await _uniqueValuesCache.GetUniqueDeliveryCitys(),
                UniqueNumbers = await _uniqueValuesCache.GetUniqueShipmentNumbers(),

                UserNames = _userManager.Users.Select(u => u.UserName).ToList(),
                TransactionTypes = [(TransactionType.OrderedToSupplier, "Заказ поставщику"), 
                                    (TransactionType.ShippedBySupplier, "Отгружен поставщиком"),
                                    (TransactionType.ShippedToSeller, "Отгружен реализатору"), 
                                    (TransactionType.OrderedToSeller, "Заказан реализатору")],
                User = await _userCacheService.GetCachedUserAsync(User),
            };

            if (pageViewModel.User.UserAccessId != null)
            {
                pageViewModel.User.UserAccess = _context.UserAccess.Find(pageViewModel.User.UserAccessId);
            }

            ViewData["DisplayName"] = pageViewModel.TransactionTypes.FirstOrDefault(t => t.Item1 == filterData.Type).Item2;

            return View(pageViewModel);
        }
        public async Task<IActionResult> Update()
        {
            await _cacheUpdater.Update(_cache);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PrintTransactionToExcel(int transactionId)
        {
            Transaction transaction = await _transactionDataServcies.GetTransaction(transactionId);
            if (transaction != null && transaction.Orders != null)
            {
                byte[] excelFile = _excelExporter.ExportToExcel(transaction.Orders.ToList());

                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                                      $"Transaction_{transaction.CreateBy}_{transaction.CreatedDateTime:dd.MM.yyyy}.xlsx");
            }
            return NotFound("Transaction not found");
        }
        
        public async Task<IActionResult> PrintTransactionToPdf(int transactionId)
        {
            Transaction transaction = await _transactionDataServcies.GetTransaction(transactionId);
            if (transaction != null && transaction.Orders != null)
            {
                byte[] pdfFile = _pdfService.GenerateTransactionPdf(transaction);

                string transactionTypeName = transaction.Type?.GetDisplayName() ?? "Unknown";

                return File(pdfFile, "application/pdf",
                    $"{transactionTypeName}_{transaction.CreateBy}_{transaction.CreatedDateTime:dd.MM.yyyy}.pdf");
            }
            return NotFound("Transaction not found");
        }
        
        public void SetSortOrderViewData(OrderSortState sortOrder)
        {
            ViewData["ShipmentNumberSort"] = sortOrder == OrderSortState.ShipmentNumberAsc ? OrderSortState.ShipmentNumberDesc : OrderSortState.ShipmentNumberAsc;
            ViewData["ProcessingDateSort"] = sortOrder == OrderSortState.ProcessingDateAsc ? OrderSortState.ProcessingDateDesc : OrderSortState.ProcessingDateAsc;
            ViewData["ShippingDateSort"] = sortOrder == OrderSortState.ShippingDateAsc ? OrderSortState.ShippingDateDesc : OrderSortState.ShippingDateAsc;
            ViewData["StatusSort"] = sortOrder == OrderSortState.StatusAsc ? OrderSortState.StatusDesc : OrderSortState.StatusAsc;
            ViewData["CurrentPriceSort"] = sortOrder == OrderSortState.CurrentPriceAsc ? OrderSortState.CurrentPriceDesc : OrderSortState.CurrentPriceAsc;
            ViewData["AppStatusIdSort"] = sortOrder == OrderSortState.AppStatusIdAsc ? OrderSortState.AppStatusIdDesc : OrderSortState.AppStatusIdAsc;
            ViewData["ShipmentAmountSort"] = sortOrder == OrderSortState.ShipmentAmountAsc ? OrderSortState.ShipmentAmountDesc : OrderSortState.ShipmentAmountAsc;
            ViewData["ProductNameSort"] = sortOrder == OrderSortState.ProductNameAsc ? OrderSortState.ProductNameDesc : OrderSortState.ProductNameAsc;
            ViewData["ArticleSort"] = sortOrder == OrderSortState.ArticleAsc ? OrderSortState.ArticleDesc : OrderSortState.ArticleAsc;
            ViewData["PriceSort"] = sortOrder == OrderSortState.PriceAsc ? OrderSortState.PriceDesc : OrderSortState.PriceAsc;
            ViewData["QuantitySort"] = sortOrder == OrderSortState.QuantityAsc ? OrderSortState.QuantityDesc : OrderSortState.QuantityAsc;
            ViewData["ShipmentWarehouseSort"] = sortOrder == OrderSortState.ShipmentWarehouseIdAsc ? OrderSortState.ShipmentWarehouseIdDesc : OrderSortState.ShipmentWarehouseIdAsc;
            ViewData["SupplierSort"] = sortOrder == OrderSortState.SupplierAsc ? OrderSortState.SupplierDesc : OrderSortState.SupplierAsc;
            ViewData["PurchasePriceSort"] = sortOrder == OrderSortState.PurchasePriceAsc ? OrderSortState.PurchasePriceDesc : OrderSortState.PurchasePriceAsc;
            ViewData["ProductInfoIdSort"] = sortOrder == OrderSortState.ProductInfoIdAsc ? OrderSortState.ProductInfoIdDesc : OrderSortState.ProductInfoIdAsc;
            ViewData["OzonCommissionSort"] = sortOrder == OrderSortState.MinOzonCommissionAsc ? OrderSortState.MinOzonCommissionDesc : OrderSortState.MinOzonCommissionAsc;
            ViewData["VolumeSort"] = sortOrder == OrderSortState.VolumeAsc ? OrderSortState.VolumeDesc : OrderSortState.VolumeAsc;
            ViewData["ProfitSort"] = sortOrder == OrderSortState.ProfitAsc ? OrderSortState.ProfitDesc : OrderSortState.ProfitAsc;
            ViewData["DiscountSort"] = sortOrder == OrderSortState.DiscountAsc ? OrderSortState.DiscountDesc : OrderSortState.DiscountAsc;
            ViewData["DeliveryCitySort"] = sortOrder == OrderSortState.DeliveryCityAsc ? OrderSortState.DeliveryCityDesc : OrderSortState.DeliveryCityAsc;
            ViewData["CategorySort"] = sortOrder == OrderSortState.CategoryAsc ? OrderSortState.CategoryDesc : OrderSortState.CategoryAsc;
            ViewData["OrderNumberToSupplierSort"] = sortOrder == OrderSortState.OrderNumberToSupplierAsc ? OrderSortState.OrderNumberToSupplierDesc : OrderSortState.OrderNumberToSupplierAsc;
            ViewData["OzonClient"] = sortOrder == OrderSortState.OzonClientAsc ? OrderSortState.OzonClientDesc : OrderSortState.OzonClientAsc;
            ViewData["ManufacturerSort"] = sortOrder == OrderSortState.ManufacturerAsc ? OrderSortState.ManufacturerDesc : OrderSortState.ManufacturerAsc;
            ViewData["FromFileSort"] = sortOrder == OrderSortState.FromFileAsc ? OrderSortState.FromFileDesc : OrderSortState.FromFileAsc;
            ViewData["DeliveryPeriodSort"] = sortOrder == OrderSortState.DeliveryPeriodAsc ? OrderSortState.DeliveryPeriodDesc : OrderSortState.DeliveryPeriodAsc;
            ViewData["CostPriceсеSort"] = sortOrder == OrderSortState.CostPriceAsc ? OrderSortState.CostPriceDesc : OrderSortState.CostPriceAsc;
            ViewData["TimeLeftSort"] = sortOrder == OrderSortState.TimeLeftAsc ? OrderSortState.TimeLeftDesc : OrderSortState.TimeLeftAsc;
            ViewData["LastStatusChangeDateSort"] = sortOrder == OrderSortState.LastStatusChangeDateAsc ? OrderSortState.LastStatusChangeDateDesc : OrderSortState.LastStatusChangeDateAsc;
        }
        public async Task<IEnumerable<Order>> ApplySortOrder(IEnumerable<Order> orders, OrderSortState sortOrder)
        {
            return sortOrder switch
            {
                OrderSortState.ShipmentNumberAsc => orders.OrderBy(o => o.ShipmentNumber),
                OrderSortState.ShipmentNumberDesc => orders.OrderByDescending(o => o.ShipmentNumber),

                OrderSortState.ProcessingDateAsc => orders.OrderBy(o => o.ProcessingDate),
                OrderSortState.ProcessingDateDesc => orders.OrderByDescending(o => o.ProcessingDate),

                OrderSortState.ShippingDateAsc => orders.OrderBy(o => o.ShippingDate),
                OrderSortState.ShippingDateDesc => orders.OrderByDescending(o => o.ShippingDate),

                OrderSortState.StatusAsc => orders.OrderBy(o => o.Status),
                OrderSortState.StatusDesc => orders.OrderByDescending(o => o.Status),

                OrderSortState.AppStatusIdAsc => orders.OrderBy(o => o.AppStatus.Name),
                OrderSortState.AppStatusIdDesc => orders.OrderByDescending(o => o.AppStatus.Name),

                OrderSortState.ShipmentAmountAsc => orders.OrderBy(o => o.ShipmentAmount),
                OrderSortState.ShipmentAmountDesc => orders.OrderByDescending(o => o.ShipmentAmount),

                OrderSortState.ProductNameAsc => orders.OrderBy(o => o.ProductName),
                OrderSortState.ProductNameDesc => orders.OrderByDescending(o => o.ProductName),

                OrderSortState.ArticleAsc => orders.OrderBy(o => o.ProductKey),
                OrderSortState.ArticleDesc => orders.OrderByDescending(o => o.ProductKey),

                OrderSortState.PriceAsc => orders.OrderBy(o => o.Price),
                OrderSortState.PriceDesc => orders.OrderByDescending(o => o.Price),

                OrderSortState.QuantityAsc => orders.OrderBy(o => o.Quantity),
                OrderSortState.QuantityDesc => orders.OrderByDescending(o => o.Quantity),

                OrderSortState.ShipmentWarehouseIdAsc => orders.OrderBy(o => o.ShipmentWarehouseId),
                OrderSortState.ShipmentWarehouseIdDesc => orders.OrderByDescending(o => o.ShipmentWarehouseId),

                OrderSortState.СurrencyIdAsc => orders.OrderBy(o => o.СurrencyId),
                OrderSortState.СurrencyIdDesc => orders.OrderByDescending(o => o.СurrencyId),

                OrderSortState.SupplierAsc => orders.OrderBy(o => o.Supplier.Name),
                OrderSortState.SupplierDesc => orders.OrderByDescending(o => o.Supplier.Name),

                OrderSortState.PurchasePriceAsc => orders.OrderBy(o => o.PurchasePrice),
                OrderSortState.PurchasePriceDesc => orders.OrderByDescending(o => o.PurchasePrice),

                OrderSortState.ProductInfoIdAsc => orders.OrderBy(o => o.ProductInfoId),
                OrderSortState.ProductInfoIdDesc => orders.OrderByDescending(o => o.ProductInfoId),

                OrderSortState.MinOzonCommissionAsc => orders.OrderBy(o => o.MinOzonCommission),
                OrderSortState.MinOzonCommissionDesc => orders.OrderByDescending(o => o.MinOzonCommission),

                OrderSortState.MaxOzonCommissionAsc => orders.OrderBy(o => o.MaxOzonCommission),
                OrderSortState.MaxOzonCommissionDesc => orders.OrderByDescending(o => o.MaxOzonCommission),

                OrderSortState.ProfitAsc => orders.OrderBy(o => o.MinProfit),
                OrderSortState.ProfitDesc => orders.OrderByDescending(o => o.MinProfit),

                OrderSortState.CurrentPriceAsc => orders.OrderBy(o => o.ProductInfo.CurrentPriceWithDiscount),
                OrderSortState.CurrentPriceDesc => orders.OrderByDescending(o => o.ProductInfo.CurrentPriceWithDiscount),

                OrderSortState.VolumeAsc => orders.OrderBy(o => o.ProductInfo.VolumetricWeight),
                OrderSortState.VolumeDesc => orders.OrderByDescending(o => o.ProductInfo.VolumetricWeight),

                OrderSortState.DiscountAsc => orders.OrderBy(o => o.MinDiscount),
                OrderSortState.DiscountDesc => orders.OrderByDescending(o => o.MinDiscount),

                OrderSortState.DeliveryCityAsc => orders.OrderBy(o => o.DeliveryCity),
                OrderSortState.DeliveryCityDesc => orders.OrderByDescending(o => o.DeliveryCity),

                OrderSortState.CategoryAsc => orders.OrderBy(o => o.NewCategory),
                OrderSortState.CategoryDesc => orders.OrderByDescending(o => o.NewCategory),

                OrderSortState.OrderNumberToSupplierAsc => orders.OrderBy(o => o.OrderNumberToSupplier),
                OrderSortState.OrderNumberToSupplierDesc => orders.OrderByDescending(o => o.OrderNumberToSupplier),

                OrderSortState.OzonClientAsc => orders.OrderBy(o => o.OzonClient?.Name ?? string.Empty),
                OrderSortState.OzonClientDesc => orders.OrderByDescending(o => o.OzonClient?.Name ?? string.Empty),

                OrderSortState.ManufacturerAsc => orders.OrderBy(o => o.Manufacturer?.Name ?? string.Empty),
                OrderSortState.ManufacturerDesc => orders.OrderByDescending(o => o.Manufacturer?.Name ?? string.Empty),

                OrderSortState.DeliveryPeriodAsc => orders.OrderBy(x => x.DeliveryPeriodDay),
                OrderSortState.DeliveryPeriodDesc => orders.OrderByDescending(x => x.DeliveryPeriodDay),

                OrderSortState.CostPriceAsc => orders.OrderBy(o => o.CostPrice),
                OrderSortState.CostPriceDesc => orders.OrderByDescending(o => o.CostPrice),

                OrderSortState.TimeLeftAsc => orders.OrderBy(o => o.TimeLeftDay),
                OrderSortState.TimeLeftDesc => orders.OrderByDescending(o => o.TimeLeftDay),

                OrderSortState.LastStatusChangeDateAsc => orders.OrderBy(o => o.LastStatusChangeDate),
                OrderSortState.LastStatusChangeDateDesc => orders.OrderByDescending(o => o.LastStatusChangeDate),

                _ => orders
            };
        }
        public void SaveSortStateCookie(OrderSortState sortOrder)
        {
            if (sortOrder != OrderSortState.StandardState)
            {
                Response.Cookies.Delete("SortState");
                Response.Cookies.Append("SortState", sortOrder.ToString());
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
        public async Task<IActionResult> DelSortStateCookieForV2()
        {
            Response.Cookies.Delete("SortState");
            Response.Cookies.Append("SortState", OrderSortState.StandardState.ToString());
            return RedirectToAction("Index");
        }
    }


}
