using Newtonsoft.Json.Linq;

namespace Servcies.ApiServcies
{
    public interface IApiClient
    {
        public JObject MakeRequestPost<T>(T requestModel, string requestUri) where T : IRequestModel;

        public Task<bool> GetTestRequest(string businessId, string apiKey);
    }
}
