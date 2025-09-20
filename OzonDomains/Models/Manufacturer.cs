using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Код")]
        public string? Code { get; set; }

        [StringLength(50)]
        [Display(Name = "Имя")]
        public string? Name { get; set; }
    }
}
