using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Servcies.ApiServcies;
using Servcies.ApiServcies._1CApi.Models;

public class OneCApiClient : IApiClient
{
    private readonly string _base64Auth;

    public OneCApiClient(string username, string password)
    {
        var authString = $"{username}:{password}";
        _base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
    }

    public OneCApiClient() { }

    public JObject MakeRequestPost<T>(T requestModel, string requestUri) where T : IRequestModel
    {
        string jsonResponse;
        try
        {
            var requestBody = JsonConvert.SerializeObject(requestModel);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "POST";
            request.ContentType = "application/json";
            
            if (!string.IsNullOrEmpty(_base64Auth))
            {
                request.Headers.Add("Authorization", $"Basic {_base64Auth}");
            }

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(requestBody);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    jsonResponse = streamReader.ReadToEnd();
                }
            }
            return JObject.Parse(jsonResponse);
        }
        catch (WebException ex) when (ex.Response is HttpWebResponse httpResponse)
        {
            using (var streamReader = new StreamReader(ex.Response.GetResponseStream()))
            {
                var errorResponse = streamReader.ReadToEnd();
                throw new Exception($"1C API request failed. Status code: {httpResponse.StatusCode}. Response: {errorResponse}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while processing the 1C API request.", ex);
        }
    }
    
    public async Task<string> MakeRequestPostAsync<T>(T requestModel, string requestUri) where T : IRequestModel
    {
        try
        {
            using var httpClient = new HttpClient(); 
            if (!string.IsNullOrEmpty(_base64Auth))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _base64Auth);
            }

            var jsonContent = JsonConvert.SerializeObject(requestModel);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var httpResponse = await httpClient.PostAsync(requestUri, httpContent);
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Ошибка запроса к API 1C. Код статуса: {httpResponse.StatusCode}. Ответ: {jsonResponse}");
            }

            return jsonResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Произошла HTTP ошибка при выполнении запроса к API 1C.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new Exception("Превышено время ожидания ответа от API 1C.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Ошибка сериализации/десериализации JSON при работе с API 1C.", ex);
        }
        catch (UriFormatException ex)
        {
            throw new Exception("Неверный формат URL адреса API 1C.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Произошла непредвиденная ошибка при обработке запроса к API 1C.", ex);
        }
    }
    
    public async Task<bool> GetTestRequest(string username, string password)
    {
        try
        {
            // Тестовый запрос для проверки подключения к 1C
            var testClient = new OneCApiClient(username, password);
            var testRequest = new MovementOfGoodsRequest
            {
                Date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                INNorganization = "0000000000",
                WarehouseSender = "Основной склад",
                SenderRecipient = "Основное подразделение",
                Comment = "test connection",
                Products = new List<MovementProduct>
                {
                    new MovementProduct { Article = "TEST", Quantity = 1 }
                }
            };
            
            var response = await testClient.MakeRequestPostAsync(testRequest, OneCApiUrl.MOVEMENT_OF_GOODS_URL);
            return !string.IsNullOrEmpty(response);
        }
        catch
        {
            return false;
        }
    }
}