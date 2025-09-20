using Newtonsoft.Json;

namespace Servcies.ApiServcies.OzonApi.Filters;

public class FirstMileType
{
    [JsonProperty("dropoff_point_id")]
    public string DropoffPointId { get; set; }

    [JsonProperty("dropoff_timeslot_id")]
    public int DropoffTimeslotId { get; set; }

    [JsonProperty("first_mile_is_changing")]
    public bool FirstMileIsChanging { get; set; }

    [JsonProperty("first_mile_type")]
    public string FirstMileTypeName { get; set; }
}