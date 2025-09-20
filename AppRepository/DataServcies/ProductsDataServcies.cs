using OzonDomains.Models;
using OzonRepositories.Data;

namespace Servcies.DataServcies
{
    public class ProductsDataServcies : IDataServcies
    {
        private readonly ProductRepository _repository;

        public ProductsDataServcies(ProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddProduct(Product value)
        {
            return await _repository.Add(value);
        }

        public async Task<int> DeleteProduct(Product value)
        {
            return await _repository.Delete(value);
        }

        public async Task<List<Product>> GetProducts()
        {
            return await _repository.Get();
        }

        public async Task<Product> GetProductAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<Product> GetProductAsync(Product value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<int> Update(Product value)
        {
            return await _repository.Update(value);
        }
        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }
    }
}
