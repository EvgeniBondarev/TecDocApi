using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class GetItemsStatusResponse
{
    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("container")]
    public List<GetItemsStatusProviderContainer> Container { get; set; }

    [JsonProperty("time")]
    public double Time { get; set; }
}
