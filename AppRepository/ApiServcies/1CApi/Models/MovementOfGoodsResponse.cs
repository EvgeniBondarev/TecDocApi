using Newtonsoft.Json;

namespace Servcies.ApiServcies._1CApi.Models;

public class MovementOfGoodsResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }
        
    [JsonProperty("message")]
    public string Message { get; set; }
        
    [JsonProperty("transactionId")]
    public string TransactionId { get; set; }
}