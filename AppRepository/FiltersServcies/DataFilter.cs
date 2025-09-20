namespace Servcies.FiltersServcies
{
    public class DataFilter<T> : IDataFilter<T>
    {
        public IEnumerable<T> FilterByInt(
            IEnumerable<T> data,
            Func<T, int> propertySelector,
            int? filterValue)
        {
            if (filterValue != null)
                return data.Where(item => propertySelector(item) == filterValue);
            else
            {
                return data;
            }
        }

        public IEnumerable<T> FilterByDate(
        IEnumerable<T> data,
        Func<T, DateTime?> propertySelector,
        DateTime? filterDate)
        {
            if (filterDate != null)
            {
                DateTime filterDateDay = filterDate.Value.Date;
                return data.Where(item => propertySelector(item)?.Date == filterDateDay);
            }
            else
            {
                return data;
            }
        }


        public IEnumerable<T> FilterByString(
            IEnumerable<T> data,
            Func<T, string> propertySelector,
            string filterValue,
            bool exactMatch = false)
        {
            if (!string.IsNullOrEmpty(filterValue))
            {
                if (exactMatch)
                {
                    return data.Where(item => propertySelector(item) != null &&
                                              string.Equals(propertySelector(item), filterValue, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    return data.Where(item => propertySelector(item) != null &&
                                              propertySelector(item).IndexOf(filterValue, StringComparison.OrdinalIgnoreCase) >= 0);
                }
            }
            else
            {
                return data;
            }
        }



        public IEnumerable<T> FilterByDecimal(
         IEnumerable<T> data,
         Func<T, decimal?> propertySelector,
         decimal? filterValue,
         decimal tolerance = 1)
        {
            if (filterValue != null)
            {
                return data.Where(item => propertySelector(item) != null && Math.Abs((decimal)propertySelector(item) - filterValue.Value) <= tolerance);
            }
            else
            {
                return data;
            }
        }


        public IEnumerable<T> FilterByDouble(
            IEnumerable<T> data,
            Func<T, double?> propertySelector,
            double? filterValue,
            double tolerance = 1)
        {
            if (filterValue != null)
            {
                return data.Where(item => propertySelector(item) != null && Math.Abs(propertySelector(item).Value - filterValue.Value) <= tolerance);
            }
            else
            {
                return data;
            }
        }



        public IEnumerable<T> FilterByPeriod(
            IEnumerable<T> data,
            Func<T, DateTime> propertySelector,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (startDate != null && endDate != null)
            {
                return data.Where(item => propertySelector(item) >= startDate && propertySelector(item) <= endDate);
            }
            else if (startDate != null)
            {
                return data.Where(item => propertySelector(item) >= startDate);
            }
            else if (endDate != null)
            {
                return data.Where(item => propertySelector(item) <= endDate);
            }
            else
            {
                return data;
            }
        }

        public IEnumerable<T> FilterByEnum<T, TEnum>(
            IEnumerable<T> data,
            Func<T, TEnum?> propertySelector,
            TEnum? filterValue)
            where TEnum : struct, Enum
        {
            if (filterValue != null)
            {
                return data.Where(item =>
                    propertySelector(item) != null &&
                    propertySelector(item).Equals(filterValue));
            }
            else
            {
                return data;
            }
        }

        public IEnumerable<T> FilterByTimeSpan(
            IEnumerable<T> data,
            Func<T, TimeSpan?> propertySelector,
            TimeSpan? filterTimeSpan)
        {
            if (filterTimeSpan != null)
            {
                return data.Where(item =>
                    propertySelector(item) != null &&
                    Math.Abs(propertySelector(item).Value.TotalSeconds - filterTimeSpan.Value.TotalSeconds) <= 1);
            }
            else
            {
                return data;
            }
        }
    }
}

