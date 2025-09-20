using System.ComponentModel.DataAnnotations;

namespace OzonOrdersWeb.ViewModels.OrderViewModels;

public class OrderUpdateViewModel
{
    public int OrderId { get; set; } // Соответствует Order.Id

    [Display(Name = "Статус в отчете")]
    public int? AppStatusId { get; set; }

    [Display(Name = "Поставщик")]
    public int? SupplierId { get; set; }

    [Display(Name = "Номер заказа поставщику")]
    [MaxLength(50)]
    public string? OrderNumberToSupplier { get; set; }

    [Display(Name = "Вес продукта, кг")]
    public decimal? ProductWeight { get; set; } // Соответствует Order.ProductInfo.Weight

    [Display(Name = "Цена закупки")]
    public decimal? PurchasePrice { get; set; } // Цена, которую ввел пользователь, изначально из OriginalPurchasePrice

    // Поля, значения которых будут взяты из span/div и заполнены JS
    [Display(Name = "Цена закупки, RUB")]
    public decimal? PurchasePriceRub { get; set; }

    [Display(Name = "Себестоимость")]
    public decimal? CostPrice { get; set; }

    [Display(Name = "Минимальная комиссия ОЗОН")]
    public decimal? MinOzonCommission { get; set; }

    [Display(Name = "Максимальная комиссия ОЗОН")]
    public decimal? MaxOzonCommission { get; set; }

    [Display(Name = "Минимальная прибыль")]
    public decimal? MinProfit { get; set; }

    [Display(Name = "Максимальная прибыль")]
    public decimal? MaxProfit { get; set; }

    [Display(Name = "Минимальная наценка, %")]
    public decimal? MinDiscount { get; set; }

    [Display(Name = "Максимальная наценка, %")]
    public decimal? MaxDiscount { get; set; }

    public int? ProductInfoId { get; set; }
}