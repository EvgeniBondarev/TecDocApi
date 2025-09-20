using Newtonsoft.Json.Linq;
using OzonDomains;
using OzonDomains.Models;
using OzonRepositories.Context;
using Servcies.DataServcies;
using Servcies.ParserServcies.HelpDictEnum;
using Servcies.ReleasServcies.ReleaseManager;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Services.CacheServcies.Cache;
using SlqStudio.Application.Services.EmailService;
using SlqStudio.Application.Services.EmailService.Implementation;

namespace Servcies.ParserServcies
{
    public class OrderCaster
    {
        private readonly OzonOrderContext _context;
        private readonly ReleaseManager _releaseManager;
        private readonly OrdersFileMetadataDataService _ordersFileMetadataDataService;
        private readonly EtProducerDataServices _etProducerDataServices; 
        private readonly AppCache _appCache;
        private readonly IEmailService _emailService;
        private readonly DeliveryDataServcies _deliveryService;
        
        public OrderCaster(OzonOrderContext context,
                           ReleaseManager releaseManager,
                           OrdersFileMetadataDataService ordersFileMetadataDataService,
                           EtProducerDataServices etProducerDataServices,
                           AppCache appCache,
                           IEmailService emailService,
                           DeliveryDataServcies deliveryService)
        {
            _context = context;
            _releaseManager = releaseManager;
            _ordersFileMetadataDataService = ordersFileMetadataDataService;
            _etProducerDataServices = etProducerDataServices;
            _appCache = appCache;
            _emailService = emailService;
            _deliveryService = deliveryService;
        }

        public async Task<List<Order>> JsonToOrders(JArray orders)
        {
            List<Order> resultOrders = new List<Order>();
            JToken currentOrder = null;

            foreach (var order in orders)
            {
                try 
                {
                    resultOrders.Add(await CastToModel(order));
                }
                catch (Exception e)
                {
                    await _emailService.SendEmailAsync(e.Message, e.ToString());
                }
            }
            
            return resultOrders;
        }

        public async Task<List<Order>> ExcelToOrders(List<Dictionary<string, string>> table,
            OzonClient client,
            Manufacturer manufacturer,
            Warehouse warehouse,
            Supplier supplier,
            string clientStatus,
            CurrencyCode currencyCode,
            DateTime? selectedShippingDate,
            DateTime? selectedProcessingDate)
        {
            string cacheKey = _appCache.GenerateCacheKey(table);

            if (_appCache.GetCache().TryGetValue(cacheKey, out List<Order> cachedOrders))
            {
                Console.WriteLine($"Данные Excel найдены в кэше: {cacheKey}");
                return cachedOrders;
            }

            List<Order> resultOrders = new List<Order>();

            try
            {
                foreach (var row in table)
                {
                    resultOrders.Add(await CastToModelFromExcel(row, client, manufacturer, warehouse, supplier, 
                        clientStatus, currencyCode, selectedShippingDate, selectedProcessingDate));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке данных из Excel: {ex.Message}");
            }
            
            _appCache.GetCache().Set(cacheKey, resultOrders, TimeSpan.FromMinutes(10));

            return resultOrders;
        }
        
        public async Task<List<Order>> ExcelToOrdersForDropbox(List<Dictionary<string, string>> table,
            OzonClient client,
            Manufacturer manufacturer,
            Warehouse warehouse,
            Supplier supplier,
            string clientStatus,
            CurrencyCode currencyCode,
            DateTime? selectedShippingDate,
            DateTime? selectedProcessingDate)
        {
            string cacheKey = _appCache.GenerateCacheKey(table);

            if (_appCache.GetCache().TryGetValue(cacheKey, out List<Order> cachedOrders))
            {
                Console.WriteLine($"Данные Excel найдены в кэше: {cacheKey}");
                return cachedOrders;
            }

            List<Order> resultOrders = new List<Order>();

            try
            {
                foreach (var row in table)
                {
                    resultOrders.Add(await CastToModelFromExcelForDropBox(row, client, manufacturer, warehouse, supplier, 
                        clientStatus, currencyCode, selectedShippingDate, selectedProcessingDate));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке данных из Excel: {ex.Message}");
            }
            
            _appCache.GetCache().Set(cacheKey, resultOrders, TimeSpan.FromMinutes(10));

            return resultOrders;
        }


        public async Task<List<Order>> YandexToOrders(JArray orders)
        {
            List<Order> resultOrders = new List<Order>();

            foreach (var order in orders)
            {
                resultOrders.Add(await CastToModelFromYandex(order));
            }
            return resultOrders;
        }

        public async Task<List<Order>> SetFileDataAsync(List<Order> orders, string filePath, string fileName)
        {
            var fileMetadata = await _ordersFileMetadataDataService.GetOrdersFileMetadataAsync(new OrdersFileMetadata
            {
                FileName = fileName,
                FolderName = filePath
            });

            if (fileMetadata == null)
            {
                fileMetadata = new OrdersFileMetadata
                {
                    FileName = fileName,
                    FolderName = filePath
                };

                await _ordersFileMetadataDataService.AddOrdersFileMetadata(fileMetadata);
                fileMetadata = await _ordersFileMetadataDataService.GetOrdersFileMetadataAsync(fileMetadata);
            }
            foreach (var order in orders)
            {
                order.ExcelFileData = fileMetadata;
            }

            return orders;
        }

        private async Task<Order> CastToModel(JToken? jsonOrder)
        {
            Order order = new();

            CultureInfo culture = new("en-US");
            NumberStyles style = NumberStyles.Number;

            order.Key = jsonOrder["Номер отправления"].ToString() + jsonOrder["Артикул"].ToString();

            order.ShipmentNumber = jsonOrder["Номер отправления"].ToString();

            order.ProcessingDate = DateTime.TryParse(jsonOrder["Принят в обработку"].ToString(), culture, out var processingDate)
                ? processingDate
                : null;

            order.ShippingDate = DateTime.TryParse(jsonOrder["Дата отгрузки"].ToString(), culture, out var shipingData)
                ? shipingData
                : null;

            order.Status = SetCorrectStatus(jsonOrder["Статус"].ToString());

            order.ShipmentAmount = decimal.TryParse(DelSubStr(jsonOrder["Сумма отправления"].ToString()), style, culture, out var shipmentAmount)
                ? shipmentAmount
                : null;

            order.ProductName = jsonOrder["Наименование товара"].ToString();

            order.ProductKey = jsonOrder["Артикул"].ToString();



            order.Quantity = int.TryParse(DelSubStr(jsonOrder["Количество"].ToString()), style, culture, out var quantity)
            ? quantity
            : null;

      

            Warehouse shipmentWarehouse = _context.Warehouses.FirstOrDefault(w => w.Name == jsonOrder["productWarehousesAndCitysWithNumber"]["delivery_method"]["warehouse"].ToString());

            if (shipmentWarehouse != null)
            {
                order.ShipmentWarehouse = shipmentWarehouse;
            }
            else
            {
                var newWarehouse = new Warehouse
                {
                    Name = jsonOrder["productWarehousesAndCitysWithNumber"]["delivery_method"]["warehouse"].ToString()
                };

                _context.Warehouses.Add(newWarehouse);
                _context.SaveChanges();
                order.ShipmentWarehouse = newWarehouse;
            }

            string extractedCode = Regex.Match(jsonOrder["Артикул"].ToString(), @"=(\w{3})").Groups[1].Value;
            

            if (string.IsNullOrEmpty(extractedCode))
            {
                extractedCode = Regex.Match(jsonOrder["Артикул"].ToString(), @"=(\w{2})").Groups[1].Value;
            }
            
            order = await SetEtProducerByCode(order, extractedCode);
            
            string currencyCode = jsonOrder["Код валюты отправления"].ToString();

            using (var transaction = _context.Database.BeginTransaction())
            {
                Currency currency = _context.Currencys
                    .FirstOrDefault(c => c.Name == currencyCode);

                if (currency != null)
                {
                    order.Сurrency = currency;
                }
                else
                {
                    var newCurrency = new Currency
                    {
                        Name = currencyCode
                    };

                    _context.Currencys.Add(newCurrency);
                    _context.SaveChanges();

                    order.Сurrency = newCurrency;
                }
                transaction.Commit();
            }


            order.Supplier = null;
            order.PurchasePrice = null;

            if (order.ProductInfo == null)
            {
                Product product = _context.Products.FirstOrDefault(p => p.Article == jsonOrder["Артикул"].ToString());
                if (product != null)
                {
                    order.ProductInfo = product;
                }
                else
                {
                    product = new Product();
                    product.Article = jsonOrder["Артикул"].ToString();
                    product.OzonProductId = jsonOrder["productWithArticle"]["Ozon Product ID"].ToString();
                    product.FboOzonSkuId = jsonOrder["productWithArticle"]["SKU"].ToString();
                    product.FbsOzonSkuId = jsonOrder["productWithArticle"]["SKU"].ToString();
                    product.CommercialCategory = jsonOrder["productWithArticle"]["Категория комиссии"].ToString();

                    double volume;
                    if (double.TryParse(DelSubStr(jsonOrder["productWithArticle"]["Объем товара, л"].ToString()), style, culture, out volume))
                    {
                        product.Volume = volume;
                    }
                    else
                    {
                        product.Volume = null;
                    }

                    double volumetricWeight;
                    if (double.TryParse(DelSubStr(jsonOrder["productWithArticle"]["Объемный вес, кг"].ToString()), style, culture, out volumetricWeight))
                    {
                        product.VolumetricWeight = volumetricWeight;
                    }
                    else
                    {
                        product.VolumetricWeight = null;
                    }

                    _context.Products.Add(product);
                    _context.SaveChanges();

                    order.ProductInfo = product;

                }
            }


            var city = jsonOrder["productWarehousesAndCitysWithNumber"]["analytics_data"]["city"].ToString();
            var region = jsonOrder["productWarehousesAndCitysWithNumber"]["analytics_data"]["region"].ToString();

            order.DeliveryCity = city != null ? city.ToString() : region.ToString();


            if (order.AppStatus == null)
            {
                string statusName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    AppStatus appStatus = _context.AppStatuses.FirstOrDefault(c => c.Name == statusName);

                    if (appStatus != null)
                    {
                        order.AppStatus = appStatus;
                    }
                    else
                    {
                        var newStatus = new AppStatus
                        {
                            Name = statusName
                        };

                        _context.AppStatuses.Add(newStatus);
                        _context.SaveChanges();

                        order.AppStatus = newStatus;
                    }
                    transaction.Commit();
                }
            }

            if (order.Supplier == null)
            {
                string supplierName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    Supplier supplier = _context.Suppliers.FirstOrDefault(c => c.Name == supplierName);

                    if (supplier != null)
                    {
                        order.Supplier = supplier;
                    }
                    else
                    {
                        var newSupplier = new Supplier
                        {
                            Name = supplierName
                        };

                        _context.Suppliers.Add(newSupplier);
                        _context.SaveChanges();

                        order.Supplier = newSupplier;
                    }
                    transaction.Commit();
                }
            }
            
            var deliveryName = jsonOrder["productWarehousesAndCitysWithNumber"]["delivery_method"]["name"]?.ToString();
            var deliveryProvider = jsonOrder["productWarehousesAndCitysWithNumber"]["delivery_method"]["tpl_provider"]?.ToString();

            if (!string.IsNullOrWhiteSpace(deliveryName))
            { 
                var delivery = await _deliveryService.GetOrCreateDeliveryAsync(deliveryName, deliveryProvider);

                order.Delivery = delivery;
                order.DeliveryId = delivery.Id;
            }


            order = SetCorrectProductKey(order);

            var (price, priceWithDiscount, maxComment, minComment, min, max) = CalculateСommissions(jsonOrder, order);

            order.Price = price;
            order.ProductInfo.CurrentPriceWithDiscount = priceWithDiscount;

            order.MaxOzonCommission = max;
            order.MinOzonCommission = min;
            order.MaxCommissionInfo = maxComment;
            order.MinCommissionInfo = minComment;
            order.UpdatedBy = "Ozon";
            _context.SaveChanges();

            return order;
        }

        public async Task<Order> CastToModelFromYandex(JToken? jsonOrder)
        {
            Order order = new();

            CultureInfo culture = new("en-US");
            NumberStyles style = NumberStyles.Number;
            var dateFormats = new[] { "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy" };

            order.Key = jsonOrder["id"].ToString() + jsonOrder["items"][0]["offerId"].ToString();

            order.ShipmentNumber = jsonOrder["id"].ToString();

            if (order.ShipmentNumber == "48734722112")
            {
                
            }
            
            order.ProcessingDate = DateTime.TryParseExact(
                                    jsonOrder["creationDate"].ToString(),
                                    dateFormats,
                                    culture,
                                    DateTimeStyles.None,
                                    out var processingDate)
                                    ? processingDate
                                    : null;

            order.ShippingDate = jsonOrder["delivery"]?["shipments"]?.FirstOrDefault()?["shipmentDate"] != null &&
                                    DateTime.TryParseExact(
                                        jsonOrder["delivery"]["shipments"]?.FirstOrDefault()?["shipmentDate"]?.ToString(),
                                        dateFormats,
                                        culture,
                                        DateTimeStyles.None,
                                        out var shippingDate)
                                    ? shippingDate
                                    : null;
            /*order.ShippingDate =
            jsonOrder["delivery"]?["dates"]?["toDate"] != null &&
                    DateTime.TryParseExact(
                    jsonOrder["delivery"]["dates"]?["toDate"]?.ToString(),
                    dateFormats,
                    culture,
                    DateTimeStyles.None,
                    out var shippingDate)
                    ? shippingDate
                    : null;
           */

            order.Status = SetCorrectStatus(YandexStatus.OrderStatuses[jsonOrder["status"].ToString()]);

            order.ProductName = jsonOrder["items"][0]["offerName"].ToString();

            order.ProductKey = jsonOrder["items"][0]["offerId"].ToString();



            order.Quantity = int.TryParse(DelSubStr(jsonOrder["items"][0]["count"].ToString()), style, culture, out var quantity)
            ? quantity
            : null;


            Warehouse shipmentWarehouse = _context.Warehouses.FirstOrDefault(w => w.Name == jsonOrder["warehouseName"].ToString());

            if (shipmentWarehouse != null)
            {
                order.ShipmentWarehouse = shipmentWarehouse;
            }
            else
            {
                var newWarehouse = new Warehouse
                {
                    Name = jsonOrder["warehouseName"].ToString()
                };

                _context.Warehouses.Add(newWarehouse);
                _context.SaveChanges();
                order.ShipmentWarehouse = newWarehouse;
            }

            string extractedCode = Regex.Match(jsonOrder["items"][0]["offerId"].ToString(), @"=(\w{3})").Groups[1].Value;

            if (string.IsNullOrEmpty(extractedCode))
            {
                extractedCode = Regex.Match(jsonOrder["items"][0]["offerId"].ToString(), @"=(\w{2})").Groups[1].Value;
            }

            Manufacturer manufacturer = _context.Manufacturers
                .Where(m => m.Code == extractedCode)
                .FirstOrDefault();

            if (manufacturer != null)
            {
                order.Manufacturer = manufacturer;
            }
            else
            {
                var newManufacturer = new Manufacturer { Code = extractedCode };
                _context.Manufacturers.Add(newManufacturer);
                _context.SaveChanges();
                order.Manufacturer = newManufacturer;
            }
            
            order = await SetEtProducerByCode(order, extractedCode);

            string currencyCode = jsonOrder["currency"].ToString();

            using (var transaction = _context.Database.BeginTransaction())
            {
                Currency currency = _context.Currencys
                    .FirstOrDefault(c => c.Name == currencyCode);

                if (currency != null)
                {
                    order.Сurrency = currency;
                }
                else
                {
                    var newCurrency = new Currency
                    {
                        Name = currencyCode
                    };

                    _context.Currencys.Add(newCurrency);
                    _context.SaveChanges();

                    order.Сurrency = newCurrency;
                }
                transaction.Commit();
            }


            order.Supplier = null;
            order.PurchasePrice = null;

            if (order.ProductInfo == null)
            {
                Product product = _context.Products.FirstOrDefault(p => p.Article == jsonOrder["items"][0]["offerId"].ToString());
                if (product != null)
                {
                    order.ProductInfo = product;
                }
                else
                {
                    product = new Product();
                    product.Article = jsonOrder["items"][0]["offerId"].ToString();

                    product.Volume = null;

                    _context.Products.Add(product);
                    _context.SaveChanges();

                    order.ProductInfo = product;

                }
            }


            var city = jsonOrder["delivery"]?["address"]?["city"]?.ToString() ?? string.Empty;
            var region = jsonOrder["delivery"]?["address"]?["country"]?.ToString() ?? string.Empty;

            order.DeliveryCity = city != null ? city.ToString() : region.ToString();


            if (order.AppStatus == null)
            {
                string statusName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    AppStatus appStatus = _context.AppStatuses.FirstOrDefault(c => c.Name == statusName);

                    if (appStatus != null)
                    {
                        order.AppStatus = appStatus;
                    }
                    else
                    {
                        var newStatus = new AppStatus
                        {
                            Name = statusName
                        };

                        _context.AppStatuses.Add(newStatus);
                        _context.SaveChanges();

                        order.AppStatus = newStatus;
                    }
                    transaction.Commit();
                }
            }

            if (order.Supplier == null)
            {
                string supplierName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    Supplier supplier = _context.Suppliers.FirstOrDefault(c => c.Name == supplierName);

                    if (supplier != null)
                    {
                        order.Supplier = supplier;
                    }
                    else
                    {
                        var newSupplier = new Supplier
                        {
                            Name = supplierName
                        };

                        _context.Suppliers.Add(newSupplier);
                        _context.SaveChanges();

                        order.Supplier = newSupplier;
                    }
                    transaction.Commit();
                }
            }
            
            
            decimal? startPrice = decimal.TryParse(DelSubStr(jsonOrder["items"][0]["price"]?.ToString() ?? string.Empty), style, culture, out var startPriceParse)
                ? startPriceParse
                : null;

            decimal? subsidy = 0; 
            var subsidyToken = jsonOrder["items"][0]["subsidy"];
            if (subsidyToken != null)
            {
                subsidy = decimal.TryParse(DelSubStr(subsidyToken.ToString()), style, culture, out var subsidyParse)
                    ? subsidyParse
                    : 0;
            }
            else
            {
                // 2. Если основного subsidy нет, ищем в массиве "subsidies" с типом "SUBSIDY"
                var subsidiesArray = jsonOrder["items"][0]["subsidies"];
                if (subsidiesArray != null && subsidiesArray.Any())
                {
                    var subsidyItem = subsidiesArray.FirstOrDefault(s => s?["type"]?.ToString() == "SUBSIDY");
                    if (subsidyItem != null)
                    {
                        subsidy = decimal.TryParse(DelSubStr(subsidyItem["amount"]?.ToString() ?? string.Empty), style, culture, out var subsidyAmountParse)
                            ? subsidyAmountParse
                            : 0;
                    }
                }
            }

            order.Price = startPrice + subsidy;

            order.ProductInfo.CurrentPriceWithDiscount = decimal.TryParse(DelSubStr(jsonOrder["items"][0]["buyerPrice"].ToString()), 
                                                                 style, culture, out var buyerPrice)
                            ? buyerPrice + subsidy
                            : null;

            order.MaxOzonCommission = 0;
            order.MinOzonCommission = 0;
            order.UpdatedBy = "Yandex";

            order = SetCorrectProductKey(order);

            _context.SaveChanges();

            return order;
        }

        private async Task<Order> CastToModelFromExcel(Dictionary<string, string> dataColumn,
                                                       OzonClient selectedClient,
                                                       Manufacturer selectedManufacturer,
                                                       Warehouse selectedWarehouse,
                                                       Supplier selectedSupplier,
                                                       string selectedClientStatus,
                                                       CurrencyCode selectedCurrencyCode,
                                                       DateTime? selectedShippingDate,
                                                       DateTime? selectedProcessingDate)
        {
            Order order = new();

            CultureInfo culture = new("en-US");
            NumberStyles style = NumberStyles.Number;

            if(dataColumn.TryGetValue("Артикул", out var article))
            {
                order.ProductKey = article?.ToString();
            }
            else if(dataColumn.TryGetValue("Key", out var key))
            {
                order.ProductKey = key.ToString();
                article = key.ToString();
                selectedManufacturer = new Manufacturer() { Id = -1};
            }


            order.Key = (dataColumn.TryGetValue("Номер заказа", out var shipmentNumber) ? shipmentNumber?.ToString() : null)
                      + (order.ProductKey != null ? order.ProductKey : null);

            order.ShipmentNumber = shipmentNumber?.ToString();


            if (dataColumn.TryGetValue("Клиент", out var clientName))
            {
                OzonClient clientByName = _context.OzonClients.FirstOrDefault(o => o.Name == clientName);
                if (clientByName == null)
                {
                    OzonClient newOzonClient = new OzonClient()
                    {
                        Name = clientName,
                        CurrencyCode = CurrencyCode.USD,
                    };

                    _context.OzonClients.Add(newOzonClient);
                    await _context.SaveChangesAsync();

                    order.OzonClient = newOzonClient;
                }
                else
                {
                    order.OzonClient = clientByName;
                }
            }
            else if (selectedClient.Id != 0)
            {
                order.OzonClient = _context.OzonClients.Find(selectedClient.Id);
            }


            if (selectedCurrencyCode != CurrencyCode.NON && order.OzonClient != null)
            {
                if (order.OzonClient.CurrencyCode != selectedCurrencyCode)
                {
                    order.OzonClient.CurrencyCode = selectedCurrencyCode;
                }
            }
            else if (selectedClient.Id != 0)
            {
                order.OzonClient = _context.OzonClients.Find(selectedClient.Id);
            }

            var cultureDateInfo = new CultureInfo("ru-RU");

            order.ProcessingDate = dataColumn.TryGetValue("Принят в обработку", out var processingDateStr) &&
                                   DateTime.TryParse(processingDateStr, cultureDateInfo, DateTimeStyles.None, out var processingDate)
                                   ? processingDate
                                   : selectedProcessingDate != null ? selectedProcessingDate : DateTime.Now.AddHours(_releaseManager.GetTimeZone());


            order.ShippingDate = dataColumn.TryGetValue("Дата отгрузки", out var shippingDateStr) &&
                                 DateTime.TryParse(shippingDateStr, cultureDateInfo, DateTimeStyles.None, out var shippingDate)
                                 ? shippingDate
                                 : selectedShippingDate;

            order.Status = dataColumn.TryGetValue("Статус", out var status) ? SetCorrectStatus(status?.ToString()) : null;



            order.ProductName = dataColumn.TryGetValue("Наименование товара", out var productName) ? productName?.ToString() : null;

            

            order.Quantity = dataColumn.TryGetValue("Кол.", out var quantityStr) &&
                             int.TryParse(DelSubStr(quantityStr), style, culture, out var quantity)
                             ? quantity
                             : (int?)null;

            if (dataColumn.TryGetValue("Склад отгрузки", out var warehouseName))
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    Warehouse shipmentWarehouse = _context.Warehouses.FirstOrDefault(w => w.Name == warehouseName);

                    if (shipmentWarehouse != null)
                    {
                        order.ShipmentWarehouse = shipmentWarehouse;
                    }
                    else
                    {
                        var newWarehouse = new Warehouse { Name = warehouseName };
                        _context.Warehouses.Add(newWarehouse);
                        _context.SaveChanges();
                        order.ShipmentWarehouse = newWarehouse;
                    }
                    transaction.Commit();
                }
            }
            else if (selectedWarehouse != null && selectedWarehouse.Id != 0)
            {
                order.ShipmentWarehouse = _context.Warehouses.FirstOrDefault(w => w.Id == selectedWarehouse.Id);
            }

            if (dataColumn.TryGetValue("Производитель", out var manufacturerName))
            {
                Manufacturer manufacturer = _context.Manufacturers.FirstOrDefault(m => m.Name == manufacturerName);

                if (manufacturer != null)
                {
                    order.Manufacturer = manufacturer;
                }
                else
                {
                    var newManufacturer = new Manufacturer
                    {
                        Code = null,
                        Name = manufacturerName
                    };

                    _context.Manufacturers.Add(newManufacturer);
                    _context.SaveChanges();

                    order.Manufacturer = newManufacturer;
                }

                order = await SetEtProducerByName(order, manufacturerName);
            }
            else if (selectedManufacturer != null && selectedManufacturer.Id != 0)
            {
                if (selectedManufacturer.Id == -1)
                {
                    string extractedCode = Regex.Match(article.ToString(), @"=(\w{3})").Groups[1].Value;

                    if (string.IsNullOrEmpty(extractedCode))
                    {
                        extractedCode = Regex.Match(article.ToString(), @"=(\w{2})").Groups[1].Value;
                    }

                    Manufacturer manufacturer = _context.Manufacturers
                        .Where(m => m.Code == extractedCode)
                        .FirstOrDefault();

                    if (manufacturer != null)
                    {
                        order.Manufacturer = manufacturer;
                    }
                    else
                    {
                        var newManufacturer = new Manufacturer { Code = extractedCode };
                        _context.Manufacturers.Add(newManufacturer);
                        _context.SaveChanges();
                        order.Manufacturer = newManufacturer;
                    }
                    
                    order = await SetEtProducerByCode(order, extractedCode);
                }
                else if(selectedManufacturer.Id == -2)
                {
                    string tabelManufacturerName = order.ProductName != null ? order.ProductName.Split(' ')[0] : null;
                    if(tabelManufacturerName != null)
                    {
                        Manufacturer manufacturer = _context.Manufacturers.Where(m => m.Name.ToLower().Contains(tabelManufacturerName.ToLower())).FirstOrDefault();
                        if (manufacturer != null)
                        {
                            order.Manufacturer = manufacturer;
                        }
                    }
                }
                else
                {
                    order.Manufacturer = _context.Manufacturers.FirstOrDefault(m => m.Id == selectedManufacturer.Id);
                }
            }


            if (dataColumn.TryGetValue("Код валюты отправления", out var currencyName))
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    Currency currency = _context.Currencys.FirstOrDefault(c => c.Name == currencyName);

                    if (currency != null)
                    {
                        order.Сurrency = currency;
                    }
                    else
                    {
                        var newCurrency = new Currency
                        {
                            Name = currencyName
                        };

                        _context.Currencys.Add(newCurrency);
                        _context.SaveChanges();

                        order.Сurrency = newCurrency;
                    }

                    // Завершаем транзакцию
                    transaction.Commit();
                }
            }

            order.Supplier = null;
            order.PurchasePrice = null;

            if (order.ProductInfo == null && dataColumn.TryGetValue("Артикул", out var productArticle))
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    Product product = _context.Products.FirstOrDefault(p => p.Article == productArticle);

                    if (product != null)
                    {
                        order.ProductInfo = product;
                    }
                    else
                    {
                        product = new Product
                        {
                            Article = productArticle
                        };

                        _context.Products.Add(product);
                        _context.SaveChanges();

                        order.ProductInfo = product;
                    }
                    transaction.Commit();
                }
            }
            else if(order.ProductInfo == null && !dataColumn.TryGetValue("Артикул", out var nullProductArticle))
            {
                var product = new Product();

                _context.Products.Add(product);
                _context.SaveChanges();

                order.ProductInfo = product;
            }

            order.DeliveryCity = dataColumn.TryGetValue("Город доставки", out var deliveryCity) ? deliveryCity?.ToString() : null;

            if (dataColumn.TryGetValue("Статус клинта", out var clientStauts))
            {
                order.Status = clientStauts;
            }
            else if (selectedClientStatus != null)
            {
                order.Status = selectedClientStatus;
            }

            if (order.AppStatus == null)
            {
                string statusName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    AppStatus appStatus = _context.AppStatuses.FirstOrDefault(c => c.Name == statusName);

                    if (appStatus != null)
                    {
                        order.AppStatus = appStatus;
                    }
                    else
                    {
                        var newStatus = new AppStatus { Name = statusName };

                        _context.AppStatuses.Add(newStatus);
                        _context.SaveChanges();

                        order.AppStatus = newStatus;
                    }
                    transaction.Commit();
                }
            }

            if (order.Supplier == null && selectedSupplier.Id == 0)
            {
                string supplierName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    Supplier supplier = _context.Suppliers.FirstOrDefault(c => c.Name == supplierName);

                    if (supplier != null)
                    {
                        order.Supplier = supplier;
                    }
                    else
                    {
                        var newSupplier = new Supplier { Name = supplierName };

                        _context.Suppliers.Add(newSupplier);
                        _context.SaveChanges();

                        order.Supplier = newSupplier;
                    }
                    transaction.Commit();
                }
            }
            else if (selectedSupplier.Id != 0)
            {
                order.Supplier = _context.Suppliers.FirstOrDefault(m => m.Id == selectedSupplier.Id);
            }

            order.Price = dataColumn.TryGetValue("Цена", out var priceStr) &&
                                   decimal.TryParse(DelSubStr(priceStr), style, culture, out var price)
                                   ? price
                                   : (decimal?)null;

            if (dataColumn.TryGetValue("Сумма отправления", out var shipmentAmountStr) &&
                       decimal.TryParse(DelSubStr(shipmentAmountStr), style, culture, out var shipmentAmount))
            {
                order.ShipmentAmount = shipmentAmount;
            }
            else if (order.Price != (decimal?)null && order.Quantity != (int?)null)
            {
                order.ShipmentAmount = order.Price * order.Quantity;
            }

            order.ProductInfo.CurrentPriceWithDiscount = dataColumn.TryGetValue("Цена сайта", out var priceWithDiscountStr) &&
                                   decimal.TryParse(priceWithDiscountStr, style, culture, out var priceWithDiscount)
                                   ? priceWithDiscount
                                   : 0;

            order.MaxOzonCommission = dataColumn.TryGetValue("Максимальная комиссия", out var maxOzonCommissionStr) &&
                                   decimal.TryParse(maxOzonCommissionStr, style, culture, out var maxOzonCommission)
                                   ? maxOzonCommission
                                   : 0;

            order.MinOzonCommission = dataColumn.TryGetValue("Минимальная комиссия", out var minOzonCommissionStr) &&
                                   decimal.TryParse(minOzonCommissionStr, style, culture, out var minOzonCommission)
                                   ? minOzonCommission
                                   : 0;

            order.PurchasePrice = dataColumn.TryGetValue("Цена закупки", out var purchasePriceStr) &&
                                   decimal.TryParse(purchasePriceStr, style, culture, out var purchasePrice)
                                   ? purchasePrice
                                   : 0;

            order = SetCorrectProductKey(order);

            order.FromFile = true;
            order.UpdatedBy = "Excel";

            _context.SaveChanges();

            return order;
        }
        
        private async Task<Order> CastToModelFromExcelForDropBox(Dictionary<string, string> dataColumn,
                                                       OzonClient selectedClient,
                                                       Manufacturer selectedManufacturer,
                                                       Warehouse selectedWarehouse,
                                                       Supplier selectedSupplier,
                                                       string selectedClientStatus,
                                                       CurrencyCode selectedCurrencyCode,
                                                       DateTime? selectedShippingDate,
                                                       DateTime? selectedProcessingDate)
        {
            Order order = new();

            CultureInfo culture = new("en-US");
            NumberStyles style = NumberStyles.Number;
            string regionCode = _releaseManager.GetRegionCode();

            if(dataColumn.TryGetValue("Артикул", out var article))
            {
                order.ProductKey = article?.ToString();
            }
            else if(dataColumn.TryGetValue("Key", out var key))
            {
                order.ProductKey = key.ToString();
                article = key.ToString();
                selectedManufacturer = new Manufacturer() { Id = -1};
            }


            order.Key = (dataColumn.TryGetValue("Номер заказа", out var shipmentNumber) ? shipmentNumber?.ToString() : null)
                      + (order.ProductKey != null ? order.ProductKey : null);

            order.ShipmentNumber = shipmentNumber?.ToString();


            if (dataColumn.TryGetValue("Клиент", out var clientName))
            {
                OzonClient clientByName = _context.OzonClients.FirstOrDefault(o => o.Name == clientName);
                if (clientByName == null)
                {
                    OzonClient newOzonClient = new OzonClient()
                    {
                        Name = clientName,
                        CurrencyCode = CurrencyCode.USD,
                    };

                    _context.OzonClients.Add(newOzonClient);
                    await _context.SaveChangesAsync();

                    order.OzonClient = newOzonClient;
                }
                else
                {
                    order.OzonClient = clientByName;
                }
            }
            else if (selectedClient.Id != 0)
            {
                order.OzonClient = _context.OzonClients.Find(selectedClient.Id);
            }


            if (selectedCurrencyCode != CurrencyCode.NON && order.OzonClient != null)
            {
                if (order.OzonClient.CurrencyCode != selectedCurrencyCode)
                {
                    order.OzonClient.CurrencyCode = selectedCurrencyCode;
                }
            }
            else if (selectedClient.Id != 0)
            {
                order.OzonClient = _context.OzonClients.Find(selectedClient.Id);
            }

            var cultureDateInfo = new CultureInfo("ru-RU");

            order.ProcessingDate = dataColumn.TryGetValue("Принят в обработку", out var processingDateStr) &&
                                   DateTime.TryParse(processingDateStr, cultureDateInfo, DateTimeStyles.None, out var processingDate)
                                   ? processingDate
                                   : selectedProcessingDate != null ? selectedProcessingDate : DateTime.Now.AddHours(_releaseManager.GetTimeZone());


            order.ShippingDate = dataColumn.TryGetValue("Дата отгрузки", out var shippingDateStr) &&
                                 DateTime.TryParse(shippingDateStr, cultureDateInfo, DateTimeStyles.None, out var shippingDate)
                                 ? shippingDate
                                 : selectedShippingDate;

            order.Status = dataColumn.TryGetValue("Статус", out var status) ? SetCorrectStatus(status?.ToString()) : null;



            order.ProductName = dataColumn.TryGetValue("Наименование товара", out var productName) ? productName?.ToString() : null;

            

            order.Quantity = dataColumn.TryGetValue("Кол.", out var quantityStr) &&
                             int.TryParse(DelSubStr(quantityStr), style, culture, out var quantity)
                             ? quantity
                             : (int?)null;

            if (dataColumn.TryGetValue("Склад отгрузки", out var warehouseName))
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    Warehouse shipmentWarehouse = _context.Warehouses.FirstOrDefault(w => w.Name == warehouseName);

                    if (shipmentWarehouse != null)
                    {
                        order.ShipmentWarehouse = shipmentWarehouse;
                    }
                    else
                    {
                        var newWarehouse = new Warehouse { Name = warehouseName };
                        _context.Warehouses.Add(newWarehouse);
                        _context.SaveChanges();
                        order.ShipmentWarehouse = newWarehouse;
                    }
                    transaction.Commit();
                }
            }
            else if (selectedWarehouse != null && selectedWarehouse.Id != 0)
            {
                order.ShipmentWarehouse = _context.Warehouses.FirstOrDefault(w => w.Id == selectedWarehouse.Id);
            }

            if (dataColumn.TryGetValue("Производитель", out var manufacturerName))
            {
                Manufacturer manufacturer = _context.Manufacturers.FirstOrDefault(m => m.Name == manufacturerName);

                if (manufacturer != null)
                {
                    order.Manufacturer = manufacturer;
                }
                else
                {
                    var newManufacturer = new Manufacturer
                    {
                        Code = null,
                        Name = manufacturerName
                    };

                    _context.Manufacturers.Add(newManufacturer);
                    _context.SaveChanges();

                    order.Manufacturer = newManufacturer;
                }

                order = await SetEtProducerByName(order, manufacturerName);
            }
            else if (selectedManufacturer != null && selectedManufacturer.Id != 0)
            {
                if (selectedManufacturer.Id == -1)
                {
                    string extractedCode = Regex.Match(article.ToString(), @"=(\w{3})").Groups[1].Value;

                    if (string.IsNullOrEmpty(extractedCode))
                    {
                        extractedCode = Regex.Match(article.ToString(), @"=(\w{2})").Groups[1].Value;
                    }

                    Manufacturer manufacturer = _context.Manufacturers
                        .Where(m => m.Code == extractedCode)
                        .FirstOrDefault();

                    if (manufacturer != null)
                    {
                        order.Manufacturer = manufacturer;
                    }
                    else
                    {
                        var newManufacturer = new Manufacturer { Code = extractedCode };
                        _context.Manufacturers.Add(newManufacturer);
                        _context.SaveChanges();
                        order.Manufacturer = newManufacturer;
                    }
                    
                    order = await SetEtProducerByCode(order, extractedCode);
                }
                else if(selectedManufacturer.Id == -2)
                {
                    string tabelManufacturerName = order.ProductName != null ? order.ProductName.Split(' ')[0] : null;
                    if(tabelManufacturerName != null)
                    {
                        Manufacturer manufacturer = _context.Manufacturers.Where(m => m.Name.ToLower().Contains(tabelManufacturerName.ToLower())).FirstOrDefault();
                        if (manufacturer != null)
                        {
                            order.Manufacturer = manufacturer;
                        }
                    }
                }
                else
                {
                    order.Manufacturer = _context.Manufacturers.FirstOrDefault(m => m.Id == selectedManufacturer.Id);
                }
            }


            if (dataColumn.TryGetValue("Код валюты отправления", out var currencyName))
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    Currency currency = _context.Currencys.FirstOrDefault(c => c.Name == currencyName);

                    if (currency != null)
                    {
                        order.Сurrency = currency;
                    }
                    else
                    {
                        var newCurrency = new Currency
                        {
                            Name = currencyName
                        };

                        _context.Currencys.Add(newCurrency);
                        _context.SaveChanges();

                        order.Сurrency = newCurrency;
                    }

                    // Завершаем транзакцию
                    transaction.Commit();
                }
            }

            order.Supplier = null;
            order.PurchasePrice = null;

            if (order.ProductInfo == null && dataColumn.TryGetValue("Артикул", out var productArticle))
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    Product product = _context.Products.FirstOrDefault(p => p.Article == productArticle);

                    if (product != null)
                    {
                        order.ProductInfo = product;
                    }
                    else
                    {
                        product = new Product
                        {
                            Article = productArticle
                        };

                        _context.Products.Add(product);
                        _context.SaveChanges();

                        order.ProductInfo = product;
                    }
                    transaction.Commit();
                }
            }
            else if(order.ProductInfo == null && !dataColumn.TryGetValue("Артикул", out var nullProductArticle))
            {
                var product = new Product();

                _context.Products.Add(product);
                _context.SaveChanges();

                order.ProductInfo = product;
            }

            order.DeliveryCity = dataColumn.TryGetValue("Город доставки", out var deliveryCity) ? deliveryCity?.ToString() : null;

            if (dataColumn.TryGetValue("Статус клинта", out var clientStauts))
            {
                order.Status = clientStauts;
            }
            else if (selectedClientStatus != null)
            {
                order.Status = selectedClientStatus;
            }

            if (order.AppStatus == null)
            {
                string statusName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    AppStatus appStatus = _context.AppStatuses.FirstOrDefault(c => c.Name == statusName);

                    if (appStatus != null)
                    {
                        order.AppStatus = appStatus;
                    }
                    else
                    {
                        var newStatus = new AppStatus { Name = statusName };

                        _context.AppStatuses.Add(newStatus);
                        _context.SaveChanges();

                        order.AppStatus = newStatus;
                    }
                    transaction.Commit();
                }
            }

            if (order.Supplier == null && selectedSupplier.Id == 0)
            {
                string supplierName = "Не указан";

                using (var transaction = _context.Database.BeginTransaction())
                {
                    Supplier supplier = _context.Suppliers.FirstOrDefault(c => c.Name == supplierName);

                    if (supplier != null)
                    {
                        order.Supplier = supplier;
                    }
                    else
                    {
                        var newSupplier = new Supplier { Name = supplierName };

                        _context.Suppliers.Add(newSupplier);
                        _context.SaveChanges();

                        order.Supplier = newSupplier;
                    }
                    transaction.Commit();
                }
            }
            else if (selectedSupplier.Id != 0)
            {
                order.Supplier = _context.Suppliers.FirstOrDefault(m => m.Id == selectedSupplier.Id);
            }
            
            var numberStyles = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;

            if (order.Price == null && 
                    dataColumn.TryGetValue("Целая часть", out var integerPartStr) &&
                    dataColumn.TryGetValue("Дробная часть", out var fractionalPartStr))
            {
                    if (int.TryParse(integerPartStr, out var integerPart) &&
                        int.TryParse(fractionalPartStr, out var fractionalPart))
                    {
                        order.Price = integerPart + (decimal)fractionalPart / 100;
                    }
            }
            
            if (dataColumn.TryGetValue("Сумма отправления", out var shipmentAmountStr) &&
                decimal.TryParse(DelSubStr(shipmentAmountStr), numberStyles, cultureDateInfo, out var shipmentAmount))
            {
                order.ShipmentAmount = shipmentAmount;
            }
            else if (order.Price != null && order.Quantity != null)
            {
                order.ShipmentAmount = order.Price * order.Quantity;
            }

            order.ProductInfo.CurrentPriceWithDiscount = dataColumn.TryGetValue("Цена сайта", out var priceWithDiscountStr) 
                ? decimal.TryParse(priceWithDiscountStr, numberStyles, cultureDateInfo, out var priceWithDiscount) 
                    ? priceWithDiscount 
                    : 0 
                : 0;

            order.MaxOzonCommission = dataColumn.TryGetValue("Максимальная комиссия", out var maxOzonCommissionStr) 
                ? decimal.TryParse(maxOzonCommissionStr, numberStyles, cultureDateInfo, out var maxOzonCommission) 
                    ? maxOzonCommission 
                    : 0 
                : 0;

            order.MinOzonCommission = dataColumn.TryGetValue("Минимальная комиссия", out var minOzonCommissionStr) 
                ? decimal.TryParse(minOzonCommissionStr, numberStyles, cultureDateInfo, out var minOzonCommission) 
                    ? minOzonCommission 
                    : 0 
                : 0;

            order.PurchasePrice = dataColumn.TryGetValue("Цена закупки", out var purchasePriceStr) 
                ? decimal.TryParse(purchasePriceStr, numberStyles, cultureDateInfo, out var purchasePrice) 
                    ? purchasePrice 
                    : 0 
                : 0;


            order = SetCorrectProductKey(order);

            order.FromFile = true;
            order.UpdatedBy = "Excel";

            _context.SaveChanges();

            return order;
        }

        private Order SetCorrectProductKey(Order order)
        {
            string pattern = @"^[A-Za-z0-9\-[\]]+=[A-Za-z0-9\[\]]+$";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(order.ProductKey);

            if (match.Success)
            {
                order.Article = Regex.Replace(order.ProductKey.Substring(0, order.ProductKey.IndexOf('=')), "[^A-Za-z0-9]", "");
                return order;
            }

            string cleanedArticle = Regex.Replace(order.ProductKey, "[^A-Za-z0-9]", "");
            string key = order.EtProducer != null && order.EtProducer.MarketPrefix != null ? $"={order.EtProducer.MarketPrefix}" : "=";

            order.Article = order.ProductKey;
            order.ProductKey = cleanedArticle + key;

            return order;
        }

        private string SetCorrectStatus(string status)
        {
            if(status == null)
            {
                return null;
            }

            if (status == "Отменен")
            {
                status = "Отменён";
            }
            return status;
        }

        private (decimal, decimal, string, string, decimal, decimal) CalculateСommissions(JToken jsonOrder, Order order)
        {
            JToken productPrices = jsonOrder["productPricesWithArticle"];
            JToken productСommissions = productPrices["commissions"];
            string regionCode = _releaseManager.GetRegionCode();

            decimal priceWithDiscount = ConvertNumberByRegion(productPrices["price"]["marketing_price"].ToString(), regionCode);
            //decimal price = ConvertNumberByRegion(jsonOrder["Итоговая стоимость товара"].ToString(), regionCode); 
            
            var product = jsonOrder["productWarehousesAndCitysWithNumber"]?["products"]
            ?.FirstOrDefault(p => p?["offer_id"]?.ToString() == order.ProductKey);
            decimal price = 0;
            if (product != null)
            {
                string priceStr = product["price"]?.ToString();
                price = ConvertNumberByRegion(priceStr, regionCode);
            }

            decimal percentFbs = ConvertNumberByRegion(productСommissions["sales_percent_fbs"].ToString(), regionCode);
            decimal acquiring = ConvertNumberByRegion(productPrices["acquiring"].ToString(), regionCode);
            decimal delivToCustomer = ConvertNumberByRegion(productСommissions["fbs_deliv_to_customer_amount"].ToString(), regionCode);
            decimal fbsFirstMileMax = ConvertNumberByRegion(productСommissions["fbs_first_mile_max_amount"].ToString(), regionCode);
            decimal fbsDirectFlowTransMax = ConvertNumberByRegion(productСommissions["fbs_direct_flow_trans_max_amount"].ToString(), regionCode);
            decimal fbsFirstMileMin = ConvertNumberByRegion(productСommissions["fbs_first_mile_min_amount"].ToString(), regionCode);
            decimal fbsDirectFlowTransMin = ConvertNumberByRegion(productСommissions["fbs_direct_flow_trans_min_amount"].ToString(), regionCode);

            decimal salesСommission = price / 100 * percentFbs;
            decimal standaertSum = acquiring + delivToCustomer + salesСommission;

            decimal maxСommission = standaertSum + fbsFirstMileMax + fbsDirectFlowTransMax;
            decimal minCommission = standaertSum + fbsFirstMileMin + fbsDirectFlowTransMin;


            string sb1 = $" = Комиссия за продажу ({salesСommission}) + Максимальная комиссия за эквайринг({acquiring}) +" +
                          $"Последняя миля (FBS) ({delivToCustomer}) + Максимальная комиссия за обработку отправления (FBS) — 25 рублей ({fbsFirstMileMax}) +" +
                          $"Магистраль до (FBS) ({fbsDirectFlowTransMax})";

            string sb2 = $" = Комиссия за продажу ({salesСommission}) + Максимальная комиссия за эквайринг({acquiring}) +" +
                          $"Последняя миля (FBS) ({delivToCustomer}) + Минимальная комиссия за обработку отправления (FBS) — 0 рублей ({fbsFirstMileMin}) +" +
                          $"Магистраль от (FBS) ({fbsDirectFlowTransMin})";

            var (maxComment, minComment, min, max) = (sb1, sb2, minCommission, maxСommission);

            return (price, priceWithDiscount, maxComment, minComment, min, max);
        }

        public decimal ConvertNumberByRegion(string number, string regionCode)
        {
            return regionCode switch
            {
                "en" => decimal.Parse(number.Replace(',', '.')),
                "ru" => decimal.Parse(number.Replace('.', ',')),
                _ => decimal.Parse(number.Replace('.', ','))
            };
        }
        
        public decimal ConvertNumberByRegionForExcel(string number, string regionCode)
        {
            if (string.IsNullOrWhiteSpace(number))
                return 0;
            
            number = number.Replace(" ", "");

            return regionCode switch
            {
                "en-US" or "en" => decimal.Parse(number.Replace(',', '.')),
                "ru-RU" or "ru" => decimal.Parse(number.Replace('.', ',')),
                _ => decimal.Parse(number.Replace('.', ',')) 
            };
        }
        
        private string DelSubStr(string str)
        {
            if (str.StartsWith("'"))
            {
                return str.Substring(1);
            }
            return str;
        }

        private async Task<Order> SetEtProducerByCode(Order order, string extractedCode)
        {
            var producer = await _etProducerDataServices.GetRealIdAsyncByCode(extractedCode);
            if (producer != null)
            {
                order.EtProducerId = producer.Id;
                order.EtProducer = producer;
            }
            return order;
        }
        
        private async Task<Order> SetEtProducerByName(Order order, string name)
        {
            var producer = await _etProducerDataServices.GetRealIdAsyncByName(name);
            if (producer != null)
            {
                order.EtProducerId = producer.Id;
                order.EtProducer = producer;
            }
            return order;
        }
    }
}