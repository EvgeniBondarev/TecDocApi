using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OzonApiManage.Filters;
using System.Text;

namespace OzonApiManage
{
    public class OzonApiClient
    {
        private readonly string _clientId;
        private readonly string _apiKey;

        public OzonApiClient(string clientId, string apiKey)
        {
            _clientId = clientId;
            _apiKey = apiKey;
        }
        public async Task<JObject> MakeRequestAsync<T>(T requestModel, string requestUri) where T : IRequestModel
        {
            await Task.Delay(5000);
            // Создаем новый экземпляр HttpClient
            using (var httpClient = new HttpClient())
            {
                // Формирование HTTP заголовков
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Client-Id", _clientId);
                httpClient.DefaultRequestHeaders.Add("Api-Key", _apiKey);

                // Формирование запроса
                var requestBody = JsonConvert.SerializeObject(requestModel);
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Отправка запроса
                var response = await httpClient.PostAsync(requestUri, content);

                // Обработка ответа
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(jsonResponse);
                }
                else
                {
                    throw new Exception($"Request failed. Status code: {response.StatusCode}");
                }
            }
        }


        public async Task<JObject> MakeRequestAsync(string requestBody, string requestUri, int timeoutFromSeconds = 5)
        {
            // Создаем новый экземпляр HttpClient
            using (var httpClient = new HttpClient())
            {
                // Формирование HTTP заголовков
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Client-Id", _clientId);
                httpClient.DefaultRequestHeaders.Add("Api-Key", _apiKey);

                // Формирование запроса
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // Установка времени ожидания
                httpClient.Timeout = TimeSpan.FromSeconds(timeoutFromSeconds);

                // Отправка запроса
                var response = await httpClient.PostAsync(requestUri, content);

                // Обработка ответа
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(jsonResponse);
                }
                else
                {
                    throw new Exception($"Request failed. Status code: {response.StatusCode}");
                }
            }
        }

    }
}
