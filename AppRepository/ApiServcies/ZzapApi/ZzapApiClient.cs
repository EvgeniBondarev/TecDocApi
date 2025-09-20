using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Servcies.ApiServcies.ZzapApi;

public class ZzapApiClient
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<JObject> MakeRequestAsync<T>(T requestModel, string requestUri) where T : IRequestModel
    {
        try
        {
            var properties = typeof(T).GetProperties();
            var queryParams = new List<KeyValuePair<string, string>>();
            
            foreach (var property in properties)
            {
                var jsonPropertyAttribute = (JsonPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(JsonPropertyAttribute));
                var propertyName = jsonPropertyAttribute?.PropertyName ?? property.Name;
                
                var value = property.GetValue(requestModel)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    queryParams.Add(new KeyValuePair<string, string>(propertyName, value));
                }
            }
            
            var builder = new UriBuilder(requestUri);
            var query = new FormUrlEncodedContent(queryParams).ReadAsStringAsync().Result;
            builder.Query = query;

            var response = await _httpClient.GetAsync(builder.Uri);
            response.EnsureSuccessStatusCode();

            var xmlString = await response.Content.ReadAsStringAsync();
            
            // Парсим XML-ответ
            var xDoc = XDocument.Parse(xmlString);
            
            var jsonResponse = xDoc.Root.Value;

            // Парсим извлеченную строку как JSON
            return JObject.Parse(jsonResponse);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Request failed.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while processing the request.", ex);
        }
    }
    
    public Task<bool> GetTestRequest(string login, string password)
    {
        // Эта логика может быть расширена для реального тестового запроса
        return Task.FromResult(true);
    }
}