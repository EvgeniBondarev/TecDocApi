using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models;

public class MakeOrderOfflineParam
{
    [JsonProperty("provider")]
    public string Provider { get; set; }

    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("comment")]
    public string Comment { get; set; }

    [JsonProperty("clientOrderNumber")]
    public string ClientOrderNumber { get; set; }

    [JsonProperty("items")]
    public List<MakeOrderOfflineItem> Items { get; set; }

    [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
    public object Options { get; set; } // Если нужны кастомные параметры
}
