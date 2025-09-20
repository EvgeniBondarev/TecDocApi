using Newtonsoft.Json.Linq;

namespace OzonApiManage
{

    public class JsonDataBuilder
    {
        private ApiDataManager _apiDataManager;

        public void Init(string clientId, string apiKey)
        {
            _apiDataManager = new ApiDataManager(clientId, apiKey);
        }

        public async Task<JArray> BuilData(int reportPeriod = 4)
        {
            var orderCode = await _apiDataManager.GetReportCode(reportPeriod);

            var orderFile = await _apiDataManager.GetReportFile(orderCode);

            var oredersContent = await _apiDataManager.ReadFileByUrl(orderFile);

            List<string> articles = new List<string>();
            List<string> ozonIds = new List<string>();

            foreach (var oreder in oredersContent)
            {
                articles.Add(oreder["Артикул"].ToString());
                ozonIds.Add(oreder["OZON id"].ToString());
            }

            var productsCode = await _apiDataManager.GetProductCode(articles.ToArray(), ozonIds.ToArray());

            var productsFile = await _apiDataManager.GetReportFile(productsCode);

            var productsContent = await _apiDataManager.ReadFileByUrl(productsFile);

            List<string> newArticles = new List<string>();
            List<string> ozonProductIds = new List<string>();
            List<string> fbsOzonSkuIds = new List<string>();

            foreach (var order in productsContent)
            {
                newArticles.Add(order["Артикул"].ToString());
                ozonProductIds.Add(order["Ozon Product ID"].ToString());
                fbsOzonSkuIds.Add(order["FBS OZON SKU ID"].ToString());
            }

            var productPrices = await _apiDataManager.GetProductPrices(newArticles.ToArray(), ozonProductIds.ToArray());

            JObject allProductWarehouses = new JObject();

            const int batchSize = 500;
            int batches = (int)Math.Ceiling((double)fbsOzonSkuIds.Count / batchSize);

            for (int i = 0; i < batches; i++)
            {
                var currentBatch = fbsOzonSkuIds.Skip(i * batchSize).Take(batchSize).ToArray();

                var productWarehouses = await _apiDataManager.GetProductWarehouses(currentBatch, currentBatch);

                allProductWarehouses.Merge(productWarehouses, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }

            var collectDataResult = await CollectData(oredersContent, productsContent, productPrices, allProductWarehouses);

            return collectDataResult;
        }

        private async Task<JArray> CollectData(JArray oredersContent,
                                               JArray productsContent,
                                               JObject productPrices,
                                               JObject allProductWarehouses)
        {
            foreach (var orders in oredersContent)
            {
                var productWithArticle = productsContent
                                        .Where(order => ((string)order.Value<JToken>("Артикул")).TrimStart('\'') == orders["Артикул"].ToString())
                                        .FirstOrDefault();

                orders["productWithArticle"] = productWithArticle;

                var productPricesWithArticle = productPrices["result"]["items"]
                                            .Select(item => item as JObject)
                                            .FirstOrDefault(item => item?["offer_id"].ToString() == orders["Артикул"].ToString());

                orders["productPricesWithArticle"] = productPricesWithArticle;

                var productWarehousesWithArticle = allProductWarehouses["result"]
                                            .Select(item => item as JObject)
                                            .LastOrDefault(item => item?["sku"].ToString() == orders["productWithArticle"]["FBS OZON SKU ID"].ToString());

                orders["productWarehousesWithArticle"] = productWarehousesWithArticle;

            }

            return oredersContent;
        }
    }
}
