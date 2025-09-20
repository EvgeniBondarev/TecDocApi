namespace Servcies.FiltersServcies
{
    public interface IDataFilter<T>
    {
        IEnumerable<T> FilterByInt(IEnumerable<T> data, Func<T, int> propertySelector, int? filterValue);
        IEnumerable<T> FilterByDate(IEnumerable<T> data, Func<T, DateTime?> propertySelector, DateTime? filterDate);
        IEnumerable<T> FilterByString(IEnumerable<T> data, Func<T, string> propertySelector, string filterValue, bool exactMatch = false);
        IEnumerable<T> FilterByDecimal(IEnumerable<T> data, Func<T, decimal?> propertySelector, decimal? filterValue, decimal tolerance = 1);
        IEnumerable<T> FilterByDouble(IEnumerable<T> data, Func<T, double?> propertySelector, double? filterValue, double tolerance = 1);
        IEnumerable<T> FilterByPeriod(IEnumerable<T> data, Func<T, DateTime> propertySelector, DateTime? startDate, DateTime? endDate);
        IEnumerable<T> FilterByEnum<T, TEnum>(IEnumerable<T> data, Func<T, TEnum?> propertySelector, TEnum? filterValue) where TEnum : struct, Enum;
    }
}
