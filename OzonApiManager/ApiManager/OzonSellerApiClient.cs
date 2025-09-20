using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OzonApiManager.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OzonApiManager.OzonApiManager.ApiManager
{
    public class OzonSellerApiClient
    {
        private readonly HttpClient _client;

        public OzonSellerApiClient(string clientId, string apiKey)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Client-Id", clientId);
            _client.DefaultRequestHeaders.Add("Api-Key", apiKey);
        }

        public async Task<string> GetAnalyticsDataAsync(DateTime fromDate, DateTime toDate)
        {
            var requestUri = "https://api-seller.ozon.ru/v1/analytics/data";
            var requestData = new
            {
                date_from = fromDate.ToString("yyyy-MM-dd"),
                date_to = toDate.ToString("yyyy-MM-dd"),
                metrics = new[] { "hits_view_search" },
                dimension = new[] { "sku", "spu", "day" },
                filters = Array.Empty<object>(),
                sort = new[] { new { key = "hits_view_search", order = "DESC" } },
                limit = 1000,
                offset = 0
            };

            var response = await _client.PostAsync(requestUri, new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Ошибка: {response.StatusCode}");
            }

        }

    }
}
