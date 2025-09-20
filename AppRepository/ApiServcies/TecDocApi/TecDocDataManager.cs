using Newtonsoft.Json.Linq;
using Servcies.ApiServcies.TecDocApi.Filters;

namespace Servcies.ApiServcies.TecDocApi
{
    public class TecDocDataManager : IApiDataManager<TecDocDataManager>
    {
        private readonly TecDocApiClient _apiClient;
        public TecDocDataManager(TecDocApiClient client)
        {
            _apiClient = client;
        }

        public async Task<JObject> GetDetailFullInfo(string supplier, string article)
        {
            try
            {
                JObject campaignInfoResponse = _apiClient.MakeRequestPost(
                    new DetailFullInfoRequest(),
                    string.Format(TecDocApiUrl.DETAILFULLINFO, supplier, article)
                );
                return campaignInfoResponse;
            }
            catch (Exception ex)
            {
                return new JObject();
            }
        }

        public async Task<JObject> GetSubstitute(string supplier, string article)
        {
            try
            {
                JObject campaignInfoResponse = _apiClient.MakeRequestPost(
                    new SubstituteRequest(),
                    string.Format(TecDocApiUrl.SUBSTITUTE, supplier, article)
                );
                return campaignInfoResponse;
            }
            catch (Exception ex)
            {
                return new JObject();
            }
        }

        public Task<bool> GetTestRequest(string clientId, string apiKey)
        {
            throw new NotImplementedException();
        }

        public TecDocDataManager SetClient(string clientId, string apiKey)
        {
            throw new NotImplementedException();
        }
    }
}
