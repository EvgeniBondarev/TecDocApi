using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;


namespace Servcies.ApiServcies.TecDocApi
{
    public class TecDocApiClient : IApiClient
    {
        private readonly string _baseUri;

        public TecDocApiClient(string baseUri = "http://109.196.101.10:8000/")
        {
            _baseUri = baseUri;
        }

        public JObject MakeRequestPost<T>(T requestModel, string requestUri) where T : IRequestModel
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(requestModel);
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "GET";
                request.ContentType = "application/json";

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

        public Task<bool> GetTestRequest(string businessId, string apiKey)
        {
            throw new NotImplementedException();
        }
    }
}
