using OzonDomains.ViewModels;
using OzonDomains;

namespace OzonOrdersWeb.ViewModels.OzonClientViewModels
{
    public class OzonClientViewModel<T, K> : PageViewModel<T, K>
    {
        public List<(CurrencyCode, string)> CurrencyCodes { get; set; }

        public OzonClientViewModel(IEnumerable<T> pageDate, int page, int pageSize, K filterModel) : base(pageDate, page, pageSize, filterModel)
        {
        }
    }
}
