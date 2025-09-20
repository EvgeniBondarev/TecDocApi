using OzonDomains.Models;

namespace Servcies.TransactionUtilsServcies
{
    public interface ITransactionManager
    {
        Task<(int, string)> CreateTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment);
    }
}
