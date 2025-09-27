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

        try
        {
            // Попробуем использовать шрифты, которые точно поддерживают кириллицу
            string[] knownCyrillicFonts = {
                "times.ttf", "timesbd.ttf",               // Times New Roman
                "dejavu-sans.ttf", "dejavu-sans-bold.ttf", // DejaVu Sans
                "liberation-sans.ttf", "liberation-sans-bold.ttf", // Liberation Sans
                "freesans.ttf", "freesansbold.ttf",       // FreeSans
                "arial.ttf", "arialbd.ttf"                // Arial
            };

            string fontsFolder = null;
            string regularFontPath = null;
            string boldFontPath = null;

            // Ищем папку со шрифтами
            string[] possibleFontFolders = {
                Path.Combine(Directory.GetCurrentDirectory(), "Fonts"),
                Path.Combine(Directory.GetCurrentDirectory(), "fonts"),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts"),
                Path.Combine(AppContext.BaseDirectory, "Fonts"),
                Path.Combine(AppContext.BaseDirectory, "fonts")
            };

            foreach (var folder in possibleFontFolders)
            {
                if (Directory.Exists(folder))
                {
                    foreach (var fontFile in knownCyrillicFonts)
                    {
                        string path = Path.Combine(folder, fontFile);
                        if (File.Exists(path))
                        {
                            if (fontFile.Contains("bold") || fontFile.Contains("bd"))
                            {
                                boldFontPath = path;
                            }
                            else if (!fontFile.Contains("bold") && !fontFile.Contains("bd"))
                            {
                                regularFontPath = path;
                            }
                        }
                    }
                    
                    if (regularFontPath != null) break;
                }
            }

            if (regularFontPath != null)
            {
                _font = PdfFontFactory.CreateFont(regularFontPath, PdfEncodings.IDENTITY_H);
                _boldFont = boldFontPath != null 
                    ? PdfFontFactory.CreateFont(boldFontPath, PdfEncodings.IDENTITY_H)
                    : PdfFontFactory.CreateFont(regularFontPath, PdfEncodings.IDENTITY_H);
                
                Console.WriteLine($"Успешно загружены шрифты: {regularFontPath}");
            }
            else
            {
                TryLoadSystemFonts();
            }
        }
        catch (Exception ex)
        {
            TryLoadSystemFonts();
            Console.WriteLine($"Ошибка загрузки шрифтов: {ex.Message}");
        }
    }
    
    private void TryLoadSystemFonts()
    {
        try
        {
            // Пути к шрифтам в Linux
            string[] linuxFontPaths = {
                "/usr/share/fonts/truetype/freefont/FreeSans.ttf",
                "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
                "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                "/usr/share/fonts/truetype/msttcorefonts/Arial.ttf"
            };

            foreach (var path in linuxFontPaths)
            {
                if (File.Exists(path))
                {
                    _font = PdfFontFactory.CreateFont(path, PdfEncodings.IDENTITY_H);
                    _boldFont = PdfFontFactory.CreateFont(path, PdfEncodings.IDENTITY_H);
                    Console.WriteLine($"Используем системный шрифт: {path}");
                    return;
                }
            }

            // Если ничего не найдено, используем стандартные шрифты
            _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            _boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Console.WriteLine("Используются стандартные шрифты Helvetica (кириллица не поддерживается)");
        }
        catch
        {
            _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            _boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
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

           // Информация об отправителе (остается одна для всех заказов)
            var sender = _orders?.FirstOrDefault()?.ShipmentWarehouse?.Name ?? "Не указан";
            _document.Add(new Paragraph($"Отправитель: {sender}")
                .SetFont(_font)
                .SetFontSize(11));

            // Группируем заказы по клиентам
            var ordersByClient = _orders?
                .Where(o => o.OzonClient != null)
                .GroupBy(o => o.OzonClient)
                .ToList();

            if (ordersByClient != null && ordersByClient.Any())
            {
                foreach (var clientGroup in ordersByClient)
                {
                    var client = clientGroup.Key;
                    var clientOrders = clientGroup.ToList();
                    var receiver = client.WarehouseName ?? "Не указан";

                    // Добавляем заголовок для клиента
                    _document.Add(new Paragraph($"Получатель: {receiver} ({client.Name})")
                        .SetFont(_font)
                        .SetFontSize(11)
                        .SetMarginTop(15)
                        .SetMarginBottom(5));

                    // Таблица с товарами для текущего клиента
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

                    // Данные таблицы для текущего клиента
                    int rowNumber = 1;
                    foreach (var order in clientOrders)
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

                    // Добавляем отступ между таблицами клиентов
                    _document.Add(new Paragraph(" ").SetMarginBottom(10));
                }
            }
            else
            {
                _document.Add(new Paragraph("Получатель: Не указан")
                    .SetFont(_font)
                    .SetFontSize(11)
                    .SetMarginBottom(10));
                
                _document.Add(new Paragraph("Нет данных о заказах")
                    .SetFont(_font)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.RED));
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