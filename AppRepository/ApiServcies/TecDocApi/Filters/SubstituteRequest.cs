namespace Servcies.ApiServcies.TecDocApi.Filters
{
    public class SubstituteRequest : IRequestModel
    {
        public string Supplier { get; set; }
        public string Article { get; set; }
    }
}
