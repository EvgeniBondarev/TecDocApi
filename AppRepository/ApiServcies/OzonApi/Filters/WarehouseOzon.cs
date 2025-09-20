using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class WarehouseOzon
{
    [JsonProperty("has_entrusted_acceptance")]
    public bool HasEntrustedAcceptance { get; set; }

    [JsonProperty("is_rfbs")]
    public bool IsRfbs { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("warehouse_id")]
    public long WarehouseId { get; set; }

    [JsonProperty("can_print_act_in_advance")]
    public bool CanPrintActInAdvance { get; set; }

    [JsonProperty("first_mile_type")]
    public FirstMileType FirstMileType { get; set; }

    [JsonProperty("has_postings_limit")]
    public bool HasPostingsLimit { get; set; }

    [JsonProperty("is_karantin")]
    public bool IsKarantin { get; set; }

    [JsonProperty("is_kgt")]
    public bool IsKgt { get; set; }

    [JsonProperty("is_economy")]
    public bool IsEconomy { get; set; }

    [JsonProperty("is_timetable_editable")]
    public bool IsTimetableEditable { get; set; }

    [JsonProperty("min_postings_limit")]
    public int MinPostingsLimit { get; set; }

    [JsonProperty("postings_limit")]
    public int PostingsLimit { get; set; }

    [JsonProperty("min_working_days")]
    public int MinWorkingDays { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("working_days")]
    public List<string> WorkingDays { get; set; }
}

