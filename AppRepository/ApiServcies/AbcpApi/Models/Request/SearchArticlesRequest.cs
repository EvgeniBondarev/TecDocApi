using Servcies.ApiServcies.AbcpApi.Models.Request;

public class SearchArticlesRequest : AbcpBaseRequest
{
    public List<SearchItem> Search { get; set; } = new();
}