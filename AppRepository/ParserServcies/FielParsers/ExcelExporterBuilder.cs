using OfficeOpenXml;
using OzonDomains.Models;

namespace Servcies.ParserServcies.FielParsers
{
    public class ExcelExporterBuilder
    {
        private List<Order> _orders;
        private List<string> _columnsToExport;
        private Dictionary<string, Func<Order, object>> _columnMappings;
        private Action<ExcelWorksheet> _additionalLogic;
        private List<string> _columnsToSum;

        public Dictionary<string, string> ColumnFormats = new Dictionary<string, string>
        {
            { "Номер заказа", "@" },
            { "Клиент", "@" },
            { "Принят в обработку", "dd.MM.yyyy HH:mm" },
            { "Дата отгрузки", "dd.MM.yyyy HH:mm" },
            { "Срок доставки", @"d' д. 'hh' ч.'mm' мин.'" },
            { "Статус клиента", "@" },
            { "Статус", "@" },
            { "Наименование товара", "@" },
            { "Артикул", "@" },
            { "Производитель", "@" },
            { "Склад отгрузки", "@" },
            { "Поставщик", "@" },
            { "Номер заказа поставщику", "@" },
            { "Цена сайта", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Цена", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Количество", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Сумма отправления", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Категория", "@" },
            { "Объемный вес", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Цена закупки", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Себестоимость", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Комиссия ОЗОН (мин.)", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Комиссия ОЗОН (макс.)", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Прибыль (мин.)", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Прибыль (макс.)", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Наценка % (мин.)", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Наценка % (макс.)", "#,0" }, // Изменено на целое число без десятичных знаков
            { "Город доставки", "@" },
            { "Цена закупки до перевода", "#,0" },
            { "Валюта", "@" }
        };


        public ExcelExporterBuilder()
        {
            _columnMappings = new Dictionary<string, Func<Order, object>>
            {
                { "Номер заказа", order => order.ShipmentNumber },
                { "Клиент", order => order.OzonClient?.Name },
                { "Принят в обработку", order => order.FullFormattedProcessingDate },
                { "Дата отгрузки", order => order.FullFormattedShippingDate },
                { "Срок доставки", order => order.FormattedDeliveryPeriod },
                { "Статус клиента", order => order.Status },
                { "Статус", order => order.AppStatus?.Name },
                { "Наименование товара", order => order.ProductName },
                { "Артикул", order => order.Article },
                { "Производитель", order => order.Manufacturer?.Name },
                { "Склад отгрузки", order => order.ShipmentWarehouse?.Name },
                { "Поставщик", order => order.Supplier?.Name },
                { "Номер заказа поставщику", order => order.OrderNumberToSupplier },
                { "Цена сайта", order => order.ProductInfo?.CurrentPriceWithDiscount },
                { "Цена", order => order.Price },
                { "Количество", order => order.Quantity },
                { "Сумма отправления", order => order.ShipmentAmount },
                { "Категория", order => order.ProductInfo?.CommercialCategory },
                { "Объемный вес", order => order.ProductInfo?.VolumetricWeight },
                { "Цена закупки", order => order.PurchasePrice },
                { "Себестоимость", order => order.CostPrice },
                { "Комиссия ОЗОН (мин.)", order => order.MinOzonCommission },
                { "Комиссия ОЗОН (макс.)", order => order.MaxOzonCommission },
                { "Прибыль (мин.)", order => order.MinProfit },
                { "Прибыль (макс.)", order => order.MaxProfit },
                { "Наценка % (мин.)", order => order.MinDiscount },
                { "Наценка % (макс.)", order => order.MaxDiscount },
                { "Город доставки", order => order.DeliveryCity },
                { "Цена закупки до перевода", order => order.OriginalPurchasePrice },
                { "Валюта",  order => order.Сurrency?.Name }
            };
            _columnsToExport = _columnMappings.Keys.ToList();
            _columnsToSum = new List<string>();
        }

        public ExcelExporterBuilder WithOrders(List<Order> orders)
        {
            _orders = orders;
            return this;
        }

        public ExcelExporterBuilder WithColumnsToExport(List<string> columns)
        {
            _columnsToExport = columns;
            return this;
        }

        public ExcelExporterBuilder WithColumnSums(List<string> columnsToSum)
        {
            _columnsToSum = columnsToSum;
            return this;
        }

        public byte[] Build()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Orders");

                BuildTable(worksheet);
                AddColumnSums(worksheet);

                worksheet.Cells.AutoFitColumns();
                
                _additionalLogic?.Invoke(worksheet);

                return package.GetAsByteArray();
            }
        }

        private void BuildTable(ExcelWorksheet worksheet)
        {
            int colIndex = 1;
            
            for (int i = 0; i < _columnsToExport.Count; i++)
            {
                var columnName = _columnsToExport[i];

                if (columnName == "Комиссия ОЗОН" || columnName == "Прибыль" || columnName == "Наценка %")
                {
                    worksheet.Cells[1, colIndex].Value = columnName + " (мин.)";
                    colIndex++;
                    worksheet.Cells[1, colIndex].Value = columnName + " (макс.)";
                    colIndex++;
                }
                else
                {
                    worksheet.Cells[1, colIndex].Value = columnName;
                    colIndex++;
                }
            }

            int row = 2;
            foreach (var order in _orders)
            {
                colIndex = 1;
                foreach (var columnName in _columnsToExport)
                {
                    if (columnName == "Комиссия ОЗОН" || columnName == "Прибыль" || columnName == "Наценка %")
                    {
                        if (_columnMappings.TryGetValue(columnName + " (мин.)", out Func<Order, object> getMinValue))
                        {
                            worksheet.Cells[row, colIndex].Value = getMinValue(order);
                            colIndex++;
                        }

                        if (_columnMappings.TryGetValue(columnName + " (макс.)", out Func<Order, object> getMaxValue))
                        {
                            worksheet.Cells[row, colIndex].Value = getMaxValue(order);
                            colIndex++;
                        }
                    }
                    else if (_columnMappings.TryGetValue(columnName, out Func<Order, object> getValue))
                    {
                        worksheet.Cells[row, colIndex].Value = getValue(order);
                        colIndex++;
                    }
                }
                row++;
            }

            ApplyColumnFormats(worksheet, row);
        }

        private void ApplyColumnFormats(ExcelWorksheet worksheet, int totalRows)
        {
            for (int colIndex = 1; colIndex <= _columnsToExport.Count; colIndex++)
            {
                var columnName = _columnsToExport[colIndex - 1];
                if (ColumnFormats.TryGetValue(columnName, out var format))
                {
                    worksheet.Cells[2, colIndex, totalRows, colIndex].Style.Numberformat.Format = format;
                }
            }
        }

        private void AddColumnSums(ExcelWorksheet worksheet)
        {
            int row = _orders.Count + 2;

            foreach (var columnName in _columnsToSum)
            {
                int sumColumnIndex = _columnsToExport.IndexOf(columnName) + 1;
                if (sumColumnIndex > 0)
                {
                    worksheet.Cells[row, sumColumnIndex].Formula = $"SUM({worksheet.Cells[2, sumColumnIndex].Address}:{worksheet.Cells[row - 1, sumColumnIndex].Address})";
                    worksheet.Cells[row, sumColumnIndex].Style.Font.Bold = true;
                }
            }
        }
    }
}
