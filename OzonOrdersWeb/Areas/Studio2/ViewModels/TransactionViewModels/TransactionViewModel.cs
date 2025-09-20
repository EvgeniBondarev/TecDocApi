using OServcies.FiltersServcies.FilterModels;
using OzonDomains;
using OzonDomains.Models;
using OzonDomains.ViewModels;
using OzonRepositories.Context.Identity;
using Servcies.FiltersServcies.FilterModels;

namespace OzonOrdersWeb.ViewModels.TransactionViewModels
{
    public class TransactionViewModel<T, K> : PageViewModel<T, K>
    {
        public List<string> UserNames { get; set; }

        public Dictionary<string, int> UniqueArticles { get; set; }
        public Dictionary<string, int> UniqueDeliveryCitys { get; set; }
        public Dictionary<string, int> UniqueNumbers { get; set; }

        public CustomIdentityUser User { get; set; }
        public List<(TransactionType, string)> TransactionTypes { get; set; }
        public OrderFilterModel OrderFilterModel { get; set; }

        public TransactionViewModel(IEnumerable<T> pageDate, int page, int pageSize, K filterModel)
            : base(pageDate, page, pageSize, filterModel)
        {
        }
    }
}
