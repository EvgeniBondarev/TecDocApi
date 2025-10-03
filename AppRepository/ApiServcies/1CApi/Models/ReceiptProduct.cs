using Newtonsoft.Json;

public class ReceiptProduct
{
    [JsonProperty("Article")]
    public string Article { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("sum")]
    public decimal Sum { get; set; }

    [JsonProperty("sumNDS")]
    public decimal SumNDS { get; set; }
}