using Newtonsoft.Json;

public class Role
{
    [JsonProperty("name")]
    public string Name { get; set; }
            
    [JsonProperty("methods")]
    public List<string> Methods { get; set; }
}