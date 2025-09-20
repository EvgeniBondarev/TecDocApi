using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class ProductImportPriceError
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}