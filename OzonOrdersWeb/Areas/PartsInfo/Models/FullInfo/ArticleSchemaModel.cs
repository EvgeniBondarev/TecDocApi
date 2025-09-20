using System.ComponentModel.DataAnnotations;

namespace OzonOrdersWeb.Areas.PartsInfo.Models.FullInfo
{
    public class ArticleSchemaModel
    {
        public int SupplierId { get; set; }

        [Display(Name = "Артикул в нормальном написании (со спецсимволами)")]
        public string DataSupplierArticleNumber { get; set; }

        [Display(Name = "Статус изделия (нормальный, снят с производства и др.)")]
        public string ArticleStateDisplayValue { get; set; }

        [Display(Name = "Дополнительное описание (примечание)")]
        public string Description { get; set; }

        [Display(Name = "Является сопутствующим товаром?")]
        public bool FlagAccessory { get; set; }

        [Display(Name = "Сертифицированное сырье?")]
        public bool FlagMaterialCertification { get; set; }

        [Display(Name = "Восстановленное изделие?")]
        public bool FlagRemanufactured { get; set; }

        [Display(Name = "Поставляется без упаковки?")]
        public bool FlagSelfServicePacking { get; set; }

        [Display(Name = "Артикул в поисковом написании")]
        public string FoundString { get; set; }

        [Display(Name = "Имеет применяемость в осях?")]
        public bool HasAxle { get; set; }

        [Display(Name = "Имеет применяемость в коммерческих ТС?")]
        public bool HasCommercialVehicle { get; set; }

        [Display(Name = "Связь с серийными номерами автомобилей")]
        public bool HasCVManuID { get; set; }

        [Display(Name = "Имеет применяемость в двигателях?")]
        public bool HasEngine { get; set; }

        [Display(Name = "Имеет применяемость?")]
        public bool HasLinkitems { get; set; }

        [Display(Name = "Имеет применяемость в мототехнике?")]
        public bool HasMotorbike { get; set; }

        [Display(Name = "Имеет применяемость в легковых ТС?")]
        public bool HasPassengerCar { get; set; }

        [Display(Name = "Артикул разрешен к использованию в БД?")]
        public bool IsValid { get; set; }

        [Display(Name = "Основное описание (наименование)")]
        public string NormalizedDescription { get; set; }

        [Display(Name = "Упаковочная единица")]
        public int PackingUnit { get; set; }

        [Display(Name = "Количество в упаковке")]
        public int QuantityPerPackingUnit { get; set; }
    }
}
