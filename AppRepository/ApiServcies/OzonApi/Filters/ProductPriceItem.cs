

using Newtonsoft.Json;

public class ProductPriceItem
{
    [JsonProperty("auto_action_enabled")]
    public string AutoActionEnabled { get; set; } = "UNKNOWN";

    [JsonProperty("auto_add_to_ozon_actions_list_enabled")]
    public string AutoAddToOzonActionsListEnabled { get; set; } = "UNKNOWN";

    [JsonProperty("currency_code")]
    public string CurrencyCode { get; set; } = "RUB";

    [JsonProperty("min_price")]
    public string MinPrice { get; set; }

    [JsonProperty("min_price_for_auto_actions_enabled")]
    public bool MinPriceForAutoActionsEnabled { get; set; }

    [JsonProperty("net_price")]
    public string NetPrice { get; set; }

    [JsonProperty("offer_id")]
    public string OfferId { get; set; }

    [JsonProperty("old_price")]
    public string OldPrice { get; set; }

    [JsonProperty("price")]
    public string Price { get; set; }

    [JsonProperty("price_strategy_enabled")]
    public string PriceStrategyEnabled { get; set; } = "UNKNOWN";

    [JsonProperty("product_id")]
    public long ProductId { get; set; }

    [JsonProperty("quant_size")]
    public int QuantSize { get; set; } = 1;

    [JsonProperty("vat")]
    public string Vat { get; set; }
}