using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.AbcpApi.Models;
using Servcies.ApiServcies.AbcpApi.Models.Request;
using Servcies.ApiServcies.AbcpApi.Models.Response;

namespace Servcies.ApiServcies.AbcpApi;

public class AbcpApiClient : IApiClient
{
    public JObject MakeRequestPost<T>(T requestModel, string requestUri) where T : IRequestModel
    {
        try
        {
            string postData = "";

            if (requestModel is AbcpBaseRequest abcpRequest)
            {
                var keyValues = new List<string>
                {
                    $"userlogin={Uri.EscapeDataString(abcpRequest.UserLogin)}",
                    $"userpsw={Uri.EscapeDataString(abcpRequest.UserPsw)}"
                };

                if (requestModel is SearchArticlesRequest searchRequest)
                {
                    for (int i = 0; i < searchRequest.Search.Count; i++)
                    {
                        keyValues.Add($"search[{i}][number]={Uri.EscapeDataString(searchRequest.Search[i].Number)}");
                        keyValues.Add($"search[{i}][brand]={Uri.EscapeDataString(searchRequest.Search[i].Brand)}");
                    }
                }

                postData = string.Join("&", keyValues);
            }

            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(postData);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonResponse = streamReader.ReadToEnd();
                if (jsonResponse.TrimStart().StartsWith("["))
                {
                    jsonResponse = $"{{\"data\": {jsonResponse}}}";
                }

                return JObject.Parse(jsonResponse);
            }
        }
        catch (WebException ex) when (ex.Response is HttpWebResponse httpResponse)
        {
            throw new Exception($"Request failed. Status code: {httpResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while processing the request.", ex);
        }
    }
    
    public JObject MakeRequestGet<T>(T requestModel, string requestUri) where T : IRequestModel
    {
        try
        {
            string queryString = "";

            if (requestModel is AbcpBaseRequest abcpRequest)
            {
                var parameters = new List<string>
                {
                    $"userlogin={Uri.EscapeDataString(abcpRequest.UserLogin)}",
                    $"userpsw={Uri.EscapeDataString(abcpRequest.UserPsw)}"
                };

                queryString = "?" + string.Join("&", parameters);
            }

            var fullUrl = requestUri + queryString;

            var request = (HttpWebRequest)WebRequest.Create(fullUrl);
            request.Method = "GET";
            request.ContentType = "application/json";

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonResponse = streamReader.ReadToEnd();
                if (jsonResponse.TrimStart().StartsWith("["))
                {
                    jsonResponse = $"{{\"data\": {jsonResponse}}}";
                }

                return JObject.Parse(jsonResponse);
            }
        }
        catch (WebException ex) when (ex.Response is HttpWebResponse httpResponse)
        {
            throw new Exception($"Request failed. Status code: {httpResponse.StatusCode}");
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while processing the GET request.", ex);
        }
    }

    
    public async Task<List<DistributorShortInfo>> GetDistributorsShortInfo(string userLogin, string userPsw, string domain)
    {
        try
        {
            var url = $"{AbcpApiUrl.BASE_URL.Replace("{0}", domain)}/cp/distributors";
            var request = new AbcpBaseRequest { UserLogin = userLogin, UserPsw = userPsw };
        
            var response = MakeRequestGet(request, url);
            return response["data"]?.ToObject<List<DistributorShortInfo>>() ?? new List<DistributorShortInfo>();
        }
        catch
        {
            return new List<DistributorShortInfo>();
        }
    }


    public async Task<bool> GetTestRequest(string userLogin, string userPsw)
    {
        try
        { 
            var testUrl = AbcpApiUrl.BASE_URL.Replace("{0}", "test") + "search/articles/";
            var request = new AbcpBaseRequest { UserLogin = userLogin, UserPsw = userPsw };
            
            var response = MakeRequestPost(request, testUrl);
            return response != null;
        }
        catch
        {
            return false;
        }
    }
}