using Newtonsoft.Json;

public class GetItemsStatusContainer
{
    [JsonProperty("provider")]
    public string Provider { get; set; }

    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

    [JsonProperty("items")]
    public List<string> Items { get; set; }
}