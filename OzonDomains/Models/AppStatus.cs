using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OzonDomains.Models
{
    public class AppStatus
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Статус в отчете")]
        public string? Name { get; set; }
        
        public int? StatusColorId { get; set; }
        public virtual StatusColor? StatusColor { get; set; }
        public string GetStatusColor()
        {
            return StatusColor?.ColorCode ?? "#ffffff";
        }
    }
}
