using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models
{
    public class Currency
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Валюта")]
        public string? Name { get; set; }
    }
}
