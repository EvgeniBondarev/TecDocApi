namespace Servcies.FiltersServcies.DataFilterManagers
{
    public interface IFilterManager<T, K>
    {
        List<T> FilterByRadioButton(List<T> filterModel, string buttonState);
        List<T> FilterByFilterData(List<T> filterModel, K filterData);
    }
}
