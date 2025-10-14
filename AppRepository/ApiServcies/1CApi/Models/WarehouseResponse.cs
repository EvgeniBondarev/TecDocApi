using Newtonsoft.Json;

namespace Servcies.ApiServcies._1CApi.Models;

public class WarehouseResponse
{
    [JsonProperty("name")] public string Name { get; set; }
}