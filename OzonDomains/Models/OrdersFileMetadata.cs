using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.Models
{
    public class OrdersFileMetadata
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Дириктория")]
        public string? FolderName { get; set; }

        [StringLength(100)]
        [Display(Name = "Имя файла")]
        public string? FileName { get; set; }
    }
}
