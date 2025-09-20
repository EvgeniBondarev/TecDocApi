using Newtonsoft.Json;

namespace OzonApiManage.Filters
{
    public class PostingsReportRequest : IRequestModel
    {
        [JsonProperty("filter")]
        public PostingsReportRequestFilter Filter { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }
}
