using OzonDomains.ViewModels;
using OzonDomains;
using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.AppStatusViewModels
{
    public class AppStatusViewModel<T, K> : PageViewModel<T, K>
    {
        public List<AppStatus> AppStatus { get; set; }

        public AppStatusViewModel(IEnumerable<T> pageDate, int page, int pageSize, K filterModel) : base(pageDate, page, pageSize, filterModel)
        {
        }
    }
}
