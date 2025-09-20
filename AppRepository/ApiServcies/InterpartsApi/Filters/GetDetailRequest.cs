namespace Servcies.ApiServcies.InterpartsApi.Filters
{
    public class GetDetailRequest : IInterpartsRequestModel
    {
        public string RequestUrl { get; set; }
        public string Article { get; set; }
    }
}
