using System.Text;
using System.Text.RegularExpressions;
using Hangfire;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OzonDomains;
using OzonDomains.Models;
using OzonDomains.Models.OrderCarts;
using OzonOrdersWeb.ViewModels.JobViewModels;
using OzonRepositories.Context;
using OzonRepositories.Data;
using Servcies.ApiServcies.DropBoxApi;
using Servcies.ApiServcies.OzonApi;
using Servcies.ApiServcies.TradesoftApi;
using Servcies.ApiServcies.TradesoftApi.Models;
using Servcies.ApiServcies.YandexApi;
using Servcies.DataServcies;
using Servcies.ParserServcies;
using Servcies.ParserServcies.FielParsers;
using Servcies.ParserServcies.HelpDictEnum;
using Servcies.ReleasServcies.ReleaseManager;
using Servcies.SignalRServcies;

namespace OzonOrdersWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Studio2")]
    public class JobController : Controller
    {
        private readonly OrdersDataServcies _orderRepository;
        private readonly OrderCaster _orderCaster;
        private readonly OzonJsonDataBuilder _jsonDataBuilder;
        private readonly ReleaseManager _releaseManager;
        private readonly DuplicateOrdersServcies _duplicateOrdersServcies;
        private readonly OzonClientServcies _ozonClientServcies;
        private readonly YandexDataManager _yandexDataManager;
        private readonly DropboxApiClient _dropboxApiClient;
        private readonly ExcelParser _excelParser;
        private readonly ColumnMappingDataServcies _columnMappingDataServcies;
        private readonly OzonOrderContext _context;
        private readonly OrdersDataServcies _orderDataServcies;
        private readonly FileUploadRecordDataService _fileUploadRecordDataService;
        private readonly MaxiPartsConfig _maxiPartsConfig;
        private readonly TradesoftDataManager _tradesoftDataManager;
        private readonly OrderItemRepository _orderItemRepository;
        public JobController(OrdersDataServcies ordersDataServcies,
                              OrderCaster orderCaster,
                              OzonJsonDataBuilder jsonDataBuilder,
                              ReleaseManager releaseManager,
                              DuplicateOrdersServcies duplicateOrdersServcies,
                              OzonClientServcies ozonClientServcies,
                              YandexDataManager yandexDataManager,
                              DropboxApiClient dropboxApiClient,
                              ExcelParser excelParser,
                              ColumnMappingDataServcies columnMappingDataServcies,
                              OzonOrderContext context,
                              OrdersDataServcies orderDataServcies,
                              FileUploadRecordDataService fileUploadRecordDataService,
                              MaxiPartsConfig maxiPartsConfig,
                              TradesoftDataManager tradesoftDataManager,
                              OrderItemRepository orderItemRepository)
        {
            _orderRepository = ordersDataServcies;
            _orderCaster = orderCaster;
            _jsonDataBuilder = jsonDataBuilder;
            _releaseManager = releaseManager;
            _duplicateOrdersServcies = duplicateOrdersServcies;
            _ozonClientServcies = ozonClientServcies;
            _yandexDataManager = yandexDataManager;
            _dropboxApiClient = dropboxApiClient;
            _excelParser = excelParser;
            _columnMappingDataServcies = columnMappingDataServcies;
            _context = context;
            _orderDataServcies = orderDataServcies;
            _fileUploadRecordDataService = fileUploadRecordDataService;
            _maxiPartsConfig = maxiPartsConfig;
            _tradesoftDataManager = tradesoftDataManager;
            _orderItemRepository = orderItemRepository;
        }

        public IActionResult Index()
        {
            var api = JobStorage.Current.GetMonitoringApi();
            JobList<SucceededJobDto> succeededJobs = api.SucceededJobs(0, 50);

            // Проверка на null или пустоту
            if (succeededJobs == null || succeededJobs.Count == 0)
            {
                return View(new JobResultViewModel { Results = new List<string>() });
            }

            var results = new List<dynamic>();

            foreach (var job in succeededJobs)
            {
                if (job.Value.Result != null)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<dynamic>((string)job.Value.Result);
                        results.Add(result);
                    }
                    catch (JsonException ex)
                    {
                        results.Add($"Error deserializing result for job {job.Key}: {ex.Message}");
                    }
                }
            }

            var jobResult = new JobResultViewModel()
            {
                Results = new List<string>()
            };

            // Обработка результатов
            foreach (var result in results)
            {
                if (result is string)
                {
                    jobResult.Results.Add((string)result);
                }
            }

            return View(jobResult);
        }
        
        [HttpPost]
        public IActionResult SetOzonRecurring(int delay)
        {
            string cronExp = $"*/{delay} * * * *";
            RecurringJob.AddOrUpdate(() => UploadingOzon(1), cronExp, queue: "upload-queue-new");

            return Redirect("/Hangfire");
        }
        

        [HttpPost]
        public IActionResult SetYandexRecurring(int delay)
        {
            string cronExp = $"*/{delay} * * * *";
            RecurringJob.AddOrUpdate(() => UploadingYandex(1), cronExp, queue: "upload-queue-new");

            return Redirect("/Hangfire");
        }

        [HttpPost]
        public async Task<IActionResult> SetEverydayUpdate(int monthsCount, int updateHour, int updateMinute)
        {
            string cronExp = $"{updateMinute} {updateHour} * * *";
            RecurringJob.AddOrUpdate(() => EverydayUpdate(monthsCount), cronExp, queue: "upload-queue-new");

            return Redirect("/Hangfire");
        }
        
        [HttpPost]
        public IActionResult SetOzonFileUploadRecurring(int delay)
        {
            string cronExp = $"*/{delay} * * * *"; 
            RecurringJob.AddOrUpdate("FileUpload", () => FileUpload(), cronExp, queue: "upload-queue-new");

            return Redirect("/Hangfire");
        }
        
        [HttpPost]
        public IActionResult SetStatusUpdateRecurring(int delay, int count)
        {
            string cronExp = $"*/{delay} * * * *";
            RecurringJob.AddOrUpdate(
                "StatusUpdateJob", 
                () => UpdateCartItemsStatusAsync(count), 
                cronExp, 
                queue: "upload-queue-new"
            );
        
            TempData["Message"] = $"Фоновое обновление статусов установлено с периодом {delay} минут";
            return Redirect("/Hangfire");
        }

        [HttpPost]
        public async Task<Dictionary<string, int[]>> EverydayUpdate(int monthsCount)
        {
            var periods = GetDateRanges(monthsCount);
            List<OzonClient> ozonClients = (await _ozonClientServcies.GetOzonClients()).Where(c => c.ClientType == ClientType.OZON).ToList();
            Dictionary<string, int[]> result = new Dictionary<string, int[]>();

            foreach (var period in periods)
            {
                int timeZone = _releaseManager.GetTimeZone();


                foreach (var client in ozonClients)
                {
                    try
                    {
                        _jsonDataBuilder.SetClient(client.DecryptClientId, client.DecryptApiKey);
                        var jsonData = await _jsonDataBuilder.BuildData(period.Item1, period.Item2);
                        var orders = await _orderCaster.JsonToOrders(jsonData);
                        orders = orders.Select(order => { order.OzonClient = client; return order; }).ToList();
                        var uploadResult = await _orderRepository.AddOrders(orders);
                        result.Add(client.Name, uploadResult);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            return result;
        }

        public async Task<string> UploadingOzon(int period)
        {
            List<OzonClient> ozonClients = (await _ozonClientServcies.GetOzonClients())
                .Where(c => c.ClientType == ClientType.OZON)
                .ToList();

            Dictionary<string, int[]> result = new Dictionary<string, int[]>();
            int totalUploaded = 0;
            int totalUpdated = 0;

            int timeZone = _releaseManager.GetTimeZone();
            var start = DateTime.Now.AddHours(-period * 12 + timeZone);
            var end = DateTime.Now.AddHours(timeZone);
            string clientStatus = "Результат загрузки по Ozon клиентам<br>";

            foreach (var client in ozonClients)
            {
                try
                {
                    _jsonDataBuilder.SetClient(client.DecryptClientId, client.DecryptApiKey);
                    var jsonData = await _jsonDataBuilder.BuildData(start, end);
                    var orders = await _orderCaster.JsonToOrders(jsonData);
                    orders = orders.Select(order => { order.OzonClient = client; return order; }).ToList();

                    var updatingStatusCount = await ProcessOrdersForStatusUpdate(orders);
                    var uploadResult = await _orderRepository.AddOrders(orders);
                    result.Add(client.Name, uploadResult);

                    totalUploaded += uploadResult[0];
                    totalUpdated += uploadResult[1];

                    clientStatus += $"&nbsp;&nbsp;&nbsp;&nbsp;{client.Name}: отчёт успешно создан (обновлено {updatingStatusCount} статусов)<br>";
                }
                catch (Exception ex)
                {
                    clientStatus += $"&nbsp;&nbsp;&nbsp;&nbsp;{client.Name}: {ex.Message}<br>";
                }
            }

            int? duplicateCount;
            int? deletedRowsCount;
            (duplicateCount, deletedRowsCount) = _duplicateOrdersServcies.DeleteDuplicateOrders();

            await NotificationService.NotifyAllAsync(
                $"Ozon {start: HH:mm:ss} - {end: HH:mm:ss}: загружено {totalUploaded}, обновлено {totalUpdated}");

            return GetJobResultString(result, start, end, clientStatus, duplicateCount, deletedRowsCount);
        }

        
        private async Task<int> ProcessOrdersForStatusUpdate(List<Order> orders)
        {
            int updatingCount = 0;
            async Task ProcessOrder(Order order)
            {
                order.OzonClient ??= await _ozonClientServcies.GetOzonClientAsync(order.OzonClient?.Id ?? 0);
                if (order.OzonClient != null && order.OzonClient.ClientType == ClientType.OZON)
                {
                    _jsonDataBuilder.SetClient(order.OzonClient.DecryptClientId, order.OzonClient.DecryptApiKey);

                    string newStatus = await _jsonDataBuilder.GetProductSatatus(order.ShipmentNumber);
                    if (!string.IsNullOrEmpty(newStatus) &&
                        !order.Status.Equals(OzonStatus.OrderStatuses[newStatus], StringComparison.OrdinalIgnoreCase))
                    {
                        order.Status = OzonStatus.OrderStatuses[newStatus];
                        updatingCount++;
                    }
                }
            }
            var tasks = orders.Select(order => ProcessOrder(order));
            await Task.WhenAll(tasks);
            return updatingCount;
        }
        

        public async Task<string> UploadingYandex(int period)
        {
            List<OzonClient> ozonClients = (await _ozonClientServcies.GetOzonClients())
                .Where(c => c.ClientType == ClientType.YANDEX)
                .ToList();

            Dictionary<string, int[]> result = new Dictionary<string, int[]>();
            int totalUploaded = 0;
            int totalUpdated = 0;

            int timeZone = _releaseManager.GetTimeZone();
            var start = DateTime.Now.AddHours(-period * 12 + timeZone);
            var end = DateTime.Now.AddHours(timeZone);
            string clientStatus = "Результат загрузки по Yandex клиентам<br>";

            foreach (var client in ozonClients)
            {
                try
                {
                    _yandexDataManager.SetClient(client.DecryptClientId, client.DecryptApiKey);
                    var jsonData = await _yandexDataManager.GetOrders(start, end);
                    var orders = await _orderCaster.YandexToOrders(jsonData);
                    orders = orders.Select(order => { order.OzonClient = client; return order; }).ToList();

                    var uploadResult = await _orderRepository.AddOrders(orders);
                    result.Add(client.Name, uploadResult);

                    totalUploaded += uploadResult[0];
                    totalUpdated += uploadResult[1];

                    clientStatus += $"&nbsp;&nbsp;&nbsp;&nbsp;{client.Name}: отчёт успешно создан<br>";
                }
                catch (Exception ex)
                {
                    clientStatus += $"&nbsp;&nbsp;&nbsp;&nbsp;{client.Name}: {ex.Message}<br>";
                    continue;
                }
            }

            int? duplicateCount;
            int? deletedRowsCount;
            (duplicateCount, deletedRowsCount) = _duplicateOrdersServcies.DeleteDuplicateOrders();

            await NotificationService.NotifyAllAsync(
                $"Yandex {start: HH:mm:ss} - {end: HH:mm:ss}: загружено {totalUploaded}, обновлено {totalUpdated}");

            return GetJobResultString(result, start, end, clientStatus, duplicateCount, deletedRowsCount);
        }


        private string GetJobResultString(Dictionary<string, int[]> result, DateTime start, DateTime end, string clientStatus, int? duplicateCount, int? deletedRowsCount)
        {
            string jobResult = "";

            jobResult += $"Период: {start.ToString("dd.MM.yyyy HH:mm:ss")} - {end.ToString("dd.MM.yyyy HH:mm:ss")}<br/>";
            foreach(var kvp in result)
            {
                jobResult += $"&nbsp;&nbsp;&nbsp;&nbsp;Клиент: {kvp.Key} - загружено {kvp.Value[0]}, обновлено {kvp.Value[1]}<br/>";
            }
            jobResult += clientStatus;

            jobResult += $"Удалено дублей {deletedRowsCount} из {duplicateCount}";

            
            return jobResult;
        }

        public  List<(DateTime, DateTime)> GetDateRanges(int months)
        {
            List<(DateTime, DateTime)> dateRanges = new List<(DateTime, DateTime)>();
            
            DateTime endDate = DateTime.Today;

            DateTime startDate = endDate.AddMonths(-months);

            while (startDate < endDate)
            {
                DateTime weekStart = startDate;
                DateTime weekEnd = startDate.AddDays(6);
                if (weekEnd > endDate)
                {
                    weekEnd = endDate;
                }
                dateRanges.Add((weekStart, weekEnd));
                startDate = startDate.AddDays(7);
            }

            return dateRanges;
        }
        
        public async Task<string> FileUpload()
        {
            string resultTxt = string.Empty;
            
            var files = await _dropboxApiClient.GetFolderContentsAsync("/Orders");
            foreach (var file in files)
            {
                try
                {
                    DateTime selectedShippingDate = DateTime.Now;
                    DateTime selectedProcessingDate = DateTime.Now;
                    
                    (var mappingName, selectedShippingDate) = ParseFileFieldAndAdjustDate(file.Key, selectedProcessingDate);
                    
                    var mapping = await _columnMappingDataServcies.GetColumnMappingAsync(new ColumnMapping { MappingName = mappingName });
                    if (mapping == null)
                    {
                        continue;
                    }

                    var selectedClient = await _context.OzonClients.FindAsync(mapping.SelectedClientId) ?? new OzonClient { Id = 0 };
                    var selectedManufacturer = await _context.Manufacturers.FindAsync(mapping.SelectedManufacturerId) ?? new Manufacturer { Id = 0 };
                    var selectedWarehouse = await _context.Warehouses.FindAsync(mapping.SelectedWarehouseId) ?? new Warehouse { Id = 0 };
                    var selectedSupplier = await _context.Suppliers.FindAsync(mapping.SelectedSupplierId) ?? new Supplier { Id = 0 };

                    foreach (var fileName in file.Value)
                    {
                        var isFileUnique =
                            await _fileUploadRecordDataService.GetFileUploadRecordAsync(new FileUploadRecord()
                                { FileName = fileName.Key });
                        if (isFileUnique != null)
                        {
                            continue;
                        }
                            
                        int[] result = [0, 0];
                        string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(fileName.Key));

                        try
                        {
                            using (var httpClient = new HttpClient())
                            {
                                var fileBytes = await httpClient.GetByteArrayAsync(fileName.Value);
                                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                                {
                                    await fileStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                                }
                            }
                            List<Dictionary<string, string>> tableData;
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                                {
                                    await fileStream.CopyToAsync(memoryStream);
                                }
                                memoryStream.Position = 0;

                                string contentType = Path.GetExtension(tempFilePath).ToLower();
                                
                                var startRow = 1;
                                var startColumn = 1;
                                
                                if (mappingName == "Автопитер")
                                {
                                    startRow = 3;
                                    startColumn = 1;
                                }
                                
                                if (contentType == ".xlsx" || contentType == ".xls")
                                {
                                    tableData = await _excelParser.GetTableDataAsyncForDropbox(memoryStream, startRow: startRow, startColumn: startColumn);
                                }
                                else if (contentType == ".csv")
                                {
                                    var convertedStream = _excelParser.ConvertCsvToExcel(memoryStream, delimiter: ',');
                                    convertedStream.Position = 0;
                                    tableData = await _excelParser.GetTableDataAsyncForDropbox(convertedStream, startRow: startRow, startColumn: startColumn);
                                }
                                else
                                {
                                    throw new Exception("Неподдерживаемый тип файла.");
                                }
                            }
                            
                            tableData = _excelParser.UpdateTableToStandartColumns(tableData, mapping.ColumnMappings);
                            List<Order> orders = await _orderCaster.ExcelToOrdersForDropbox(
                                tableData,
                                selectedClient,
                                selectedManufacturer,
                                selectedWarehouse,
                                selectedSupplier,
                                mapping.SelectedStatus,
                                mapping.SelectedCurrencyCode.Value,
                                selectedShippingDate,
                                selectedProcessingDate);

                            orders = await _orderDataServcies.SetUniqueShipmentNumberAndKey(orders);
                            orders = await _orderCaster.SetFileDataAsync(orders, mappingName, fileName.Key);

                            // Сохранение заказов в базу данных
                            var uploadResult = await _orderDataServcies.AddOrders(orders);
                            result[0] += uploadResult[0];
                            result[1] += uploadResult[1];
                            resultTxt += $"{mappingName}/{fileName.Key}: Добавлено {result[0]}, обновлено {result[1]}<br>";
                            await _fileUploadRecordDataService.AddFileUploadRecord(new FileUploadRecord()
                            {
                                FileName = fileName.Key,
                                FolderName = mappingName,
                                Date = DateTime.Now,
                                Result = $"Добавлено {result[0]}, обновлено {result[1]}"
                            });
                        }
                        catch (Exception e)
                        {
                            // Логирование ошибки (предполагается наличие логгера)
                            resultTxt += $"{mappingName}/{fileName.Key}: Ошибка: {e.Message}<br>";
                        }
                        finally
                        {
                            if (System.IO.File.Exists(tempFilePath))
                            {
                                System.IO.File.Delete(tempFilePath);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // Обработка ошибок на уровне файла
                    resultTxt += $"{file.Key}: Ошибка: {e.Message}<br>";
                }
            };
            
            if (resultTxt.IsNullOrEmpty())
            {
                resultTxt = "Новых файлов не найдено";
            }
            return resultTxt;
        }
        
        public (string Name, DateTime Date) ParseFileFieldAndAdjustDate(string file, DateTime initialDate)
        {
            if (string.IsNullOrEmpty(file))
                return (file, initialDate); // Если поле пустое, возвращаем исходное значение и дату

            // Используем регулярное выражение для поиска шаблона: название + пробел + знак (+/-) + число
            var match = Regex.Match(file, @"^(.*?)\s([+-]\d+)$");

            if (match.Success) // Если найдено соответствие шаблону
            {
                string name = match.Groups[1].Value; // Название (первая часть)
                string value = match.Groups[2].Value; // Значение со знаком и числом (например, +6 или -3)

                if (int.TryParse(value, out int days)) 
                {
                    DateTime adjustedDate = initialDate.AddDays(days);
                    return (name, adjustedDate); 
                }
            }
            return (file, initialDate);
        }
        
        public async Task<string> UpdateCartItemsStatusAsync(int count)
        {
            // Получаем последние элементы
            var items = await _orderItemRepository.GetLast(count);

            if (items == null || !items.Any())
                return "<p>Ошибка: Нет данных для обработки</p>";

            // Создаем запрос, исключая null OrderItemCode
            var request = new GetItemsStatusContainer
            {
                Provider = "war_provider_maxi_parts",
                Login = _maxiPartsConfig.User,
                Password = _maxiPartsConfig.Password,
                Items = items.Where(x => !string.IsNullOrEmpty(x.OrderItemCode))
                             .Select(x => x.OrderItemCode)
                             .ToList()
            };

            if (!request.Items.Any())
                return "<p>Ошибка: Нет валидных OrderItemCode</p>";

            // Получаем статусы из внешнего API
            var response = await _tradesoftDataManager.GetItemsStatusAsync(request);

            if (response?.Container == null)
                return "<p>Ошибка: Пустой ответ от API</p>";

            var container = response.Container.FirstOrDefault();
            if (container?.Items == null)
                return "<p>Информация: Нет данных для обновления</p>";

            // Собираем информацию об изменениях
            var changes = new List<string>();
            int updatedCount = 0;

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.OrderItemCode))
                    continue;

                if (container.Items.TryGetValue(item.OrderItemCode, out var statusItem) &&
                    string.IsNullOrEmpty(statusItem?.Error))
                {
                    var cartItem = await _orderItemRepository.GetAsync(item.Id);
                    if (cartItem == null)
                        continue;

                    var oldStatus = cartItem.ItemStatus?.Name ?? "нет статуса";
                    var newStatus = statusItem.StateName;

                    if (oldStatus != newStatus)
                    {
                        cartItem.ItemStatus = new ItemStatus { Name = newStatus };
                        await _orderItemRepository.Update(cartItem);
                        updatedCount++;
                        changes.Add($"[ID: {item.Id}] {oldStatus} → {newStatus}");
                    }
                }
            }
            var report = new StringBuilder();
            report.Append($"<h4>=== Отчет об обновлении статусов ===</h4>");
            report.Append($"<p>• Всего обработано: {items.Count}<br>");
            report.Append($"• Успешно обновлено: {updatedCount}</p>");

            if (updatedCount > 0)
            {
                report.Append("<p><strong>Детали изменений:</strong><br>");
                report.Append(string.Join("<br>", changes));
                report.Append("</p>");
            }
            report.Append($"<p>Время обработки: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

            await NotificationService.NotifyAllAsync($"Обновлено {updatedCount} статусов в фоне");
            
            return report.ToString();
        }
    }
}
