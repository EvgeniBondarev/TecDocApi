using Newtonsoft.Json;

namespace Servcies.ApiServcies._1CApi.Models;

public class ReceiptOfGoodsRequest : IRequestModel
{
    [JsonProperty("Date")]
    public string Date { get; set; }

    [JsonProperty("INNorganization")]
    public string INNorganization { get; set; }

    [JsonProperty("subdivisions")]
    public string Subdivisions { get; set; }

    [JsonProperty("SenderRecipient")]
    public string SenderRecipient { get; set; }

    [JsonProperty("Innbuyer")]
    public string Innbuyer { get; set; }
    [JsonProperty("comment")]
    public string Comment { get; set; }

    [JsonProperty("product")]
    public List<ReceiptProduct> Products { get; set; } = new List<ReceiptProduct>();
}
