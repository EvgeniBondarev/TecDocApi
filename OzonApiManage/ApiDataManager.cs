using Newtonsoft.Json.Linq;
using OzonApiManage.Filters;
using OzonParser;

namespace OzonApiManage
{
    public class ApiDataManager
    {
        private readonly string clientId;
        private readonly string apiKey;

        private readonly OzonApiClient ozonApiClient;
        private readonly CsvUrlParser csvUrlParser;

        public ApiDataManager(string clientId, string apiKey)
        {
            this.clientId = clientId;
            this.apiKey = apiKey;

            ozonApiClient = new OzonApiClient(clientId, apiKey);
            csvUrlParser = new CsvUrlParser();
        }

        public async Task<string> GetReportCode(int period = 7,
                                                string uri = "https://api-seller.ozon.ru/v1/report/postings/create",
                                                string deliverySchema = "fbs")
        {
            var postingsReportRequest = new PostingsReportRequest
            {
                Filter = new PostingsReportRequestFilter
                {
                    ProcessedAtFrom = DateTime.Today.AddDays(period * -1),
                    ProcessedAtTo = DateTime.Today,
                    DeliverySchema = new string[] { deliverySchema },
                    Sku = Array.Empty<string>(),
                    CancelReasonId = Array.Empty<string>(),
                    OfferId = "",
                    StatusAlias = Array.Empty<string>(),
                    Statuses = Array.Empty<string>(),
                    Title = ""
                },
                Language = "DEFAULT"
            };

            try
            {
                var postingsResponse = await ozonApiClient.MakeRequestAsync(postingsReportRequest, uri);

                return postingsResponse["result"]["code"].ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> GetReportFile(string code, string uri = "https://api-seller.ozon.ru/v1/report/info")
        {
            var reportInfoRequest = new ReportInfoRequest
            {
                Code = code
            };

            try
            {
                var infoResponse = await ozonApiClient.MakeRequestAsync(reportInfoRequest, uri);

                return infoResponse["result"]["file"].ToString();

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<JArray> ReadFileByUrl(string url)
        {
            var fileContent = await csvUrlParser.ReadFileFromUrl(url);
            return fileContent;
        }

        public async Task<string> GetProductCode(string[] articles,
                                                  string[] ozonIds,
                                                  string uri = "https://api-seller.ozon.ru/v1/report/products/create")
        {
            var productsReportRequest = new ProductsReportRequest
            {
                Language = "DEFAULT",
                OfferId = articles,
                Search = "",
                Sku = ozonIds,
                Visibility = "ALL"
            };

            try
            {
                var productsReportResponse = await ozonApiClient.MakeRequestAsync(productsReportRequest, uri);
                return productsReportResponse["result"]["code"].ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<JObject> GetProductPrices(string[] articles,
                                                   string[] ozonProductIds,
                                                   string uri = "https://api-seller.ozon.ru/v4/product/info/prices")
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
                var productPricesResponse = await ozonApiClient.MakeRequestAsync(productPricesRequest, uri);
                return productPricesResponse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<JObject> GetProductWarehouses(string[] sku,
                                                       string[] fbsSku,
                                                       string uri = "https://api-seller.ozon.ru/v1/product/info/stocks-by-warehouse/fbs")
        {
            var stocksByWarehouseRequest = new StocksByWarehouseRequest
            {
                Sku = sku,
                FbsSku = fbsSku
            };

            try
            {
                var stocksByWarehouseResponse = await ozonApiClient.MakeRequestAsync(stocksByWarehouseRequest, uri);

                return stocksByWarehouseResponse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<JObject> GetWarehousesList(string uri = "https://api-seller.ozon.ru/v1/warehouse/list")
        {
            var requestBody = "{}";
            try
            {
                var warehouseListResponse = await ozonApiClient.MakeRequestAsync(requestBody, uri);

                return warehouseListResponse;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
