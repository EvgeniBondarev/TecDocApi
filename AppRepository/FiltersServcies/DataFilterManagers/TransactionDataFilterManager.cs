using Microsoft.EntityFrameworkCore;
using OzonDomains;
using OzonDomains.Models;
using Servcies.DataServcies;
using Servcies.FiltersServcies.FilterModels;
using System.Reflection;
using OServcies.FiltersServcies.FilterModels;


namespace Servcies.FiltersServcies.DataFilterManagers
{
    public class TransactionDataFilterManager
    {
        private QueryableDataFilter<Transaction> _filter;
        private TransactionDataServcies _servcies;
        public TransactionDataFilterManager(QueryableDataFilter<Transaction> filter,
                                            TransactionDataServcies transactionDataServcies)
        {
            _filter = filter;
            _servcies = transactionDataServcies;
        }
        public async Task<List<Transaction>> FilterByFilterData(List<Transaction> standartTransactions, OrderFilterModel orderFilterModel, TransactionFilterModel filterModel)
        {
            bool filterIsNull = AreAllPropertiesNull(filterModel);
            bool orderFilterIsNotNull = AreAllPropertiesNull(orderFilterModel);
            IQueryable<Transaction> transactions = _servcies.GetTransactions();
            
            if (filterIsNull && !orderFilterIsNotNull)
            {
                return await transactions.ToListAsync();
            }
            else if (filterIsNull && orderFilterIsNotNull)
            {
                return standartTransactions;
            }
            
            transactions = _filter.FilterByString(transactions, tr => tr.CreateBy, filterModel.CreateBy);
            transactions = _filter.FilterByDate(transactions, tr => tr.CreatedDateTime, filterModel.CreatedDateTime);
            transactions = _filter.FilterByEnum(transactions, t => t.Type, filterModel.Type);

            return await transactions.ToListAsync();
        }

        public async Task<List<Transaction>> FilterByRadioButton(List<Transaction> filterModel, string buttonState)
        {
            throw new NotImplementedException();
        }

        public bool AreAllPropertiesNull(TransactionFilterModel model)
        {
            PropertyInfo[] properties = typeof(TransactionFilterModel).GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == nameof(TransactionFilterModel.Type))
                {
                    if (model.Type.HasValue && model.Type != TransactionType.All)
                    {
                        return false;
                    }
                }
                else if (prop.Name == nameof(TransactionFilterModel.CreateBy))
                {
                    if (!string.IsNullOrEmpty(model.CreateBy) && model.CreateBy != "Все")
                    {
                        return false;
                    }
                }
                else if (prop.Name == nameof(TransactionFilterModel.CreatedDateTime))
                {
                    if (model.CreatedDateTime.HasValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public bool AreAllPropertiesNull(OrderFilterModel model)
        {
            PropertyInfo[] properties = typeof(OrderFilterModel).GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                object? value = prop.GetValue(model);
                if (value != null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
