using Newtonsoft.Json;
using Servcies.ApiServcies.TradesoftApi.Models.Response;

public class ProviderInfo
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("active")]
    public bool Active { get; set; }
    
    [JsonProperty("orderActive")]
    public bool OrderActive { get; set; }
    
    [JsonProperty("orderFeature")]
    public bool OrderFeature { get; set; }
    
    [JsonProperty("syncFeature")]
    public bool SyncFeature { get; set; }
    
    [JsonProperty("activeTo")]
    public long? ActiveTo { get; set; }
    
    [JsonProperty("title")]
    public string Title { get; set; }
    
    [JsonProperty("description")]
    public string Description { get; set; }
    
    [JsonProperty("contractRequired")]
    public bool ContractRequired { get; set; }
    
    [JsonProperty("agreementText")]
    public string AgreementText { get; set; }
    
    [JsonProperty("iconUrl")]
    public string IconUrl { get; set; }
    
    [JsonProperty("siteUrl")]
    public string SiteUrl { get; set; }
    
    [JsonProperty("geoIds")]
    public List<string> GeoIds { get; set; }
    
    public DateTime? ActiveToDate => ActiveTo.HasValue 
        ? DateTimeOffset.FromUnixTimeSeconds(ActiveTo.Value).DateTime 
        : null;

    public PreOrderSearchResponse ProductPreOrderSearch { get; set; } = null;
}