namespace Servcies.ApiServcies.InterpartsApi.Filters
{
    public class SearchBrandRequest : IInterpartsRequestModel
    {
        public string RequestUrl { get; set; }
        public string Code { get; set; }
    }
}
