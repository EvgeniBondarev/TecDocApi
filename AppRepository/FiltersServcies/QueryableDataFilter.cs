using System.Linq.Expressions;

namespace Servcies.FiltersServcies
{
    public class QueryableDataFilter<T> :  IQueryableDataFilter<T>
    {
        public IQueryable<T> FilterByInt(
        IQueryable<T> data,
        Expression<Func<T, int?>> propertySelector,
        int? filterValue)
            {
                if (filterValue != null)
                {
                    var param = Expression.Parameter(typeof(T), "item");
                    var property = Expression.Invoke(propertySelector, param);
                    var notNullExpression = Expression.NotEqual(property, Expression.Constant(null, typeof(int?)));
                    var equalExpression = Expression.Equal(property, Expression.Constant(filterValue, typeof(int?)));
                    var andExpression = Expression.AndAlso(notNullExpression, equalExpression);
                    var lambda = Expression.Lambda<Func<T, bool>>(andExpression, param);

                    return data.Where(lambda);
                }
                else
                {
                    return data;
                }
            }


        public IQueryable<T> FilterByDate(
            IQueryable<T> data,
            Expression<Func<T, DateTime?>> propertySelector,
            DateTime? filterDate)
        {
            if (filterDate != null)
            {
                DateTime filterDateDay = filterDate.Value.Date;
                var param = Expression.Parameter(typeof(T), "item");
                var property = Expression.Invoke(propertySelector, param);
                var propertyDate = Expression.Property(property, "Value");
                var propertyDateDate = Expression.Property(propertyDate, "Date");
                var filterDateConstant = Expression.Constant(filterDateDay, typeof(DateTime));
                var equalExpression = Expression.Equal(propertyDateDate, filterDateConstant);
                var notNullExpression = Expression.NotEqual(property, Expression.Constant(null, typeof(DateTime?)));
                var andExpression = Expression.AndAlso(notNullExpression, equalExpression);
                var lambda = Expression.Lambda<Func<T, bool>>(andExpression, param);

                return data.Where(lambda);
            }
            else
            {
                return data;
            }
        }


        public IQueryable<T> FilterByString(
         IQueryable<T> data,
         Expression<Func<T, string>> propertySelector,
         string filterValue,
         bool exactMatch = false)
        {
            if (!string.IsNullOrEmpty(filterValue))
            {
                var param = Expression.Parameter(typeof(T), "item");
                var property = Expression.Invoke(propertySelector, param);
                var notNullExpression = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
                Expression conditionExpression;

                if (exactMatch)
                {
                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                    var toLowerProperty = Expression.Call(property, toLowerMethod);
                    var toLowerFilterValue = Expression.Constant(filterValue.ToLower());
                    conditionExpression = Expression.Equal(toLowerProperty, toLowerFilterValue);
                }
                else
                {
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                    var toLowerProperty = Expression.Call(property, toLowerMethod);
                    var toLowerFilterValue = Expression.Constant(filterValue.ToLower());
                    conditionExpression = Expression.Call(toLowerProperty, containsMethod, toLowerFilterValue);
                }

                var andExpression = Expression.AndAlso(notNullExpression, conditionExpression);
                var lambda = Expression.Lambda<Func<T, bool>>(andExpression, param);

                return data.Where(lambda);
            }
            else
            {
                return data;
            }
        }


        public IQueryable<T> FilterByDecimal(
            IQueryable<T> data,
            Expression<Func<T, decimal?>> propertySelector,
            decimal? filterValue,
            decimal tolerance = 1)
        {
            if (filterValue != null)
            {
                var param = Expression.Parameter(typeof(T), "item");
                var property = Expression.Invoke(propertySelector, param);
                var notNullExpression = Expression.NotEqual(property, Expression.Constant(null, typeof(decimal?)));

                var lowerBound = Expression.Constant(filterValue.Value - tolerance, typeof(decimal?));
                var upperBound = Expression.Constant(filterValue.Value + tolerance, typeof(decimal?));

                var greaterThanOrEqualExpression = Expression.GreaterThanOrEqual(property, lowerBound);
                var lessThanOrEqualExpression = Expression.LessThanOrEqual(property, upperBound);

                var betweenExpression = Expression.AndAlso(greaterThanOrEqualExpression, lessThanOrEqualExpression);
                var andExpression = Expression.AndAlso(notNullExpression, betweenExpression);

                var lambda = Expression.Lambda<Func<T, bool>>(andExpression, param);

                return data.Where(lambda);
            }
            else
            {
                return data;
            }
        }


        public IQueryable<T> FilterByDouble(
            IQueryable<T> data,
            Expression<Func<T, double?>> propertySelector,
            double? filterValue)
        {
            if (filterValue != null)
            {
                var param = Expression.Parameter(typeof(T), "item");
                var property = Expression.Invoke(propertySelector, param);
                var notNullExpression = Expression.NotEqual(property, Expression.Constant(null, typeof(double?)));
                var equalExpression = Expression.Equal(property, Expression.Constant(filterValue, typeof(double?)));
                var andExpression = Expression.AndAlso(notNullExpression, equalExpression);
                var lambda = Expression.Lambda<Func<T, bool>>(andExpression, param);

                return data.Where(lambda);
            }
            else
            {
                return data;
            }
        }

        public IQueryable<T> FilterByPeriod(
            IQueryable<T> data,
            Expression<Func<T, DateTime>> propertySelector,
            DateTime? startDate,
            DateTime? endDate)
        {
            var param = Expression.Parameter(typeof(T), "item");
            var property = Expression.Invoke(propertySelector, param);

            Expression combinedExpression = null;

            if (startDate != null && endDate != null)
            {
                var startDateConstant = Expression.Constant(startDate.Value, typeof(DateTime));
                var endDateConstant = Expression.Constant(endDate.Value, typeof(DateTime));

                var greaterThanOrEqualExpression = Expression.GreaterThanOrEqual(property, startDateConstant);
                var lessThanOrEqualExpression = Expression.LessThanOrEqual(property, endDateConstant);

                combinedExpression = Expression.AndAlso(greaterThanOrEqualExpression, lessThanOrEqualExpression);
            }
            else if (startDate != null)
            {
                var startDateConstant = Expression.Constant(startDate.Value, typeof(DateTime));
                combinedExpression = Expression.GreaterThanOrEqual(property, startDateConstant);
            }
            else if (endDate != null)
            {
                var endDateConstant = Expression.Constant(endDate.Value, typeof(DateTime));
                combinedExpression = Expression.LessThanOrEqual(property, endDateConstant);
            }

            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, param);
                return data.Where(lambda);
            }

            return data;
        }


        public IQueryable<T> FilterByEnum<TEnum>(
            IQueryable<T> data,
            Expression<Func<T, TEnum?>> propertySelector,
            TEnum? filterValue)
            where TEnum : struct, Enum
        {
            if (filterValue != null)
            {
                var param = Expression.Parameter(typeof(T), "item");
                var property = Expression.Invoke(propertySelector, param);
                var notNullExpression = Expression.NotEqual(property, Expression.Constant(null, typeof(TEnum?)));
                var equalExpression = Expression.Equal(property, Expression.Constant(filterValue, typeof(TEnum?)));
                var andExpression = Expression.AndAlso(notNullExpression, equalExpression);
                var lambda = Expression.Lambda<Func<T, bool>>(andExpression, param);

                return data.Where(lambda);
            }
            else
            {
                return data;
            }
        }
    }
}
