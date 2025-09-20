using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.TradesoftApi.Models;

namespace Servcies.ApiServcies.TradesoftApi;

public class TradesoftApiClient : IApiClient
{
    public JObject MakeRequestPost<T>(T requestModel, string requestUri) where T : IRequestModel
    {
        string jsonResponse;
        try
        {
            var requestBody = JsonConvert.SerializeObject(requestModel);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "POST";
            request.ContentType = "application/json";

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
                throw new Exception($"Request failed. Status code: {httpResponse.StatusCode}. Response: {errorResponse}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while processing the request.", ex);
        }
    }
    
    public async Task<string> MakeRequestPostAsync<T>(T requestModel, string requestUri)
        where T : IRequestModel
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            var jsonContent = JsonConvert.SerializeObject(requestModel);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var httpResponse = await httpClient.PostAsync(requestUri, httpContent);
            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed. Status code: {httpResponse.StatusCode}. Response: {jsonResponse}");
            }

            return jsonResponse;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("An HTTP error occurred while processing the request.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while processing the request.", ex);
        }
    }
    
    public async Task<bool> GetTestRequest(string businessId, string apiKey)
    {
        try
        {
            return true;
        }
        catch
        {
            return false;
        }
    }
}