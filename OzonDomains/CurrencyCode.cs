using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OzonDomains
{
    public enum CurrencyCode
    {
        [Display(Name = "Нет")]
        NON = 0,

        [Display(Name = "USD")]
        USD = 1,

        [Display(Name = "EUR")]
        EUR = 2,

        [Display(Name = "RUB")]
        RUB = 3,

        [Display(Name = "BYN")]
        BYN = 4,
    }
}
