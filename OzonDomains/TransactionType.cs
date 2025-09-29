using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains
{
    public enum TransactionType
    {
        [Display(Name = "Заказан поставщику")]
        OrderedToSupplier = 0,

        [Display(Name = "Все")]
        All = 1,

        [Display(Name = "Отгружен поставщиком")]
        ShippedBySupplier = 2,
        
        [Display(Name = "Отгружен реализатором")]
        ShippedToSeller = 3,
        
        [Display(Name = "Заказан реализатору")]
        OrderedToSeller = 4,
        
        [Display(Name = "Отгружен клиенту")]
        ShippedToClient = 6,
        
        [Display(Name = "Проценён")]
        Percentage = 6,
    }
    
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())[0]
                .GetCustomAttribute<DisplayAttribute>()?
                .Name ?? enumValue.ToString();
        }
    }
}
