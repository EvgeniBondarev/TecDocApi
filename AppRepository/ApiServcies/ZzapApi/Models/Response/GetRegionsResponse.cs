namespace Servcies.ApiServcies.ZzapApi.Models.Response;

public class GetRegionsResponse
{
    public string error { get; set; }
    public int row_count { get; set; }
    public List<GetRegionItem> table { get; set; }
}