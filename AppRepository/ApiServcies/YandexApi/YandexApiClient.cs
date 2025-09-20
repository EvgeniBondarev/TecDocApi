using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;


namespace Servcies.ApiServcies.YandexApi
{
    public class YandexApiClient : IApiClient
    {
        private readonly string _apiKey;

        public YandexApiClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GetApiKey()
        {
            return _apiKey;
        }

        public JObject MakeRequestPost<T>(T requestModel, string requestUri) where T : IRequestModel
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(requestModel);
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "GET"; 
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");


                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonResponse = streamReader.ReadToEnd();
                        return JObject.Parse(jsonResponse);
                    }
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

        public async Task<bool> GetTestRequest(string campaignId, string apiKey)
        {
            var requestUri = $"https://api.partner.market.yandex.ru/campaigns/{campaignId}/orders" +
                             "?orderIds=&status=&substatus=&fromDate=&toDate=&supplierShipmentDateFrom=&supplierShipmentDateTo=" +
                             "&updatedAtFrom=&updatedAtTo=&dispatchType=&fake=&hasCis=&onlyWaitingForCancellationApprove=" +
                             "&onlyEstimatedDelivery=&buyerType=&page=&pageSize=&page_token=eyBuZXh0SWQ6IDIzNDIgfQ%3D%3D&limit=20";
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse httpResponse)
            {
                // Handle the WebException with a specific response
                return false;
            }
            catch (Exception)
            {
                // Handle any other exceptions
                return false;
            }
        }


    }
}
