using Newtonsoft.Json;

namespace Servcies.ApiServcies.YandexApi.Filters
{
    public class YandexFilter : IYandexModel
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
    }
}
