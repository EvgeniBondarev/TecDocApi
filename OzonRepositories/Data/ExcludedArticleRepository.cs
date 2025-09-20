using Microsoft.EntityFrameworkCore;
using OzonDomains.Models;
using OzonRepositories.Context;

namespace OzonRepositories.Data
{
    public class ExcludedArticleRepository : MainRepository, IRepository<ExcludedArticle>
    {
        public ExcludedArticleRepository(OzonOrderContext orderContext) : base(orderContext)
        {
        }

        public async Task<int> Add(ExcludedArticle value)
        {
            _context.ExcludedArticles.Add(value);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(ExcludedArticle value)
        {
            _context.ExcludedArticles.Remove(value);
            return await _context.SaveChangesAsync();
        }

        public Task<List<ExcludedArticle>> Get()
        {
            return _context.ExcludedArticles
                .Include(e => e.OzonClient)
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }
        
        public Task<List<ExcludedArticle>> GetPaged(int page = 1, int pageSize = 20)
        {
            return _context.ExcludedArticles
                .Include(e => e.OzonClient)
                .OrderByDescending(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task<int> GetTotalCount()
        {
            return _context.ExcludedArticles.CountAsync();
        }

        public async Task<ExcludedArticle> GetAsync(int id)
        {
            return await _context.ExcludedArticles
                .Include(e => e.OzonClient)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<ExcludedArticle> GetAsync(ExcludedArticle value)
        {
            return await _context.ExcludedArticles
                .Include(e => e.OzonClient)
                .FirstOrDefaultAsync(e => e.Article == value.Article 
                                          && e.OzonClientId == value.OzonClientId);
        }

        public async Task<int> Update(ExcludedArticle value)
        {
            _context.ExcludedArticles.Update(value);
            return await _context.SaveChangesAsync();
        }
        
        public async Task<List<Order>> GetFilteredOrdersAsync(List<Order> orders)
        {
            if (orders.Count == 0)
                return orders;

            var clientNames = orders
                .Where(o => o.OzonClient != null)
                .Select(o => o.OzonClient.Name)
                .Distinct()
                .ToList();

            var articles = orders
                .Select(o => o.Article)
                .Distinct()
                .ToList();

            var excluded = await _context.ExcludedArticles
                .Include(e => e.OzonClient)
                .Where(e => clientNames.Contains(e.OzonClient.Name) && articles.Contains(e.Article))
                .Select(e => new { ClientName = e.OzonClient.Name, e.Article })
                .ToListAsync();

            return orders
                .Where(o => !excluded.Any(e =>
                    e.ClientName == o.OzonClient?.Name &&
                    e.Article == o.Article))
                .ToList();
        }
    }
}