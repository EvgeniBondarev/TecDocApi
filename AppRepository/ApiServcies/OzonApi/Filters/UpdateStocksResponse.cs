using Newtonsoft.Json;
using Servcies.ApiServcies.OzonApi.Filters;

public class UpdateStocksResponse
{
    [JsonProperty("result")]
    public List<UpdateStockResult> Result { get; set; }
}