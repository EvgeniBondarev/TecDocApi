using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class UpdateStocksRequest : IRequestModel
{
    [JsonProperty("stocks")]
    public List<StockItemForUpdate> Stocks { get; set; }
}