using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Склад")]
        public string? Name { get; set; }
    }
}
