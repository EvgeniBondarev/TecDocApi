using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.Models
{
    public class OzonClient
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [MaxLength(100)]
        [Display(Name = "Client Id")]
        public string? ClientId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Api Key")]
        public string? ApiKey { get; set; }

        [NotMapped]
        public string? DecryptClientId { get; set; }

        [NotMapped]
        public string? DecryptApiKey { get; set; }

        [Display(Name = "Валюта")]
        public CurrencyCode CurrencyCode { get; set; }

        [Display(Name = "Тип клиента")]
        public ClientType ClientType { get; set; }  
    }
}
