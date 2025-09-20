using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servcies.FiltersServcies.SortModels
{
    public enum OrderSortState
    {
        KeyAsc,                 // по ключу по возрастанию
        KeyDesc,                // по ключу по убыванию
        ShipmentNumberAsc,      // по номеру отправления по возрастанию
        ShipmentNumberDesc,     // по номеру отправления по убыванию
        ProcessingDateAsc,      // по дате обработки по возрастанию
        ProcessingDateDesc,     // по дате обработки по убыванию
        ShippingDateAsc,        // по дате отгрузки по возрастанию
        ShippingDateDesc,       // по дате отгрузки по убыванию
        StatusAsc,              // по статусу Ozon по возрастанию
        StatusDesc,             // по статусу Ozon по убыванию
        AppStatusIdAsc,         // по статусу в отчете по возрастанию
        AppStatusIdDesc,        // по статусу в отчете по убыванию
        ShipmentAmountAsc,      // по сумме отправления по возрастанию
        ShipmentAmountDesc,     // по сумме отправления по убыванию
        ProductNameAsc,         // по наименованию продукта по возрастанию
        ProductNameDesc,        // по наименованию продукта по убыванию
        ArticleAsc,             // по артикулу по возрастанию
        ArticleDesc,            // по артикулу по убыванию
        PriceAsc,               // по цене по возрастанию
        PriceDesc,              // по цене по убыванию
        QuantityAsc,            // по количеству по возрастанию
        QuantityDesc,           // по количеству по убыванию
        ShipmentWarehouseIdAsc, // по складу отгрузки по возрастанию
        ShipmentWarehouseIdDesc,// по складу отгрузки по убыванию
        СurrencyIdAsc,          // по валюте по возрастанию
        СurrencyIdDesc,         // по валюте по убыванию
        SupplierAsc,            // по поставщику по возрастанию
        SupplierDesc,           // по поставщику по убыванию
        PurchasePriceAsc,       // по цене закупки по возрастанию
        PurchasePriceDesc,      // по цене закупки по убыванию
        ProductInfoIdAsc,       // по данным продукта по возрастанию
        ProductInfoIdDesc,      // по данным продукта по убыванию
        MinOzonCommissionAsc,   // по минимальной комиссии ОЗОН по возрастанию
        MinOzonCommissionDesc,  // по минимальной комиссии ОЗОН по убыванию
        MaxOzonCommissionAsc,   // по максимальной комиссии ОЗОН по возрастанию
        MaxOzonCommissionDesc,  // по максимальной комиссии ОЗОН по убыванию
        MaxCommissionInfoAsc,   // по информации о максимальной комиссии по возрастанию
        MaxCommissionInfoDesc,  // по информации о максимальной комиссии по убыванию
        MinCommissionInfoAsc,   // по информации о минимальной комиссии по возрастанию
        MinCommissionInfoDesc,  // по информации о минимальной комиссии по убыванию
        ProfitAsc,              // по прибыли по возрастанию
        ProfitDesc,             // по прибыли по убыванию
        DiscountAsc,            // по скидке по возрастанию
        DiscountDesc,           // по скидке по убыванию
        DeliveryCityAsc,        // по городу доставки по возрастанию
        DeliveryCityDesc,       // по городу доставки по убыванию
        NewCategoryAsc,         // по категории по возрастанию
        NewCategoryDesc,        // по категории по убыванию
        IsVerifiedAsc,          // по статусу обработки по возрастанию
        IsVerifiedDesc,         // по статусу обработки по убыванию
        CommentAsc,             // по комментарию по возрастанию
        CommentDesc,            // по комментарию по убыванию
        UpdatedColumnsAsc,      // по обновленным столбцам по возрастанию
        UpdatedColumnsDesc      // по обновленным столбцам по убыванию
    }
}
