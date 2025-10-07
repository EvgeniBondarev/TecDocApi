using OzonDomains.ViewModels;

namespace OzonOrdersWeb.Areas.Studio2.ViewModels.Warehouse;

public class WarehouseMappingViewModel<T, TFilter> : PageViewModel<T, TFilter>
    where T : class
    where TFilter : class, new()
{
    public int TotalCount { get; set; }

    public WarehouseMappingViewModel(IEnumerable<T> items, int pageNumber, int pageSize, TFilter filterModel)
        : base(items, pageNumber, pageSize, filterModel)
    {
        TotalCount = items.Count(); // Получаем общее количество до пагинации
    }
}