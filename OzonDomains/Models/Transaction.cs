using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Display(Name = "Тип транзакции")]
        public TransactionType? Type {  get; set; }

        [Display(Name = "Список заказав в транзакции")]
        public virtual ICollection<Order> Orders { get; set; }

        
        [Display(Name = "Пользователь провевший транзакцию")]
        [MaxLength(50)]
        public string? CreateBy { get; set; }

        [Display(Name = "Дата создания транзакции")]
        public DateTime? CreatedDateTime { get; set; }
        public string FormattedCreatedDate
        {
            get
            {
                return CreatedDateTime.HasValue ? $"{CreatedDateTime.Value.ToString("dd.MM.yyyy")}" : "";
            }
        }

        public string FormattedCreatedTime
        {
            get
            {
                return CreatedDateTime.HasValue ? $"{CreatedDateTime.Value.ToString("HH:mm:ss")}" : "";
            }
        }

        [NotMapped]
        public string FormattedCreatedTimeDateTime
        {
            get
            {
                return CreatedDateTime.HasValue ? $"{CreatedDateTime.Value.ToString("dd.MM.yyyy HH:mm")}" : "";
            }
        }

        [Display(Name = "Комментарий")]
        [MaxLength(500)]
        public string? Comment { get; set; }
    }
}
