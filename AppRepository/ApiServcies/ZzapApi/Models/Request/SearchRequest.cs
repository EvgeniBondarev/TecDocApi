using Newtonsoft.Json;

namespace Servcies.ApiServcies.ZzapApi.Models.Request;

public class SearchRequest : IRequestModel
{
    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("search_text")]
    public string SearchText { get; set; }

    [JsonProperty("partnumber")]
    public string PartNumber { get; set; }

    [JsonProperty("class_man")]
    public string ManufacturerClass { get; set; }

    [JsonProperty("code_region")]
    public int RegionCode { get; set; }

    [JsonProperty("row_count")]
    public int RowCount { get; set; } = 100;

    [JsonProperty("type_request")]
    public int RequestType { get; set; } = 4;

    [JsonProperty("api_key")]
    public string ApiKey { get; set; }
}