using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class ExcludedArticleDataServcies : IDataServcies
    {
        private readonly ExcludedArticleRepository _repository;

        public ExcludedArticleDataServcies(ExcludedArticleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Order>> ExcludeOrdersAsync(List<Order> orders)
        {
            return await _repository.GetFilteredOrdersAsync(orders);
        }

        public async Task<int> AddExcludedArticle(ExcludedArticle value)
        {
            return await _repository.Add(value);
        }

        public async Task<int> DeleteExcludedArticle(ExcludedArticle value)
        {
            return await _repository.Delete(value);
        }

        public async Task<List<ExcludedArticle>> GetExcludedArticles()
        {
            return await _repository.Get();
        }
        
        public async Task<(List<ExcludedArticle> Articles, int TotalCount)> GetExcludedArticlesPaged(int page = 1, int pageSize = 20)
        {
            var articles = await _repository.GetPaged(page, pageSize);
            var totalCount = await _repository.GetTotalCount();
            return (articles, totalCount);
        }


        public async Task<ExcludedArticle> GetExcludedArticleAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<ExcludedArticle> GetExcludedArticleAsync(ExcludedArticle value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> UpdateExcludedArticle(ExcludedArticle value)
        {
            return await _repository.Update(value);
        }

        public Task<int> SaveChanges()
        {
            return _repository.SaveChanges();
        }
        
        
    }
}