using System.Text;

namespace OzonDomains.Models.Extensions;

public static class OrderExtensions
{
    public static string Print(this Order order)
    {
        if (order == null) return "Order is null";

        var sb = new StringBuilder();
        var type = order.GetType();
        var properties = type.GetProperties();

        foreach (var prop in properties)
        {
            if (prop.GetIndexParameters().Length > 0)
                continue; // Пропускаем индексаторы

            var value = prop.GetValue(order);

            if (value == null)
            {
                sb.AppendLine($"{prop.Name}: null");
            }
            else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                var nestedProps = prop.PropertyType.GetProperties();
                foreach (var nestedProp in nestedProps)
                {
                    if (nestedProp.GetIndexParameters().Length > 0)
                        continue;

                    object nestedValue = null;
                    try
                    {
                        nestedValue = nestedProp.GetValue(value);
                    }
                    catch
                    {
                        nestedValue = "Error retrieving";
                    }

                    sb.AppendLine($"{prop.Name}.{nestedProp.Name}: {nestedValue ?? "null"}");
                }
            }
            else
            {
                sb.AppendLine($"{prop.Name}: {value}");
            }
        }

        return sb.ToString();
    }
}
