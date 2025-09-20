using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.Models
{
    public class UserAccess
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string? Name { get; set; }

        [Display(Name ="Доступные столбцы в таблице заказов")]
        public List<string>? AvailableOrderColumns { get; set; }
    }
}
