using Servcies.ApiServcies.InterpartsApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using OzonDomains.Models.OrderCarts;

namespace OzonDomains.Models
{
    public class Order
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Display(Name = "Ключ")]
        public string? Key { get; set; }

        [MaxLength(50)]
        [Display(Name = "Номер отправления")]
        public string? ShipmentNumber { get; set; }
        
        [NotMapped]
        public string? ShipmentNumberTrimmed
        {
            get
            {
                if (string.IsNullOrEmpty(ShipmentNumber))
                    return ShipmentNumber;

                var trimmed = ShipmentNumber.Trim();
                if (trimmed.Length >= 2 && trimmed[trimmed.Length - 2] == '-' && char.IsDigit(trimmed[trimmed.Length - 1]))
                {
                    return trimmed.Substring(0, trimmed.Length - 2);
                }
                return trimmed;
            }
            set
            {
                
            }
        }

        [Display(Name = "Принят в обработку")]
        public DateTime? ProcessingDate { get; set; }

        [Display(Name = "Дата отгрузки")]
        public DateTime? ShippingDate { get; set; }

        [Display(Name = "Срок доставки")]
        [NotMapped]
        public TimeSpan? DeliveryPeriod
        {
            get
            {
                if (ProcessingDate.HasValue && ShippingDate.HasValue)
                {
                    return ShippingDate.Value - ProcessingDate.Value;
                }
                return null;
            }
            set
            {
                
            }
        }

        [Display(Name = "Осталось времени")]
        [NotMapped]
        public TimeSpan? TimeLeft
        {
            get
            {
                if (ShippingDate.HasValue)
                {
                    return ShippingDate.Value - DateTime.Now;
                }
                return null;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public int TimeLeftDay
        {
            get
            {
                if (TimeLeft.HasValue)
                {
                    return TimeLeft.Value.Days;
                }
                return 0;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedTimeLeftDay
        {
            get
            {
                if (TimeLeft.HasValue)
                {
                    return $"{TimeLeft.Value.Days} д.";
                }
                return null;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public int? DeliveryPeriodDay
        {
            get
            {
                if (DeliveryPeriod.HasValue)
                {
                    return DeliveryPeriod.Value.Days;
                }
                return null;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public int? DeliveryPeriodHour
        {
            get
            {
                if (DeliveryPeriod.HasValue)
                {
                    return DeliveryPeriod.Value.Hours;
                }
                return null;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedDeliveryPeriod
        {
            get
            {
                if (DeliveryPeriod.HasValue)
                {
                    TimeSpan diff = DeliveryPeriod.Value;
                    return $"{diff.Days} д. {diff.Hours:D2}:{diff.Minutes:D2} ч.";
                }
                return string.Empty;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedDeliveryDay
        {
            get
            {
                if (DeliveryPeriod.HasValue)
                {
                    TimeSpan diff = DeliveryPeriod.Value;
                    return $"{diff.Days} д.";
                }
                return string.Empty;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedDeliveryHours
        {
            get
            {
                if (DeliveryPeriod.HasValue)
                {
                    TimeSpan diff = DeliveryPeriod.Value;
                    return $"{diff.Hours:D2}:{diff.Minutes:D2} ч.";
                }
                return string.Empty;
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedProcessingDate
        {
            get
            {
                return ProcessingDate.HasValue ? $"{ProcessingDate.Value.ToString("dd.MM.yyyy")}" : "";
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedProcessingTime
        {
            get
            {
                return ProcessingDate.HasValue ? $"{ProcessingDate.Value.ToString("HH:mm:ss")}" : "";
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FullFormattedProcessingDate
        {
            get
            {
                return ProcessingDate.HasValue ? $"{FormattedProcessingDate} {FormattedProcessingTime}" : "";
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedShippingDate
        {
            get
            {
                return ShippingDate.HasValue ? $"{ShippingDate.Value.ToString("dd.MM.yyyy")}" : "";
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FormattedShippingTime
        {
            get
            {
                return ShippingDate.HasValue ? $"{ShippingDate.Value.ToString("HH:mm:ss")}" : "";
            }
            set
            {
                
            }
        }

        [NotMapped]
        public string FullFormattedShippingDate
        {
            get
            {
                return ShippingDate.HasValue ? $"{FormattedShippingDate} {FormattedShippingTime}" : "";
            }
            set
            {
                
            }
        }




        [MaxLength(200)]
        [Display(Name = "Статус Ozon")]
        public string? Status { get; set; }

        [Display(Name = "Статус в отчете")]
        public int? AppStatusId { get; set; }
        [Display(Name = "Статус в отчете")]
        public virtual AppStatus? AppStatus { get; set; }
        [Display(Name = "Дата последнего изменения статуса")]
        public DateTime? LastStatusChangeDate { get; set; } = null;

        [NotMapped]
        public string FormattedLastStatusChangeDate
        {
            get
            {
                return LastStatusChangeDate.HasValue ? $"{LastStatusChangeDate.Value.ToString("dd.MM.yyyy")}" : "";
            }
            set
            {
                
            }
        }
        
        [NotMapped]
        public string FullFormattedLastStatusChangeDate
        {
            get
            {
                return LastStatusChangeDate.HasValue ? $"{LastStatusChangeDate.Value.ToString("dd.MM.yyyy HH:mm")}" : "";
            }
            set
            {
                
            }
        }

        [Display(Name = "Сумма отправления")]
        
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal? ShipmentAmount { get; set; }

        [Display(Name = "Наименование")]
        [MaxLength(255)]
        public string? ProductName { get; set; }

        [MaxLength(255)]
        [Display(Name = "Ключ")]
        public string? ProductKey { get; set; }

        [MaxLength(255)]
        [Display(Name = "Арикул")]
        public string? Article { get; set; }

        public int? ManufacturerId { get; set; }
        [Display(Name = "Производитель")]
        public Manufacturer? Manufacturer { get; set; }
        
        public int? EtProducerId { get; set; }
        [Display(Name = "Производитель JCEtalon")]
        [NotMapped]
        public EtProducer? EtProducer { get; set; }

        [Display(Name = "Цена")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal? Price { get; set; }

        [Display(Name = "Количество")]
        public int? Quantity { get; set; }

        [Display(Name = "Склад отгрузки")]
        [MaxLength(100)]
        public int? ShipmentWarehouseId { get; set; }
        public virtual Warehouse? ShipmentWarehouse { get; set; }

        [Display(Name = "Валюта")]
        public int? СurrencyId { get; set; }
        public virtual Currency? Сurrency { get; set; }

        public int? SupplierId { get; set; }
        [MaxLength(255)]
        [Display(Name = "Поставщик")]
        public Supplier? Supplier { get; set; }

        [Display(Name = "Номер заказа")]
        [MaxLength(50)]
        public string? OrderNumberToSupplier { get; set; } = null;

        [Display(Name = "Цена закупки")]
        public decimal? PurchasePrice { get; set; }

        [Display(Name = "Цена закупки до перевода")]
        public decimal? OriginalPurchasePrice { get; set; }

        [Display(Name = "Данные Продукта")]
        public int? ProductInfoId { get; set; }
        public virtual Product? ProductInfo { get; set; }

        [Display(Name = "Минимальная комиссия ОЗОН")]
        public decimal? MinOzonCommission { get; set; }

        [Display(Name = "Максимальная комиссия ОЗОН")]
        public decimal? MaxOzonCommission { get; set; }

        [Display(Name = "Информация по комиссии")]
        public string? MaxCommissionInfo { get; set; }

        [Display(Name = "Информация по комиссии")]
        public string? MinCommissionInfo { get; set; }

        [Display(Name = "Минимальная прибыль")]
        public decimal? MinProfit { get; set; }

        [Display(Name = "Максимальная прибыль")]
        public decimal? MaxProfit { get; set; }

        [Display(Name = "Минимальная скидка, %")]
        public decimal? MinDiscount { get; set; }

        [Display(Name = "Максивальная скидка, %")]
        public decimal? MaxDiscount { get; set; }

        [Display(Name = "Себестоимость")]
        public decimal? CostPrice { get; set; } 

        [Display(Name = "Город доставки")]
        [MaxLength(100)]
        public string? DeliveryCity { get; set; }

        [MaxLength(255)]
        [Display(Name = "Категория")]
        public string? NewCategory { get; set; }

        [Display(Name = "Статус обработки")]
        public bool IsVerified { get; set; } = false;

        [Display(Name = "Принят")]
        public bool IsAccepted { get; set; } = false;

        [Display(Name = "Комментарий")]
        [MaxLength(500)]
        public string? Comment { get; set; }

        [Display(Name = "Обновленные столбцы")]
        public List<string>? UpdatedColumns { get; set; } = [];

        [Display(Name = "Пользователь обновивший запись")]
        public string? UpdatedBy { get; set; } = "Ozon";

        [Display(Name = "Подлежит возврату")]
        public bool? IsReturnable { get; set; } = false;

        public int? TransactionId { get; set; }

        public int? OzonClientId { get; set; }
        [Display(Name = "Ozon клиент")]
        public OzonClient? OzonClient { get; set; } = null;

        public int? FromFiletId { get; set; }
        [Display(Name = "Подгружен из Excel файла")]
        public bool? FromFile { get; set; } = false;

        public int? ExcelFileDataId { get; set; }
        [Display(Name = "Данные о файле")]
        public OrdersFileMetadata? ExcelFileData { get; set; }

        [NotMapped]
        public int NumberInExcel {  get; set; }

        public int? CartItemId  { get; set; }

        [NotMapped]
        public ItemStatus CartItemStatus { get;  set; }
        [NotMapped]
        public StatusColor? CartItemStatusColor { get; set; }
        
        public int? DeliveryId { get; set; }
        [Display(Name = "Тип доставки")]
        public Delivery? Delivery { get; set; }
        

        public Order Clone()
        {
            var clone = new Order();
            var type = typeof(Order);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.Name != nameof(Id));

            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                property.SetValue(clone, value);
            }

            return clone;
        }
        
    }
}
