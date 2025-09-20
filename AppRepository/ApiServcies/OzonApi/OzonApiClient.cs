using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Servcies.ApiServcies.OzonApi
{
    public class OzonApiClient : IApiClient
    {
        private readonly string _clientId;
        private readonly string _apiKey;

        public OzonApiClient(string clientId, string apiKey)
        {
            _clientId = clientId;
            _apiKey = apiKey;
        }

        public JObject MakeRequestPost<T>(T requestModel, string requestUri) where T : IRequestModel
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(requestModel);
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Client-Id", _clientId);
                request.Headers.Add("Api-Key", _apiKey);

                Thread.Sleep(2000);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                    streamWriter.Flush();
                }

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
        
        public JObject MakeRequestPostForBitrix<T>(T requestModel, string requestUri) where T : IRequestModel
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(requestModel);
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Client-Id", _clientId);
                request.Headers.Add("Api-Key", _apiKey);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                    streamWriter.Flush();
                }

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

        public JObject MakeFastRequest<T>(T requestModel, string requestUri) where T : IRequestModel
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(requestModel);
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Client-Id", _clientId);
                request.Headers.Add("Api-Key", _apiKey);

                Thread.Sleep(500);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(requestBody);
                    streamWriter.Flush();
                }

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
        public async Task<MemoryStream> MakeFileRequestAsync<T>(T requestModel, string requestUri) where T : IRequestModel
        {
            try
            {
                var requestBody = JsonConvert.SerializeObject(requestModel);
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Client-Id", _clientId);
                request.Headers.Add("Api-Key", _apiKey);

                using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
                {
                    await streamWriter.WriteAsync(requestBody);
                    await streamWriter.FlushAsync();
                }

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    var memoryStream = new MemoryStream();

                    using (var responseStream = response.GetResponseStream())
                    {
                        await responseStream.CopyToAsync(memoryStream);
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    return memoryStream;
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

        public async Task<bool> GetTestRequest(string clientId, string apiKey)
        {
            var requestUri = "https://api-seller.ozon.ru/v1/warehouse/list";

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Client-Id", clientId);
                request.Headers.Add("Api-Key", apiKey);

                var requestBody = "{}";

                using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
                {
                    await streamWriter.WriteAsync(requestBody);
                    await streamWriter.FlushAsync();
                }

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
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
