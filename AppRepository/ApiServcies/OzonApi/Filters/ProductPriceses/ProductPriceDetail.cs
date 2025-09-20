using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters.ProductPriceses;

public class ProductPriceDetail
{
    [JsonProperty("auto_action_enabled")]
    public bool AutoActionEnabled { get; set; }

    [JsonProperty("currency_code")]
    public string CurrencyCode { get; set; }

    [JsonProperty("marketing_price")]
    public double MarketingPrice { get; set; }

    [JsonProperty("marketing_seller_price")]
    public double MarketingSellerPrice { get; set; }

    [JsonProperty("min_price")]
    public double MinPrice { get; set; }

    [JsonProperty("old_price")]
    public double OldPrice { get; set; }

    [JsonProperty("price")]
    public double Price { get; set; }

    [JsonProperty("retail_price")]
    public double RetailPrice { get; set; }

    [JsonProperty("vat")]
    public double Vat { get; set; }

    [JsonProperty("auto_add_to_ozon_actions_list_enabled")]
    public bool AutoAddToOzonActionsListEnabled { get; set; }

    [JsonProperty("net_price")]
    public double NetPrice { get; set; }
}