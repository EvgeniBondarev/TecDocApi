using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class PostingsReportRequest : IRequestModel
    {
        [JsonProperty("filter")]
        public PostingsReportRequestFilter Filter { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }
}
