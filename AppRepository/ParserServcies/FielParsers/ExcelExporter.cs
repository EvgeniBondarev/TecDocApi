using OfficeOpenXml;
using OzonDomains.Models;


namespace Servcies.ParserServcies.FielParsers
{
    public class ExcelExporter
    {
        public Dictionary<string, Func<Order, object>> ColumnMappings = new Dictionary<string, Func<Order, object>>
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
                { "Производитель", order => order.EtProducer?.Name },
                { "Склад отгрузки", order => order.ShipmentWarehouse?.Name },
                { "Поставщик", order => order.Supplier?.Name },
                { "Номер заказа поставщику", order => order.OrderNumberToSupplier },
                { "Цена сайта", order => order.ProductInfo?.CurrentPriceWithDiscount },
                { "Цена", order => order.Price },
                { "Количество", order => order.Quantity },
                { "Сумма отправления", order => order.ShipmentAmount },
                { "Категория", order => order.ProductInfo?.CommercialCategory},
                { "Объемный вес", order => order.ProductInfo?.VolumetricWeight },
                { "Цена закупки", order => order.PurchasePrice },
                { "Оригинальная цена", order => order.OriginalPurchasePrice },
                { "Себестоимость", order => order.CostPrice },
                { "Комиссия ОЗОН (мин.)", order => order.MinOzonCommission },
                { "Комиссия ОЗОН (макс.)", order => order.MaxOzonCommission },
                { "Прибыль (мин.)", order => order.MinProfit },
                { "Прибыль (макс.)", order => order.MaxProfit },
                { "Наценка % (мин.)", order => order.MinDiscount },
                { "Наценка % (макс.)", order => order.MaxDiscount },
                { "Город доставки", order => order.DeliveryCity }
            };

        public byte[] ExportToExcel(List<Order> orders, List<string> columnsToExport)
        {
            

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Orders");

                int colIndex = 1;
                for (int i = 0; i < columnsToExport.Count; i++)
                {
                    var columnName = columnsToExport[i];

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
                foreach (var order in orders)
                {
                    colIndex = 1;
                    foreach (var columnName in columnsToExport)
                    {
                        if (columnName == "Комиссия ОЗОН" || columnName == "Прибыль" || columnName == "Наценка %")
                        {
                            if (ColumnMappings.TryGetValue(columnName + " (мин.)", out Func<Order, object> getMinValue))
                            {
                                object minValue = getMinValue(order);
                                worksheet.Cells[row, colIndex].Value = minValue;
                                colIndex++;
                            }

                            if (ColumnMappings.TryGetValue(columnName + " (макс.)", out Func<Order, object> getMaxValue))
                            {
                                object maxValue = getMaxValue(order);
                                worksheet.Cells[row, colIndex].Value = maxValue;
                                colIndex++;
                            }
                        }
                        else if (ColumnMappings.TryGetValue(columnName, out Func<Order, object> getValue))
                        {
                            object value = getValue(order);
                            worksheet.Cells[row, colIndex].Value = value;
                            colIndex++;
                        }
                    }
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        public byte[] ExportToExcel(List<Order> orders)
        {
            List<string> columnsToExport = ColumnMappings.Keys.ToList();
            return ExportToExcel(orders, columnsToExport);
        }
        
        public byte[] ExportToExcel(List<Dictionary<string, string>> data)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Data");

                // Если нет данных, возвращаем пустой файл
                if (data == null || data.Count == 0)
                {
                    return package.GetAsByteArray();
                }

                // Получаем все уникальные ключи из всех словарей
                var allKeys = data.SelectMany(d => d.Keys).Distinct().ToList();

                // Создаем заголовки столбцов
                int colIndex = 1;
                foreach (var key in allKeys)
                {
                    // Обработка специальных полей с мин/макс значениями
                    if (key == "Комиссия ОЗОН" || key == "Прибыль" || key == "Наценка %")
                    {
                        worksheet.Cells[1, colIndex].Value = key + " (мин.)";
                        colIndex++;
                        worksheet.Cells[1, colIndex].Value = key + " (макс.)";
                        colIndex++;
                    }
                    else
                    {
                        worksheet.Cells[1, colIndex].Value = key;
                        colIndex++;
                    }
                }

                // Заполняем данные
                int row = 2;
                foreach (var item in data)
                {
                    colIndex = 1;
                    foreach (var key in allKeys)
                    {
                        if (key == "Комиссия ОЗОН" || key == "Прибыль" || key == "Наценка %")
                        {
                            // Для полей с мин/макс значениями
                            if (item.TryGetValue(key + " (мин.)", out string minValue))
                            {
                                worksheet.Cells[row, colIndex].Value = minValue;
                            }
                            colIndex++;

                            if (item.TryGetValue(key + " (макс.)", out string maxValue))
                            {
                                worksheet.Cells[row, colIndex].Value = maxValue;
                            }
                            colIndex++;
                        }
                        else
                        {
                            if (item.TryGetValue(key, out string value))
                            {
                                worksheet.Cells[row, colIndex].Value = value;
                            }
                            colIndex++;
                        }
                    }
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return package.GetAsByteArray();
            }
        }
        
        public List<Dictionary<string, string>> AddOrdersToFileData(
            List<Dictionary<string, string>> standartFileData,
            Dictionary<int, Order> indexOrderMap,
            List<string> addColumnsList)
        {
            if (standartFileData == null || indexOrderMap == null || addColumnsList == null)
                return standartFileData ?? new List<Dictionary<string, string>>();

            foreach (var kvp in indexOrderMap)
            {
                var index = kvp.Key;
                var order = kvp.Value;

                if (index < 0 || index >= standartFileData.Count)
                    continue;

                var rowDict = standartFileData[index];

                foreach (var column in addColumnsList)
                {
                    if (column == "Комиссия ОЗОН" || column == "Прибыль" || column == "Наценка %")
                    {
                        AddColumnValue(rowDict, column + " (мин.)", order, ColumnMappings);
                        AddColumnValue(rowDict, column + " (макс.)", order, ColumnMappings);
                    }
                    else
                    {
                        AddColumnValue(rowDict, column, order, ColumnMappings);
                    }
                }
            }

            return standartFileData;
        }

        private void AddColumnValue(
            Dictionary<string, string> rowDict,
            string columnName,
            Order order,
            Dictionary<string, Func<Order, object>> mappings)
        {
            if (mappings.TryGetValue(columnName, out Func<Order, object> getValue))
            {
                var value = getValue(order)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    rowDict[columnName] = value;
                }
            }
        }
    }
}
