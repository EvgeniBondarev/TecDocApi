using Newtonsoft.Json.Linq;
using OzonOrdersWeb.ViewModels.OzonClientViewModels;
using Servcies.ApiServcies.YandexApi.Filters;


namespace Servcies.ApiServcies.YandexApi
{
    public class YandexDataManager : IApiDataManager<YandexDataManager>
    {
        private YandexApiClient _apiClient;
        private string _campaignId;

        public YandexDataManager()
        {
        }

        public YandexDataManager SetClient(string apiKey)
        {
            _apiClient = new YandexApiClient(apiKey);
            return this;
        }

        public YandexDataManager SetClient(string clientId, string apiKey)
        {
            _apiClient = new YandexApiClient(apiKey);
            _campaignId = clientId;
            return this;
        }

        public async Task<string> GetApiKey()
        {
            return await _apiClient.GetApiKey();
        }

        public async Task<bool> GetTestRequest(string clientId, string apiKey)
        {
            return await _apiClient.GetTestRequest(clientId, apiKey);
        }

        public async Task<List<YandexClient>> GetAccountCampaigns()
        {
            JObject campaignsResponse = _apiClient.MakeRequestPost(new CampaignsRequest(), YandexApiUrl.CAMPAIGNS);

            var campaignsList = campaignsResponse["campaigns"];
            var yandexClients = new List<YandexClient>();

            foreach (var campaign in campaignsList)
            {
                var yandexClient = new YandexClient
                {
                    WarehouseName = campaign["domain"].ToString(),
                    CompanyId = (int)campaign["id"],
                    BusinessId = (int)campaign["business"]["id"],
                    BusinessName = campaign["business"]["name"].ToString()
                };
                yandexClients.Add(yandexClient);
            }

            return yandexClients.OrderBy(ya => ya.BusinessName).ToList();
        }

        public async Task<YandexClient> GetCampaignInfo()
        {
            JObject campaignInfoResponse = _apiClient.MakeRequestPost(
                new CampaignRequest(),
                string.Format(YandexApiUrl.CAMPAIGN, _campaignId)
            );

            var yandexClient = new YandexClient
            {
                WarehouseName = campaignInfoResponse["campaign"]["domain"]?.ToString(),
                CompanyId = (int)campaignInfoResponse["campaign"]["id"],
                BusinessId = (int)campaignInfoResponse["campaign"]["business"]["id"],
                BusinessName = campaignInfoResponse["campaign"]["business"]["name"]?.ToString()
            };

            return yandexClient;
        }


        public async Task<JArray> GetOrders(DateTime start, DateTime end)
        {

            JObject response = _apiClient.MakeRequestPost(new OrdersRequest()
            {
                FromDate = start.ToString("dd-MM-yyyy"),
                ToDate = end.ToString("dd-MM-yyyy")
            },
            string.Format(YandexApiUrl.ORDERS, _campaignId));

            YandexClient yandexApiClient = await GetCampaignInfo();

            JArray orders = response["orders"]?.ToObject<JArray>();
            JArray resultOrders = new JArray();

            if (orders != null)
            {
                foreach (JObject order in orders)
                {
                    order["warehouseName"] = yandexApiClient.WarehouseName;
                    JArray items = order["items"]?.ToObject<JArray>();

                    if (items != null && items.Count > 1)
                    {
                        foreach (JObject item in items)
                        {
                            JObject clonedOrder = (JObject)order.DeepClone();
                            clonedOrder["items"] = new JArray { item };
                            resultOrders.Add(clonedOrder);
                        }
                    }
                    else
                    {
                        resultOrders.Add(order);
                    }
                }
            }

            return resultOrders;
        }

    }
}
