using OzonDomains.Models;

namespace Servcies.TransactionUtilsServcies
{
    public interface ITransactionManager
    {
        Task<(int, string)> CreateOrderToSupplierTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment);
    }
}
