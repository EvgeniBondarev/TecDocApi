using Newtonsoft.Json;

namespace Servcies.ApiServcies.ZzapApi.Models.Response;

public class SearchResponse
{
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("table")]
    public List<SearchResultItem> Data { get; set; }
}