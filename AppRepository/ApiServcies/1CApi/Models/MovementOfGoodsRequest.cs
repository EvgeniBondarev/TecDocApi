using Newtonsoft.Json;

namespace Servcies.ApiServcies._1CApi.Models;

public class MovementOfGoodsRequest : IRequestModel
{
    [JsonProperty("Date")]
    public string Date { get; set; }
        
    [JsonProperty("INNorganization")]
    public string INNorganization { get; set; }
        
    [JsonProperty("WarehouseSender")]
    public string WarehouseSender { get; set; }
        
    [JsonProperty("SenderRecipient")]
    public string SenderRecipient { get; set; }
        
    [JsonProperty("comment")]
    public string Comment { get; set; }
        
    [JsonProperty("product")]
    public List<MovementProduct> Products { get; set; } = new List<MovementProduct>();
}