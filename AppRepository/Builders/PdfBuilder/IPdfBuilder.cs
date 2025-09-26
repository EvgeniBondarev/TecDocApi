using OzonDomains.Models;

public interface IPdfBuilder
{
    void BuildHeader(Transaction transaction);
    void BuildOrdersTable(ICollection<Order> orders);
    byte[] GetPdf();
}