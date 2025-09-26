using OzonDomains;
using OzonDomains.Models;
using OzonRepositories.Context;
using Servcies.DataServcies;

namespace Servcies.TransactionUtilsServcies
{
    public class TransactionManager : ITransactionManager
    {
        private readonly TransactionDataServcies _transactionDataServcies;

        public TransactionManager(TransactionDataServcies transactionDataServcies)
        {
            _transactionDataServcies = transactionDataServcies;
        }

        public async Task<(int, string)> CreateOrderToSupplierTransaction(List<Order> orders, 
                                                string userName, 
                                                DateTime createDateTime,
                                                string comment)
        {
                Transaction transaction = new Transaction()
                {
                    Type = TransactionType.OrderedToSupplier,
                    Orders = orders,
                    CreateBy = userName,
                    CreatedDateTime = createDateTime,
                    Comment = comment
                };
                await _transactionDataServcies.AddTransaction(transaction);

                foreach(var order in orders)
                {
                    ConfirmAccepted(order);
                    
                }
                return (await _transactionDataServcies.SaveChanges(), $"{transaction.FormattedCreatedDate}\t{transaction.FormattedCreatedTime}");
        }
        
        public async Task<(int, string)> CreateShippedBySupplierTransaction(List<Order> orders, 
            string userName, 
            DateTime createDateTime,
            string comment)
        {
            Transaction transaction = new Transaction()
            {
                Type = TransactionType.ShippedBySupplier,
                Orders = orders,
                CreateBy = userName,
                CreatedDateTime = createDateTime,
                Comment = comment
            };
            await _transactionDataServcies.AddTransaction(transaction);

            foreach(var order in orders)
            {
                ConfirmAccepted(order);
                    
            }
            return (await _transactionDataServcies.SaveChanges(), $"{transaction.FormattedCreatedDate}\t{transaction.FormattedCreatedTime}");
        }

        public async Task<(int, string)> CreateShippedToSellerTransaction(List<Order> orders, 
            string userName, 
            DateTime createDateTime,
            string comment)
        {
            Transaction transaction = new Transaction()
            {
                Type = TransactionType.ShippedToSeller,
                Orders = orders,
                CreateBy = userName,
                CreatedDateTime = createDateTime,
                Comment = comment
            };
            await _transactionDataServcies.AddTransaction(transaction);

            foreach(var order in orders)
            {
                ConfirmAccepted(order);
                    
            }
            return (await _transactionDataServcies.SaveChanges(), $"{transaction.FormattedCreatedDate}\t{transaction.FormattedCreatedTime}");
        }
        
        public async Task<(int, string)> CreateOrderedToSellerTransaction(List<Order> orders, 
            string userName, 
            DateTime createDateTime,
            string comment)
        {
            Transaction transaction = new Transaction()
            {
                Type = TransactionType.OrderedToSeller,
                Orders = orders,
                CreateBy = userName,
                CreatedDateTime = createDateTime,
                Comment = comment
            };
            await _transactionDataServcies.AddTransaction(transaction);

            foreach(var order in orders)
            {
                ConfirmAccepted(order);
                    
            }
            return (await _transactionDataServcies.SaveChanges(), $"{transaction.FormattedCreatedDate}\t{transaction.FormattedCreatedTime}");
        }
        
        public async Task<(int, string)> CreateShippedToClientTransaction(List<Order> orders, 
            string userName, 
            DateTime createDateTime,
            string comment)
        {
            Transaction transaction = new Transaction()
            {
                Type = TransactionType.ShippedToSeller,
                Orders = orders,
                CreateBy = userName,
                CreatedDateTime = createDateTime,
                Comment = comment
            };
            await _transactionDataServcies.AddTransaction(transaction);

            foreach(var order in orders)
            {
                ConfirmAccepted(order);
                    
            }
            return (await _transactionDataServcies.SaveChanges(), $"{transaction.FormattedCreatedDate}\t{transaction.FormattedCreatedTime}");
        }

        public void ConfirmAccepted(Order order)
        {
            order.IsAccepted = true;
            order.IsVerified = true;
            order.UpdatedColumns = null;
        }
    }
}
