using OzonDomains;
using OzonDomains.Models;
using OzonRepositories.Context;
using OzonRepositories.Data;
using Servcies.DataServcies;

namespace Servcies.TransactionUtilsServcies
{
    public class TransactionManager : ITransactionManager
    {
        private readonly TransactionDataServcies _transactionDataServcies;
        private readonly OrderRepository _orderRepository;

        public TransactionManager(TransactionDataServcies transactionDataServcies)
        {
            _transactionDataServcies = transactionDataServcies;
        }
        
        private async Task<(int, string)> CreateTransaction(
            TransactionType type,
            List<Order> orders,
            string userName,
            DateTime createDateTime,
            string comment)
        {
            var transaction = new Transaction
            {
                Type = type,
                Orders = orders,
                CreateBy = userName,
                CreatedDateTime = createDateTime,
                Comment = comment
            };

            await _transactionDataServcies.AddTransaction(transaction);

            foreach (var order in orders)
            {
                ConfirmAccepted(order);
            }

            var result = await _transactionDataServcies.SaveChanges();
            return (result, $"{transaction.FormattedCreatedDate}\t{transaction.FormattedCreatedTime}");
        }
        
        public Task<(int, string)> CreateOrderToSupplierTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment)
            => CreateTransaction(TransactionType.OrderedToSupplier, orders, userName, createDateTime, comment);

        public Task<(int, string)> CreateShippedBySupplierTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment)
            => CreateTransaction(TransactionType.ShippedBySupplier, orders, userName, createDateTime, comment);

        public Task<(int, string)> CreateShippedToSellerTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment)
            => CreateTransaction(TransactionType.ShippedToSeller, orders, userName, createDateTime, comment);

        public Task<(int, string)> CreateOrderedToSellerTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment)
            => CreateTransaction(TransactionType.OrderedToSeller, orders, userName, createDateTime, comment);

        public Task<(int, string)> CreateShippedToClientTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment)
            => CreateTransaction(TransactionType.ShippedToClient, orders, userName, createDateTime, comment);

        public Task<(int, string)> CreatePercentageTransaction(List<Order> orders, string userName, DateTime createDateTime, string comment)
            => CreateTransaction(TransactionType.Percentage, orders, userName, createDateTime, comment);

        private void ConfirmAccepted(Order order)
        {
            order.IsAccepted = true;
            order.IsVerified = true;
            order.UpdatedColumns = null;
        }
    }
}
