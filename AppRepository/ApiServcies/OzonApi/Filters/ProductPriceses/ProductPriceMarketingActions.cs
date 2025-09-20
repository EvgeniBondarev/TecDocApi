using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters.ProductPriceses;

public class ProductPriceMarketingActions
{
    [JsonProperty("current_period_from")]
    public DateTime? CurrentPeriodFrom { get; set; }

    [JsonProperty("current_period_to")]
    public DateTime? CurrentPeriodTo { get; set; }

    [JsonProperty("actions")]
    public List<ProductPriceMarketingAction> Actions { get; set; }

    [JsonProperty("ozon_actions_exist")]
    public bool OzonActionsExist { get; set; }
}