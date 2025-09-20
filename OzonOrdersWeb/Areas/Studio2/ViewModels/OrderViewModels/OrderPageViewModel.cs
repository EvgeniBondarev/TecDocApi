using OzonDomains.ViewModels;
using OzonRepositories.Context.Identity;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class OrderPageViewModel<T, K> : PageViewModel<T, K>
    {
        public int ReturnableCount { get; }

        public Dictionary<string, int> UniqueArticles { get; set; }
        public Dictionary<string, int> UniqueDeliveryCitys { get; set; }
        public Dictionary<string, int> UniqueNumbers { get; set; }


        public CustomIdentityUser User { get; set; }

        public OrderPageViewModel(IEnumerable<T> pageData, int page, int pageSize, K filterModel, int returnableCount)
            : base(pageData, page, pageSize, filterModel)
        {
            ReturnableCount = returnableCount;
        }
    }
}
