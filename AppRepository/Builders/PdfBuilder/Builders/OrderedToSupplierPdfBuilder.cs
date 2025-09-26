using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using OzonDomains;
using OzonDomains.Models;

public class OrderedToSupplierPdfBuilder : IPdfBuilder
{
    private MemoryStream _memoryStream;
    private PdfWriter _writer;
    private PdfDocument _pdf;
    private Document _document;
    private PdfFont _font;

    public OrderedToSupplierPdfBuilder()
    {
        _memoryStream = new MemoryStream();
        _writer = new PdfWriter(_memoryStream);
        _pdf = new PdfDocument(_writer);
        _document = new Document(_pdf);

        // Подключаем шрифт с поддержкой кириллицы
        _font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA); // можно заменить на шрифт с кириллицей
    }

    public void BuildHeader(Transaction transaction)
    {
        _document.Add(new Paragraph($"Транзакция: {transaction.Id}")
            .SetFont(_font)
            .SetFontSize(16));
        _document.Add(new Paragraph($"Тип: {transaction.Type?.GetDisplayName()}")
            .SetFont(_font));
        _document.Add(new Paragraph($"Создано: {transaction.FormattedCreatedTimeDateTime}")
            .SetFont(_font));
        _document.Add(new Paragraph($"Пользователь: {transaction.CreateBy}")
            .SetFont(_font));
        if (!string.IsNullOrEmpty(transaction.Comment))
            _document.Add(new Paragraph($"Комментарий: {transaction.Comment}")
                .SetFont(_font));
    }

    public void BuildOrdersTable(ICollection<Order> orders)
    {
        if (orders == null || !orders.Any()) return;

        Table table = new Table(3);
        table.AddHeaderCell(new Cell().Add(new Paragraph("Id").SetFont(_font)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Название").SetFont(_font)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Количество").SetFont(_font)));

        foreach (var order in orders)
        {
            table.AddCell(new Cell().Add(new Paragraph(order.Id.ToString()).SetFont(_font)));
            table.AddCell(new Cell().Add(new Paragraph(order.ShipmentNumber ?? "").SetFont(_font)));
            table.AddCell(new Cell().Add(new Paragraph(order.Quantity.ToString()).SetFont(_font)));
        }

        _document.Add(table);
    }

    public byte[] GetPdf()
    {
        _document.Close();
        return _memoryStream.ToArray();
    }
}
