using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters
{
    public class ReportInfoRequest : IRequestModel
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
