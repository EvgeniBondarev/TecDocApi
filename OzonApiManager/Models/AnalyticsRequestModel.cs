namespace OzonApiManager.Models
{
    public class AnalyticsRequestModel : IRequestModel
    {
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string[] Metrics { get; set; }
        public string[] Dimension { get; set; }
        public object[] Filters { get; set; }
        public SortOption[] Sort { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }

        public AnalyticsRequestModel(DateTime fromDate, DateTime toDate)
        {
            DateFrom = fromDate.ToString("yyyy-MM-dd");
            DateTo = toDate.ToString("yyyy-MM-dd");
            Metrics = new[] { "hits_view_search" };
            Dimension = new[] { "sku" };
            Filters = Array.Empty<object>();
            Sort = new[] { new SortOption { Key = "hits_view_search", Order = "DESC" } };
            Limit = 1000;
            Offset = 0;
        }

     
    }
    public class SortOption
    {
        public string Key { get; set; }
        public string Order { get; set; }
    }
}
