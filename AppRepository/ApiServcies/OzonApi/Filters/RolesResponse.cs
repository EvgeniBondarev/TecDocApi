using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class RolesResponse
{
    [JsonProperty("roles")]
    public List<Role> Roles { get; set; }
}