using Newtonsoft.Json;

namespace OzonApiManage.Filters
{
    public class ReportInfoRequest : IRequestModel
    {
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
