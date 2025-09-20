using System.Text;

namespace Servcies.Builders;

public class OrderSummaryTableBuilder
{
    private readonly StringBuilder _builder;

    public OrderSummaryTableBuilder()
    {
        _builder = new StringBuilder();
        _builder.AppendLine("<table class=\"table table-bordered table-striped table-sm\">");
        _builder.AppendLine("<thead><tr>");
        _builder.AppendLine("<th>Shipment №</th>");
        _builder.AppendLine("<th>Статус</th>");
        _builder.AppendLine("<th>Описание</th>");
        _builder.AppendLine("<th>Дата</th>");
        _builder.AppendLine("</tr></thead>");
        _builder.AppendLine("<tbody>");
    }

    public void AddRow(string shipmentNumber, bool isSuccess, string description)
    {
        var status = isSuccess ? "✅ Успешно" : "❌ Ошибка";
        var time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

        _builder.AppendLine("<tr>");
        _builder.AppendLine($"<td>{shipmentNumber}</td>");
        _builder.AppendLine($"<td>{status}</td>");
        _builder.AppendLine($"<td>{description}</td>");
        _builder.AppendLine($"<td>{time}</td>");
        _builder.AppendLine("</tr>");
    }

    public string Build()
    {
        _builder.AppendLine("</tbody></table>");
        return _builder.ToString();
    }
}
