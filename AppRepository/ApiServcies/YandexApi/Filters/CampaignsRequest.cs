using Newtonsoft.Json;

namespace Servcies.ApiServcies.YandexApi.Filters
{
    public class CampaignsRequest : IRequestModel
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }
    }

}
