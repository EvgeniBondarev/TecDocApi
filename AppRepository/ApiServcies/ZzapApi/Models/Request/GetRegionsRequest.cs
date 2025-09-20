using Newtonsoft.Json;
using Servcies.ApiServcies;

public class GetRegionsRequest : IRequestModel
{
    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("api_key")]
    public string ApiKey { get; set; }
}