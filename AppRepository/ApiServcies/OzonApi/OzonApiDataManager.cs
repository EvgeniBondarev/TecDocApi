using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.OzonApi.Filters;
using Servcies.ParserServcies;
using System.Globalization;
using OzonDomains.Models;

namespace Servcies.ApiServcies.OzonApi
{
    public class OzonApiDataManager : IApiDataManager<OzonApiDataManager>
    {
        private OzonApiClient ozonApiClient;
        private readonly CsvUrlParser csvUrlParser;
        private readonly Dictionary<string, string>  StatusDescriptions = new Dictionary<string, string>
                                                                        {
                                                                            { "waiting", "в очереди на обработку" },
                                                                            { "processing", "обрабатывается" },
                                                                            { "success", "отчёт успешно создан" },
                                                                            { "failed", "ошибка при создании отчёта" }
                                                                        };

        public async Task<bool> GetTestRequest(string clientId, string apiKey)
        {
            return await ozonApiClient.GetTestRequest(clientId, apiKey);
        }
        public OzonApiDataManager(string clientId, string apiKey)
        {
            ozonApiClient = new OzonApiClient(clientId, apiKey);
            csvUrlParser = new CsvUrlParser();
        }
        public OzonApiDataManager SetClient(string clientId, string apiKey)
        {
            ozonApiClient = new OzonApiClient(clientId, apiKey);
            return this;
        }

        public async Task<string> GetReportCode(DateTime start, DateTime end,
                                                string uri = OzonApiUrl.REPORT_CODE,
                                                string deliverySchema = "fbs")
        {
            CultureInfo russianCulture = new CultureInfo("ru-RU");

            var postingsReportRequest = new PostingsReportRequest
            {

                Filter = new PostingsReportRequestFilter
                {
                    ProcessedAtFrom = start, // DateTime.Now.AddHours(-period * 24 + timeZone),
                    ProcessedAtTo = end, // DateTime.Now.AddHours(timeZone),
                    DeliverySchema = [deliverySchema],
                    Sku = [],
                    CancelReasonId = [],
                    OfferId = "",
                    StatusAlias = [],
                    Statuses = [],
                    Title = ""
                },
                Language = "DEFAULT"
            };


            var postingsResponse = ozonApiClient.MakeRequestPost(postingsReportRequest, uri);

            return postingsResponse["result"]["code"].ToString();
        }

        public async Task<string> GetReportFile(string code, string uri = OzonApiUrl.REPORT_FILE)
        {

            var reportInfoRequest = new ReportInfoRequest
            {
                Code = code
            };
            var infoResponse = ozonApiClient.MakeRequestPost(reportInfoRequest, uri);

            if (infoResponse["result"]["status"].ToString() == "success")
            {
                return infoResponse["result"]["file"].ToString();
            }
            else
            {
                throw new Exception(message: StatusDescriptions[infoResponse["result"]["status"].ToString()]);
            }  
        }

        public async Task<JArray> ReadFileByUrl(string url)
        {
            var fileContent = await csvUrlParser.ReadFileFromUrl(url);
            return fileContent;
        }

        public async Task<string> GetProductCode(string[] articles,
                                                  string[] ozonIds,
                                                  string uri = OzonApiUrl.PRODUCT_CODE)
        {
            var productsReportRequest = new ProductsReportRequest
            {
                Language = "DEFAULT",
                OfferId = articles,
                Search = "",
                Sku = ozonIds,
                Visibility = "ALL"
            };

            var productsReportResponse = ozonApiClient.MakeRequestPost(productsReportRequest, uri);
            return productsReportResponse["result"]["code"].ToString();

        }

        public async Task<JObject> GetProductPrices(string[] articles,
                                                   string[] ozonProductIds,
                                                   string uri = OzonApiUrl.PRODUCT_PRICES)
        {
            for (int i = 0; i < articles.Length; i++)
            {
                if (articles[i].StartsWith("'"))
                {
                    articles[i] = articles[i].Substring(1);
                }
            }
            var productPricesRequest = new ProductPricesRequest
            {
                Filter = new ProductPricesRequestFilter
                {
                    OfferId = articles,
                    ProductId = ozonProductIds,
                    Visibility = "ALL"
                },
                LastId = "",
                Limit = 500
            };
            var productPricesResponse = ozonApiClient.MakeRequestPost(productPricesRequest, uri);
            return productPricesResponse;

        }
        
        public async Task<ProductPricesResponse> GetProductPricesByArticlesModel(
            string[] articles,
            string uri = OzonApiUrl.PRODUCT_PRICES)
        {
            for (int i = 0; i < articles.Length; i++)
            {
                if (articles[i].StartsWith("'"))
                {
                    articles[i] = articles[i].Substring(1);
                }
            }

            var productPricesRequest = new ProductPricesRequest
            {
                Filter = new ProductPricesRequestFilter
                {
                    OfferId = articles,
                    Visibility = "ALL"
                },
                LastId = "",
                Limit = 500
            };

            var productPricesResponse = ozonApiClient.MakeRequestPostForBitrix(productPricesRequest, uri);

            // Десериализация в модель
            return productPricesResponse.ToObject<ProductPricesResponse>();
        }
        
        public async Task<JObject> GetProductPricesByArticles(string[] articles,
            string uri = OzonApiUrl.PRODUCT_PRICES)
        {
            for (int i = 0; i < articles.Length; i++)
            {
                if (articles[i].StartsWith("'"))
                {
                    articles[i] = articles[i].Substring(1);
                }
            }
            var productPricesRequest = new ProductPricesRequest
            {
                Filter = new ProductPricesRequestFilter
                {
                    OfferId = articles,
                    Visibility = "ALL"
                },
                LastId = "",
                Limit = 500
            };

            var productPricesResponse = ozonApiClient.MakeRequestPostForBitrix(productPricesRequest, uri);
            return productPricesResponse;
        }


        public async Task<JObject> GetProductWarehouses(string[] sku,
                                                       string[] fbsSku,
                                                       string uri = OzonApiUrl.PRODUCT_WAERHOUSES) 
        {
            var stocksByWarehouseRequest = new StocksByWarehouseRequest
            {
                Sku = sku,
                FbsSku = fbsSku
            };
            var stocksByWarehouseResponse = ozonApiClient.MakeRequestPost(stocksByWarehouseRequest, uri);
            return stocksByWarehouseResponse;
        }
        
        public async Task<JObject> GetProductWarehousesForBitrix(
            string[] sku,
            string[] fbsSku,
            string uri = OzonApiUrl.PRODUCT_WAERHOUSES)
        {
            try
            {
                var stocksByWarehouseRequest = new StocksByWarehouseRequest
                {
                    Sku = sku,
                    FbsSku = fbsSku
                };

                var stocksByWarehouseResponse = ozonApiClient.MakeRequestPostForBitrix(stocksByWarehouseRequest, uri);
                return stocksByWarehouseResponse ?? new JObject();
            }
            catch (Exception ex)
            {
                return new JObject();
            }
        }

        
        public async Task<JToken?> GetWarehouseInfo(JObject stocks, string article, OzonClient client, List<WarehouseOzon> warehouseList)
        {
            if (stocks == null || stocks["items"] == null || !stocks["items"].HasValues)
                return null;

            foreach (var stockItem in stocks["items"])
            {
                if (stockItem["stocks"] == null || !stockItem["stocks"].HasValues)
                    continue;

                foreach (var stock in stockItem["stocks"])
                {
                    var sku = stock["sku"]?.ToString();
                    if (string.IsNullOrEmpty(sku))
                        continue;

                    var warehouseInfo = await GetProductWarehousesForBitrix(
                        new[] { sku },
                        Array.Empty<string>()
                    );

                    if (warehouseInfo != null && warehouseList != null)
                    {
                        return CombineWarehouseInfoJson(warehouseInfo, warehouseList);
                    }
                    else
                    {
                        return warehouseInfo;
                    }
                }
            }

            return null;
        }
        
        private JObject CombineWarehouseInfoJson(JObject warehouseInfo, List<WarehouseOzon> warehouseList)
        {
            try
            {
                if (warehouseInfo?["result"] == null)
                    return warehouseInfo;

                var existingWarehouses = warehouseInfo["result"].ToObject<List<WarehouseStock>>();
            
                if (warehouseList == null || !warehouseList.Any())
                    return warehouseInfo;
            
                var missingWarehouses = new List<WarehouseStock>();
                foreach (var wh in warehouseList)
                {
                    if (!existingWarehouses.Any(ew => ew.warehouse_name == wh.Name))
                    {
                        missingWarehouses.Add(new WarehouseStock
                        {
                            product_id = existingWarehouses.FirstOrDefault()?.product_id ?? 0,
                            present = 0,
                            reserved = 0,
                            sku = existingWarehouses.FirstOrDefault()?.sku ?? 0,
                            warehouse_id = wh.WarehouseId,
                            warehouse_name = wh.Name,
                            fbs_sku = existingWarehouses.FirstOrDefault()?.fbs_sku ?? 0
                        });
                    }
                }

                var allWarehousesCombined = existingWarehouses.Concat(missingWarehouses).ToList();
                return new JObject
                {
                    ["result"] = JArray.FromObject(allWarehousesCombined)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error combining warehouse data: {ex.Message}");
                return warehouseInfo;
            }
        }
        
        public async Task<JObject> GetProductStocks(string[] offerIds,
            string cursor = "",
            int limit = 100,
            string uri = OzonApiUrl.PRODUCT_STOCKS)
        {
            for (int i = 0; i < offerIds.Length; i++)
            {
                if (offerIds[i].StartsWith("'"))
                {
                    offerIds[i] = offerIds[i].Substring(1);
                }
            }

            var productStocksRequest = new ProductStocksRequest
            {
                Cursor = cursor,
                Filter = new ProductStocksRequestFilter
                {
                    OfferId = offerIds
                },
                Limit = limit
            };
            var productStocksResponse = ozonApiClient.MakeRequestPostForBitrix(productStocksRequest, uri);
            return productStocksResponse;
        }

        public async Task<JObject> GetProductWarehousAndCity(string productNumber,
                                                       string uri = OzonApiUrl.WAERHOUS_AND_CITY)
        {
            var productAnalyticsRequest = new ProductAnalyticsRequest
            {
                PostingNumber = productNumber,

                With = new ProductAnalyticsRequestFilter
                {
                    AnalyticsData = true,
                    Barcodes = false,
                    FinancialData = true,
                    ProductExemplars = false,
                    Translit = false
                }
            };

            var productAnalyticsResponse = ozonApiClient.MakeRequestPost(productAnalyticsRequest, uri);
            return productAnalyticsResponse;
        }

        public async Task<string> GetProductSatatus(string productNumber,
                                                       string uri = OzonApiUrl.WAERHOUS_AND_CITY)
        {
            try
            {
                var productAnalyticsRequest = new ProductAnalyticsRequest
                {
                    PostingNumber = productNumber,

                    With = new ProductAnalyticsRequestFilter
                    {
                        AnalyticsData = true,
                        Barcodes = false,
                        FinancialData = true,
                        ProductExemplars = false,
                        Translit = false
                    }
                };

                var productAnalyticsResponse = ozonApiClient.MakeFastRequest(productAnalyticsRequest, uri);
                return productAnalyticsResponse["result"]["status"].ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<MemoryStream> GetOrderLabel(string shipmentNumber, string uri = OzonApiUrl.ORDER_LABLE)
        {
            var orderLableRequest = new OrdersLableRequest
            {
                ShipmentNumber = new[] { shipmentNumber }
            };

            var response = await ozonApiClient.MakeFileRequestAsync(orderLableRequest, uri);
            return response;
        }

        public async Task<bool> CheckOrderForExistence(string[] articles,
                                                       string[] ozonProductIds,
                                                       string uri = OzonApiUrl.PRODUCT_PRICES)
        {
            for (int i = 0; i < articles.Length; i++)
            {
                if (articles[i].StartsWith("'"))
                {
                    articles[i] = articles[i].Substring(1);
                }
            }

            var productPricesRequest = new ProductPricesRequest
            {
                Filter = new ProductPricesRequestFilter
                {
                    OfferId = articles,
                    ProductId = ozonProductIds,
                    Visibility = "ALL"
                },
                LastId = "",
                Limit = 500
            };
            try
            {
                var productPricesResponse = ozonApiClient.MakeRequestPost(productPricesRequest, uri);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<UpdateStocksResponse> UpdateProductStocks(
            List<StockItemForUpdate> stocks,
            string uri = OzonApiUrl.UPDATE_PRODUCT_STOCKS)
        {
            var request = new UpdateStocksRequest
            {
                Stocks = stocks
            };

            var response = ozonApiClient.MakeFastRequest(request, uri);

            return response.ToObject<UpdateStocksResponse>();
        }
        
        public async Task<List<WarehouseOzon>> GetWarehouseList(string uri = OzonApiUrl.WAREHOUSE_LIST)
        {
            try
            {
                var emptyRequest = new EmptyRequest();
                var response = ozonApiClient.MakeRequestPost(emptyRequest, uri);
                var warehouseResponse = response.ToObject<WarehouseResponse>();
                
                return warehouseResponse?.Warehouses ?? new List<WarehouseOzon>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting warehouse list: {ex.Message}");
                return new List<WarehouseOzon>();
            }
        }
        
        public async Task<RolesResponse> GetApiRoles(string uri = OzonApiUrl.ROLES)
        {
            try
            {
                var emptyRequest = new EmptyRequest();
                var response = ozonApiClient.MakeRequestPost(emptyRequest, uri);
                var rolesResponse = response.ToObject<RolesResponse>();
                return rolesResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting API roles: {ex.Message}");
                return new RolesResponse { Roles = new List<Role>() };
            }
        }
        
        public async Task<ProductImportPricesResponse> ImportProductPrices(
            List<ProductPriceItem> prices,
            string uri = OzonApiUrl.PRODUCT_IMPORT_PRICES)
        {
            var request = new ProductImportPricesRequest
            {
                Prices = prices
            };

            var response = ozonApiClient.MakeFastRequest(request, uri);
            return response.ToObject<ProductImportPricesResponse>();
        }
    }
}
