using Microsoft.AspNetCore.Mvc.Rendering;
using OzonDomains;
using OzonDomains.Models;

namespace OzonOrdersWeb.ViewModels.OrderViewModels
{
    public class ExelDataViewModel
    {
        public List<string> TableHeaders { get; set; }
        public List<Dictionary<string, string>> TableData { get; set; }
        public Dictionary<string, string> ColumnMappings { get; set; }
        public List<string> MainTableHeaders { get; set; } = new List<string>(){
            "Номер заказа", "Клиент", "Принят в обработку", "Дата отгрузки", "Статус клинта",
            "Наименование товара", "Key", "Артикул", "Производитель", "Склад отгрузки", "Поставщик", "Цена сайта",
            "Цена", "Кол.", "Сумма отправления", "Цена закупки", "Категория", "Объемный вес", "Комиссия ОЗОН", "Код валюты отправления",
            "Прибыль", "Наценка %", "Город доставки", "Минимальная комиссия", "Максимальная комиссия"
        };
        public List<ColumnMapping> SavedMappings { get; set; }
        public string FilePath {  get; set; }
        public string FileName { get; set; }

        public List<OzonClient> OzonClients { get; set; }
        public OzonClient? SelectedClient { get; set; }

        public SelectList Statuses { get; set; }
        public string? SelectedStatus { get; set; }

        public List<Manufacturer> Manufacturers { get; set; }
        public Manufacturer? SelectedManufacturer { get; set; }

        public List<Warehouse> Warehouses { get; set; }
        public Warehouse? SelectedWarehouse { get; set; }
        public List<Supplier> Suppliers { get; set; }
        public Supplier? SelectedSupplier { get; set; }

        public List<(CurrencyCode, string)> CurrencyCodes { get; set; }
        public CurrencyCode SelectedCurrencyCode { get; set; }

        public DateTime? SelectedProcessingDate { get; set; }

        public DateTime? SelectedShippingDate { get; set; }
    }

}
