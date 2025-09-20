using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class TransactionDataServcies : IDataServcies
    {
        private readonly TransactionRepository _repository;

        public TransactionDataServcies(TransactionRepository transactionRepository)
        {
            _repository = transactionRepository;
        }

        public async Task<int> AddTransaction(Transaction value)
        {
            return await _repository.Add(value);
        }

        public async Task<int> DeleteTransaction(Transaction value)
        {
            return await _repository.Delete(value);
        }

        public IQueryable<Transaction> GetTransactions()
        {
            return _repository.Get();
        }

        public async Task<Transaction> GetTransaction(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<Transaction> GetTransaction(Transaction value)
        {
            return await _repository.GetAsync(value);
        }

        public IQueryable<Transaction> GetTransactions(int skip, int take)
        {
            return _repository.GetTransactionsWithPagination(skip, take);
        }

        public async Task<int> GetTotalOrderCount()
        {
            return await _repository.GetTotalTransactionCount();
        }


        public Task<int> UpdateTransaction(Transaction value)
        {
            return _repository.Update(value);
        }

        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }

    }
}
