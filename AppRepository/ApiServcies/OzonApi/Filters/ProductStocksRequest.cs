using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class ProductStocksRequest : IRequestModel
{
    [JsonProperty("cursor")]
    public string Cursor { get; set; } = "";

    [JsonProperty("filter")]
    public ProductStocksRequestFilter Filter { get; set; }

    [JsonProperty("limit")]
    public int Limit { get; set; } = 100;
}