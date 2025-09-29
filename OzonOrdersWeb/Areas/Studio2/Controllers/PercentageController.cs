using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using OzonDomains.Models;
using OzonOrdersWeb.ViewModels.OrderViewModels;
using OzonRepositories.Context;
using PartsInfo.HttpUtils;
using Servcies.CacheServcies.Cache.OzonOrdersCache;
using Servcies.CacheServcies.Cache.UserCacheService;
using Servcies.DataServcies;
using Servcies.FiltersServcies.SortModels;
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
        _cacheUpdater = cacheUpdater;
        _cache = cache;
        _proxyHttpClientService = proxyHttpClientService;
    }

    public IActionResult UploadFile()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["Error"] = "Файл не выбран или пуст";
            return Json(new
            {
                redirectTo = Url.Action("UploadFile")
            });
        }

        var filteredStatuses = _context.AppStatuses.Where(status => status.Name == "Отменен").ToList();
        ViewBag.AppStatuses = new SelectList(filteredStatuses, "Id", "Name");
        ViewBag.StatusColors = filteredStatuses.ToDictionary(s => s.Id.ToString(), s => s.GetStatusColor());

        List<Order> ordersToTransaction = new List<Order>();
        var processedOrders = new List<(string OrderNumber, string Article)>();

        try
        {
            var fileExtension = Path.GetExtension(excelFile.FileName).ToLower();
            
            if (fileExtension == ".csv")
            {
                processedOrders = await ProcessCsvFile(excelFile);
            }
            else if (fileExtension == ".xlsx" || fileExtension == ".xls")
            {
                processedOrders = await ProcessExcelFile(excelFile);
            }
            else
            {
                TempData["Error"] = "Неподдерживаемый формат файла. Поддерживаются только CSV, XLSX и XLS файлы.";
                return Json(new
                {
                    redirectTo = Url.Action("Index", "Orders")
                });
            }

            if (processedOrders.Count == 0)
            {
                TempData["Error"] = "Не найдено подходящих данных для обработки";
                return Json(new
                {
                    redirectTo = Url.Action("UploadFile")
                });
            }

           

            TempData["Success"] = $"Обработано {processedOrders.Count} записей, найдено {ordersToTransaction.Count} заказов";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ошибка при обработке файла: {ex.Message}";
            return Json(new
            {
                redirectTo = Url.Action("UploadFile")
            });
        }
        
        var ids =await _orderServcies.GetOrderIdsByNumbersAndArticles(processedOrders);
        var orderIds = string.Join(",", ids);
        
        TempData["Success"] = $"Обработано {processedOrders.Count} записей, найдено {ordersToTransaction.Count} заказов";
        return RedirectToAction("Percentage", new { ids = orderIds});
    }

    // Обработка Excel файлов
    private async Task<List<(string OrderNumber, string Article)>> ProcessExcelFile(IFormFile excelFile)
    {
        var processedOrders = new List<(string OrderNumber, string Article)>();

        using (var stream = new MemoryStream())
        {
            await excelFile.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;
                var colCount = worksheet.Dimension?.Columns ?? 0;

                if (rowCount == 0 || colCount == 0)
                {
                    throw new Exception("Файл не содержит данных");
                }

                // Находим индексы столбцов
                int orderNumberCol = -1;
                int articleCol = -1;

                // Поиск столбцов по заголовкам
                for (int col = 1; col <= colCount; col++)
                {
                    var header = worksheet.Cells[1, col].Value?.ToString()?.ToLower();
                    if (header != null)
                    {
                        if (header.Contains("номер") && header.Contains("заказ"))
                            orderNumberCol = col;
                        else if (header.Contains("артикул"))
                            articleCol = col;
                    }
                }

                // Альтернативные варианты поиска столбцов
                if (orderNumberCol == -1)
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        var header = worksheet.Cells[1, col].Value?.ToString()?.ToLower();
                        if (header != null && (header.Contains("order") || header.Contains("number")))
                            orderNumberCol = col;
                    }
                }

                if (articleCol == -1)
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        var header = worksheet.Cells[1, col].Value?.ToString()?.ToLower();
                        if (header != null && (header.Contains("article") || header.Contains("art") || header.Contains("sku")))
                            articleCol = col;
                    }
                }

                if (orderNumberCol == -1 || articleCol == -1)
                {
                    throw new Exception("Не найдены необходимые столбцы 'Номер заказа' и 'Артикул'");
                }

                // Обработка данных
                for (int row = 2; row <= rowCount; row++) // начинаем с 2 строки, пропускаем заголовки
                {
                    var orderNumberStr = worksheet.Cells[row, orderNumberCol].Value?.ToString();
                    var article = worksheet.Cells[row, articleCol].Value?.ToString();

                    if (!string.IsNullOrEmpty(orderNumberStr) && !string.IsNullOrEmpty(article))
                    {
                        processedOrders.Add((orderNumberStr.Trim(), article.Trim()));
                    }
                }
            }
        }

        return processedOrders;
    }

    // Обработка CSV файлов
    private async Task<List<(string OrderNumber, string Article)>> ProcessCsvFile(IFormFile csvFile)
    {
        var processedOrders = new List<(string OrderNumber, string Article)>();
        
        using (var stream = new MemoryStream())
        {
            await csvFile.CopyToAsync(stream);
            stream.Position = 0;
            
            using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
            {
                // Создаем конфигурацию для CSV с разделителем ';'
                var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HasHeaderRecord = true,
                    TrimOptions = CsvHelper.Configuration.TrimOptions.Trim
                };
                
                using (var csv = new CsvHelper.CsvReader(reader, config))
                {
                    // Читаем заголовки
                    await csv.ReadAsync();
                    csv.ReadHeader();
                    
                    var headers = csv.HeaderRecord?.Select(h => h?.ToLower()).ToArray();
                    
                    if (headers == null || headers.Length == 0)
                    {
                        throw new Exception("CSV файл не содержит заголовков");
                    }
                    
                    // Находим индексы столбцов по точным названиям из вашего примера
                    int orderNumberCol = -1;
                    int articleCol = -1;
                    
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var header = headers[i];
                        if (header != null)
                        {
                            // Ищем столбец с номером заказа
                            if (header.Contains("номер заказа") || header.Contains("номер отправления"))
                                orderNumberCol = i;
                            // Ищем столбец с артикулом
                            else if (header.Contains("артикул"))
                                articleCol = i;
                        }
                    }
                    
                    // Альтернативные варианты поиска столбцов
                    if (orderNumberCol == -1)
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var header = headers[i];
                            if (header != null && (header.Contains("заказ") || header.Contains("отправление") || header.Contains("order") || header.Contains("number")))
                                orderNumberCol = i;
                        }
                    }
                    
                    if (articleCol == -1)
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var header = headers[i];
                            if (header != null && (header.Contains("article") || header.Contains("art") || header.Contains("sku")))
                                articleCol = i;
                        }
                    }
                    
                    if (orderNumberCol == -1 || articleCol == -1)
                    {
                        throw new Exception("Не найдены необходимые столбцы 'Номер заказа' и 'Артикул'. " +
                                          "Найдены заголовки: " + string.Join(", ", headers));
                    }
                    
                    Console.WriteLine($"Найден столбец номера заказа: {headers[orderNumberCol]} (индекс {orderNumberCol})");
                    Console.WriteLine($"Найден столбец артикула: {headers[articleCol]} (индекс {articleCol})");
                    
                    // Обработка данных
                    int processedCount = 0;
                    while (await csv.ReadAsync())
                    {
                        var orderNumberStr = csv.GetField(orderNumberCol);
                        var article = csv.GetField(articleCol);
                        
                        if (!string.IsNullOrEmpty(orderNumberStr) && !string.IsNullOrEmpty(article))
                        {
                            processedOrders.Add((orderNumberStr.Trim(), article.Trim()));
                            processedCount++;
                        }
                    }
                    
                    Console.WriteLine($"Обработано {processedCount} строк из CSV файла");
                }
            }
        }
        return processedOrders;
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