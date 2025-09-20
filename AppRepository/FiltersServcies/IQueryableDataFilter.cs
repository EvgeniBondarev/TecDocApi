using System.Linq.Expressions;

namespace Servcies.FiltersServcies
{
    public interface IQueryableDataFilter<T>
    {
        IQueryable<T> FilterByInt(IQueryable<T> data, Expression<Func<T, int?>> propertySelector, int? filterValue);
        IQueryable<T> FilterByDate(IQueryable<T> data, Expression<Func<T, DateTime?>> propertySelector, DateTime? filterDate);
        IQueryable<T> FilterByString(IQueryable<T> data, Expression<Func<T, string>> propertySelector, string filterValue, bool exactMatch = false);
        IQueryable<T> FilterByDecimal(IQueryable<T> data, Expression<Func<T, decimal?>> propertySelector, decimal? filterValue, decimal tolerance = 1);
        IQueryable<T> FilterByDouble(IQueryable<T> data, Expression<Func<T, double?>> propertySelector, double? filterValue);
        IQueryable<T> FilterByPeriod(IQueryable<T> data, Expression<Func<T, DateTime>> propertySelector, DateTime? startDate, DateTime? endDate);
        IQueryable<T> FilterByEnum<TEnum>(IQueryable<T> data, Expression<Func<T, TEnum?>> propertySelector, TEnum? filterValue) where TEnum : struct, Enum;
    }
}
