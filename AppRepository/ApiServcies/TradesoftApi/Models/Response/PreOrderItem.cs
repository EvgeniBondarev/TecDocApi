using System.Globalization;
using Newtonsoft.Json;

namespace Servcies.ApiServcies.TradesoftApi.Models.Response;

public class PreOrderItem
{
    [JsonProperty("caption")]
    public string Caption { get; set; }

    [JsonProperty("rest")]
    public string Rest { get; set; }

    [JsonProperty("deliverydays")]
    public string DeliveryDays { get; set; }

    [JsonProperty("deliverydays_min")]
    public int DeliveryDaysMin { get; set; }

    [JsonProperty("deliverydays_max")]
    public int DeliveryDaysMax { get; set; }

    [JsonProperty("currencycode")]
    public string CurrencyCode { get; set; }

    [JsonProperty("producer")]
    public string Producer { get; set; }

    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("price")]
    public string Price { get; set; }

    [JsonProperty("direction")]
    public string Direction { get; set; }

    [JsonProperty("itemHash")]
    public string ItemHash { get; set; }

    [JsonProperty("itemId")]
    public string ItemId { get; set; }
    
    [JsonProperty("minquantity")]
    public int MinQuantity { get; set; }
    public decimal PriceDecimal => decimal.Parse(Price.Replace(",", "."), CultureInfo.InvariantCulture);
    
    public string SiteUrl { get; set; }
    public string Title { get; set; }
    public string? CostPriceFormatted { get; set; }
    public string? CostPrice { get; set; }
    public string? Description { get; set; }
    public string? PriceDescription { get; set; } = null;
}