using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace Servcies.ParserServcies.FileProcessing;

public class FileProcessingService : IFileProcessingService
    {
        public async Task<List<(string OrderNumber, string Article)>> ProcessFileAsync(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var ext = Path.GetExtension(file.FileName).ToLower();
            return ext switch
            {
                ".csv" => await ProcessCsvFileAsync(file),
                ".xlsx" or ".xls" => await ProcessExcelFileAsync(file),
                _ => throw new Exception("Неподдерживаемый формат файла")
            };
        }

        public async Task<List<(string OrderNumber, string Article)>> ProcessExcelFileAsync(IFormFile excelFile)
        {
            var processedOrders = new List<(string OrderNumber, string Article)>();

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;
            var colCount = worksheet.Dimension?.Columns ?? 0;

            if (rowCount == 0 || colCount == 0)
                throw new Exception("Файл не содержит данных");

            int orderNumberCol = -1;
            int articleCol = -1;

            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString()?.ToLower();
                if (header != null)
                {
                    if (header.Contains("номер") && header.Contains("заказ")) orderNumberCol = col;
                    if (header.Contains("артикул")) articleCol = col;
                }
            }

            if (orderNumberCol == -1 || articleCol == -1)
                throw new Exception("Не найдены необходимые столбцы 'Номер заказа' и 'Артикул'");

            for (int row = 2; row <= rowCount; row++)
            {
                var orderNumber = worksheet.Cells[row, orderNumberCol].Value?.ToString();
                var article = worksheet.Cells[row, articleCol].Value?.ToString();
                if (!string.IsNullOrEmpty(orderNumber) && !string.IsNullOrEmpty(article))
                    processedOrders.Add((orderNumber.Trim(), article.Trim()));
            }

            return processedOrders;
        }

        public async Task<List<(string OrderNumber, string Article)>> ProcessCsvFileAsync(IFormFile csvFile)
        {
            var processedOrders = new List<(string OrderNumber, string Article)>();

            using var stream = new MemoryStream();
            await csvFile.CopyToAsync(stream);
            stream.Position = 0;

            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };
            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync();
            csv.ReadHeader();
            var headers = csv.HeaderRecord?.Select(h => h?.ToLower()).ToArray();
            if (headers == null) throw new Exception("CSV файл пустой");

            int orderNumberCol = Array.FindIndex(headers, h => h.Contains("номер заказа") || h.Contains("номер отправления"));
            int articleCol = Array.FindIndex(headers, h => h.Contains("артикул"));

            if (orderNumberCol == -1 || articleCol == -1)
                throw new Exception("Не найдены необходимые столбцы 'Номер заказа' и 'Артикул'");

            while (await csv.ReadAsync())
            {
                var orderNumber = csv.GetField(orderNumberCol);
                var article = csv.GetField(articleCol);
                if (!string.IsNullOrEmpty(orderNumber) && !string.IsNullOrEmpty(article))
                    processedOrders.Add((orderNumber.Trim(), article.Trim()));
            }

            return processedOrders;
        }
    }