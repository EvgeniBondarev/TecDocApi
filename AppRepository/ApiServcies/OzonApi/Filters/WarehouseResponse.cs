using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class WarehouseResponse
{
    [JsonProperty("result")]
    public List<WarehouseOzon> Warehouses { get; set; }
}
