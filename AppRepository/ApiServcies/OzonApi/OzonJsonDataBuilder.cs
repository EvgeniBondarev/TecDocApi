using Newtonsoft.Json.Linq;

namespace Servcies.ApiServcies.OzonApi
{
    public class OzonJsonDataBuilder
    {
        private OzonApiDataManager _apiDataManager;

        public void Init(string clientId, string apiKey)
        {
            _apiDataManager = new OzonApiDataManager(clientId, apiKey);
        }

        public void SetClient(string clientId, string apiKey)
        {
            _apiDataManager = _apiDataManager.SetClient(clientId, apiKey);
        }

        public async Task<JArray> BuildData(DateTime start, DateTime end)
        {
            const int batchSize = 500;

            await Task.Delay(5000);
            var orderCode = await _apiDataManager.GetReportCode(start, end);
            await Task.Delay(5000);
            var orderFile = await _apiDataManager.GetReportFile(orderCode);
            await Task.Delay(5000);
            var oredersContent = await _apiDataManager.ReadFileByUrl(orderFile);

            List<string> articles = [];
            List<string> ozonIds = [];
            List<string> productNumbers = [];

            foreach (var order in oredersContent)
            {
                articles.Add(order["Артикул"].ToString());
                ozonIds.Add(order["OZON id"].ToString());
                productNumbers.Add(order["Номер отправления"].ToString());
            }

            JArray productsContent = new JArray();
            int productBatches = GetBathesCount(batchSize, articles.Count);

            for (int i = 0; i < productBatches; i++)
            {
                var articlesBatch = articles.Skip(i * batchSize).Take(batchSize).ToArray();
                var ozonIdsBatch = ozonIds.Skip(i * batchSize).Take(batchSize).ToArray();

                var productsCode = await _apiDataManager.GetProductCode(articlesBatch.ToArray(), ozonIdsBatch.ToArray());
                await Task.Delay(5000);
                var productsFile = await _apiDataManager.GetReportFile(productsCode);

                var productContent = await _apiDataManager.ReadFileByUrl(productsFile);
                foreach (JToken product in productContent)
                {
                    productsContent.Add(product);
                }
            }

            List<string> newArticles = [];
            List<string> ozonProductIds = [];
            List<string> fbsOzonSkuIds = [];

            foreach (var order in productsContent)
            {
                newArticles.Add(order["Артикул"].ToString());
                ozonProductIds.Add(order["Ozon Product ID"].ToString());
                fbsOzonSkuIds.Add(order["SKU"].ToString());
            }



            JObject productPrices = new JObject();
            int pricesBatches = GetBathesCount(batchSize, newArticles.Count);

            for (int j = 0; j < pricesBatches; j++)
            {
                var newArticlesBatch = newArticles.Skip(j * batchSize).Take(batchSize).ToArray();
                var ozonProductBatch = ozonProductIds.Skip(j * batchSize).Take(batchSize).ToArray();

                await Task.Delay(5000);
                var productPrice = await _apiDataManager.GetProductPrices(newArticlesBatch.ToArray(), ozonProductBatch.ToArray());

                productPrices.Merge(productPrice, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }

            JObject allProductWarehouses = new();


            int productWarehousesBatches = GetBathesCount(batchSize, fbsOzonSkuIds.Count);

            for (int k = 0; k < productWarehousesBatches; k++)
            {
                var currentBatch = fbsOzonSkuIds.Skip(k * batchSize).Take(batchSize).ToArray();

                await Task.Delay(5000);
                var productWarehouses = await _apiDataManager.GetProductWarehouses(currentBatch, currentBatch);

                allProductWarehouses.Merge(productWarehouses, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }

            JArray allProductWarehousesAndCitys = new JArray();

            for (int l = 0; l < productWarehousesBatches; l++)
            {
                var productNumbersCurrentBatch = productNumbers.Skip(l * batchSize).Take(batchSize).ToArray();

                await Task.Delay(5000);

                foreach (var number in productNumbersCurrentBatch)
                {
                    var productWarehousAndCity = await _apiDataManager.GetProductWarehousAndCity(number);

                    var properties = productWarehousAndCity.Properties();

                    // Добавляем свойства в JArray
                    foreach (var property in properties)
                    {
                        allProductWarehousesAndCitys.Add(property.Value);
                    }
                }
            }

            var collectDataResult = new JArray();
            try
            {
                collectDataResult = await CollectData(oredersContent, productsContent, productPrices, allProductWarehouses, allProductWarehousesAndCitys);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return collectDataResult;
        }

        private int GetBathesCount(int batchSize, int skuCount)
        {
            return (int)Math.Ceiling((double)skuCount / batchSize);
        }

        private async Task<JArray> CollectData(JArray oredersContent,
                                               JArray productsContent,
                                               JObject productPrices,
                                               JObject allProductWarehouses,
                                               JArray allProductWarehousesAndCitys)
        {
            foreach (var orders in oredersContent)
            {
                var productWithArticle = productsContent
                                        .FirstOrDefault(order => ((string)order.Value<JToken>("Артикул")).TrimStart('\'') == orders["Артикул"].ToString());

                orders["productWithArticle"] = productWithArticle;

                var productPricesWithArticle = productPrices["items"]
                                            .FirstOrDefault(item => item?["offer_id"].ToString() == orders["Артикул"].ToString());

                orders["productPricesWithArticle"] = productPricesWithArticle;

                var productWarehousesWithArticle = allProductWarehouses["result"]
                                            .LastOrDefault(item => item?["sku"].ToString() == orders["productWithArticle"]["SKU"].ToString());

                orders["productWarehousesWithArticle"] = productWarehousesWithArticle;


                var productWarehousesAndCitysWithNumber = allProductWarehousesAndCitys
                                                        .FirstOrDefault(order => ((string)order.Value<JToken>("posting_number")).ToString() == orders["Номер отправления"].ToString());

                orders["productWarehousesAndCitysWithNumber"] = productWarehousesAndCitysWithNumber;

            }

            return oredersContent;
        }
        public async Task<bool> GetTestReques(string clientId, string apiKey)
        {
            return await _apiDataManager.GetTestRequest(clientId, apiKey);
        }
        public async Task<byte[]> GetOrderLabel(string shipmentNumber)
        {
            var result = await _apiDataManager.GetOrderLabel(shipmentNumber);
            return result.ToArray();
        }

        public async Task<bool> CheckOrderForExistence(string article, string ozonProductId)
        {
           return await _apiDataManager.CheckOrderForExistence([article], [ozonProductId]);
        }

        public async Task<string> GetProductSatatus(string productNumber)
        {
            return await _apiDataManager.GetProductSatatus(productNumber);
        }
    }
}