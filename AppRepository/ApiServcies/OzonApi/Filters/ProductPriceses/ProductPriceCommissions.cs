using Newtonsoft.Json;

public class ProductPriceCommissions
{
    [JsonProperty("fbo_deliv_to_customer_amount")]
    public double FboDelivToCustomerAmount { get; set; }

    [JsonProperty("fbo_direct_flow_trans_max_amount")]
    public double FboDirectFlowTransMaxAmount { get; set; }

    [JsonProperty("fbo_direct_flow_trans_min_amount")]
    public double FboDirectFlowTransMinAmount { get; set; }

    [JsonProperty("fbo_return_flow_amount")]
    public double FboReturnFlowAmount { get; set; }

    [JsonProperty("fbs_deliv_to_customer_amount")]
    public double FbsDelivToCustomerAmount { get; set; }

    [JsonProperty("fbs_direct_flow_trans_min_amount")]
    public double FbsDirectFlowTransMinAmount { get; set; }

    [JsonProperty("fbs_direct_flow_trans_max_amount")]
    public double FbsDirectFlowTransMaxAmount { get; set; }

    [JsonProperty("fbs_first_mile_max_amount")]
    public double FbsFirstMileMaxAmount { get; set; }

    [JsonProperty("fbs_first_mile_min_amount")]
    public double FbsFirstMileMinAmount { get; set; }

    [JsonProperty("fbs_return_flow_amount")]
    public double FbsReturnFlowAmount { get; set; }

    [JsonProperty("sales_percent_fbo")]
    public double SalesPercentFbo { get; set; }

    [JsonProperty("sales_percent_fbs")]
    public double SalesPercentFbs { get; set; }

    [JsonProperty("sales_percent_rfbs")]
    public double SalesPercentRfbs { get; set; }

    [JsonProperty("sales_percent_fbp")]
    public double SalesPercentFbp { get; set; }
}
