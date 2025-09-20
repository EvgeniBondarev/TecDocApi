namespace OzonDomains.ViewModels
{
    public class PageViewModel<T, K>
    {
        public string Info { get; set; }
        public int PageNumber { get; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        private IEnumerable<T> _items;
        private K _filterModel;

        public PageViewModel(IEnumerable<T> pageDate, int page, int pageSize, K filterModel)
        {
            pageDate = pageDate.Reverse();
            var count = pageDate.Count();
            _items = pageDate.Skip((page - 1) * pageSize).Take(pageSize);

            PageNumber = page;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            _filterModel = filterModel;
        }

        public IEnumerable<T> Items { get { return _items; } }
        public K FilterModel { get { return _filterModel; } }

    }
}
