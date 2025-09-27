using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.IO;
using iText.IO.Font.Constants;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using OzonDomains;
using OzonDomains.Models;
using OzonDomains.Models.BitrixModels;

public class ShippedToSellerPdfBuilder : IPdfBuilder
{
    private MemoryStream _memoryStream;
    private PdfWriter _writer;
    private PdfDocument _pdf;
    private Document _document;
    private PdfFont _font;
    private PdfFont _boldFont;
    private Transaction _transaction;
    private ICollection<Order> _orders;
    private Dictionary<int, List<StockInfo>> _stocks;

    public ShippedToSellerPdfBuilder()
    {
        _memoryStream = new MemoryStream();
        _writer = new PdfWriter(_memoryStream);
        _pdf = new PdfDocument(_writer);
        _document = new Document(_pdf);
        _document.SetMargins(40, 40, 40, 40);

        // Убедимся, что используем правильную кодировку
        try
        {
            // Получаем корневую директорию проекта
            string projectRoot = Directory.GetCurrentDirectory();
            
            // Пробуем разные варианты названия папки с шрифтами
            string[] possibleFontFolders = {
                Path.Combine(projectRoot, "Fonts"),
                Path.Combine(projectRoot, "fonts"),
                Path.Combine(projectRoot, "Resources", "Fonts"),
                Path.Combine(projectRoot, "resources", "fonts"),
                Path.Combine(projectRoot, "wwwroot", "fonts"),
                Path.Combine(AppContext.BaseDirectory, "Fonts"),
                Path.Combine(AppContext.BaseDirectory, "fonts")
            };

            string fontsFolder = null;
            string arialPath = null;
            string arialBoldPath = null;

            // Ищем существующую папку со шрифтами
            foreach (var folder in possibleFontFolders)
            {
                if (Directory.Exists(folder))
                {
                    fontsFolder = folder;
                    
                    // Проверяем разные варианты написания файлов
                    string[] arialVariants = { "arial.ttf", "Arial.ttf", "ARIAL.ttf" };
                    string[] arialBoldVariants = { "arialbd.ttf", "arialbold.ttf", "Arialbd.ttf", "ArialBold.ttf" };
                    
                    foreach (var variant in arialVariants)
                    {
                        string path = Path.Combine(folder, variant);
                        if (File.Exists(path))
                        {
                            arialPath = path;
                            break;
                        }
                    }
                    
                    foreach (var variant in arialBoldVariants)
                    {
                        string path = Path.Combine(folder, variant);
                        if (File.Exists(path))
                        {
                            arialBoldPath = path;
                            break;
                        }
                    }
                    
                    if (arialPath != null) break;
                }
            }

            // Если нашли шрифты, используем их
            if (arialPath != null)
            {
                _font = PdfFontFactory.CreateFont(arialPath, PdfEncodings.IDENTITY_H);
                
                if (arialBoldPath != null)
                {
                    _boldFont = PdfFontFactory.CreateFont(arialBoldPath, PdfEncodings.IDENTITY_H);
                }
                else
                {
                    // Если нет жирного шрифта, используем обычный
                    _boldFont = PdfFontFactory.CreateFont(arialPath, PdfEncodings.IDENTITY_H);
                }
                
                Console.WriteLine($"Успешно загружены шрифты: {arialPath}");
            }
            else
            {
                // Если шрифты не найдены, используем встроенные
                _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                _boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                Console.WriteLine("Шрифты не найдены, используются стандартные Helvetica");
            }
        }
        catch (Exception ex)
        {
            _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            _boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Console.WriteLine($"Ошибка загрузки шрифтов: {ex.Message}");
            
            // Логируем дополнительную информацию для отладки
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Base Directory: {AppContext.BaseDirectory}");
            
            // Покажем какие папки существуют
            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                var directories = Directory.GetDirectories(currentDir);
                Console.WriteLine("Доступные папки:");
                foreach (var dir in directories)
                {
                    Console.WriteLine($"- {Path.GetFileName(dir)}");
                }
            }
            catch (Exception dirEx)
            {
                Console.WriteLine($"Ошибка при чтении директорий: {dirEx.Message}");
            }
        }
    }

    public void BuildHeader(Transaction transaction)
    {
        _transaction = transaction;
    }

    public void BuildOrdersTable(ICollection<Order> orders)
    {
        _orders = orders;
    }

    public void SetOrdersStock(Dictionary<int, List<StockInfo>> stocks)
    {
        _stocks = stocks;
    }

    public byte[] GetPdf()
    {
        try
        {
            // Заголовок документа
            _document.Add(new Paragraph($"{_transaction.Type?.GetDisplayName()} № {_transaction.Id} от {_transaction.FormattedCreatedTimeDateTime}")
                .SetFont(_boldFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.LEFT));

            // Информация об отправителе и получателе в одной строке
            var sender = _orders?.FirstOrDefault()?.ShipmentWarehouse?.Name ?? "Не указан";
            var receiver = _orders?.FirstOrDefault()?.OzonClient?.WarehouseName ?? "Не указан";

            _document.Add(new Paragraph($"Отправитель: {sender}")
                .SetFont(_font)
                .SetFontSize(11));

            _document.Add(new Paragraph($"Получатель: {receiver}")
                .SetFont(_font)
                .SetFontSize(11)
                .SetMarginBottom(10));

            // Таблица с товарами
            if (_orders != null && _orders.Any())
            {
                // Создаем таблицу с 5 колонками (добавили Остаток на складе)
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 8, 15, 40, 12, 25 }))
                    .UseAllAvailableWidth()
                    .SetWidth(UnitValue.CreatePercentValue(100));

                // Стиль для заголовков таблицы
                var headerStyle = new Style()
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetPadding(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(_boldFont)
                    .SetFontSize(10);

                // Стиль для ячеек с данными
                var cellStyle = new Style()
                    .SetPadding(8)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFont(_font)
                    .SetFontSize(9);

                // Заголовки таблицы
                table.AddHeaderCell(new Cell().Add(new Paragraph("№").SetTextAlignment(TextAlignment.CENTER)).AddStyle(headerStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Код").SetTextAlignment(TextAlignment.CENTER)).AddStyle(headerStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Товары").SetTextAlignment(TextAlignment.CENTER)).AddStyle(headerStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Количество").SetTextAlignment(TextAlignment.CENTER)).AddStyle(headerStyle));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Остаток на складе").SetTextAlignment(TextAlignment.CENTER)).AddStyle(headerStyle));

                // Данные таблицы
                int rowNumber = 1;
                foreach (var order in _orders)
                {
                    // №
                    table.AddCell(new Cell()
                        .Add(new Paragraph(rowNumber.ToString()).SetTextAlignment(TextAlignment.CENTER))
                        .AddStyle(cellStyle));

                    // Код товара
                    table.AddCell(new Cell()
                        .Add(new Paragraph(order.ProductKey ?? ""))
                        .AddStyle(cellStyle));

                    // Название товара
                    var productName = string.IsNullOrEmpty(order.ProductName) ? "" : order.ProductName;
                    table.AddCell(new Cell()
                        .Add(new Paragraph(productName))
                        .AddStyle(cellStyle));

                    // Количество
                    table.AddCell(new Cell()
                        .Add(new Paragraph($"{order.Quantity} шт").SetTextAlignment(TextAlignment.CENTER))
                        .AddStyle(cellStyle));

                    // Остаток на складе
                    var stockCell = new Cell().AddStyle(cellStyle);
                    
                    if (_stocks != null && _stocks.TryGetValue(order.Id, out var stockInfos) && stockInfos != null && stockInfos.Any())
                    {
                        // Создаем список остатков для этого заказа
                        foreach (var stock in stockInfos)
                        {
                            stockCell.Add(new Paragraph($"{stock.StoreTitle}: {stock.Amount} шт")
                                .SetFontSize(8)
                                .SetMargin(0)
                                .SetPadding(0));
                        }
                    }
                    else
                    {
                        stockCell.Add(new Paragraph("Нет данных").SetFontSize(8));
                    }
                    
                    table.AddCell(stockCell);

                    rowNumber++;
                }

                _document.Add(table);
            }

            // Добавляем комментарий если есть
            if (!string.IsNullOrEmpty(_transaction.Comment))
            {
                _document.Add(new Paragraph($"Примечание: {_transaction.Comment}")
                    .SetFont(_font)
                    .SetFontSize(10)
                    .SetMarginTop(10));
            }

            _document.Close();
            return _memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            return CreateFallbackPdf(ex.Message);
        }
    }

    private byte[] CreateFallbackPdf(string errorMessage = null)
    {
        var fallbackStream = new MemoryStream();
        var fallbackWriter = new PdfWriter(fallbackStream);
        var fallbackPdf = new PdfDocument(fallbackWriter);
        var fallbackDoc = new Document(fallbackPdf);

        fallbackDoc.Add(new Paragraph("Перемещение запасов").SetFontSize(16));
        
        if (!string.IsNullOrEmpty(errorMessage))
        {
            fallbackDoc.Add(new Paragraph($"Ошибка: {errorMessage}").SetFontColor(ColorConstants.RED));
        }

        fallbackDoc.Add(new Paragraph($"№ {_transaction.Id}"));
        fallbackDoc.Add(new Paragraph($"Дата: {_transaction.FormattedCreatedTimeDateTime}"));

        fallbackDoc.Close();
        return fallbackStream.ToArray();
    }
}