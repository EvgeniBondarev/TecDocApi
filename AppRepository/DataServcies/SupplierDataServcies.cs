using OzonDomains.Models;
using OzonRepositories.Data;
using Servcies.ApiServcies.TradesoftApi.Models.Response;

namespace Servcies.DataServcies
{
    public class SupplierDataServcies : IDataServcies
    {
        private readonly SupplierRepository _repository;

        public SupplierDataServcies(SupplierRepository repository)
        {
            _repository = repository;
        }

        public Task<int> AddSupplier(Supplier value)
        {
            return _repository.Add(value);
        }

        public Task<int> DeleteSupplier(Supplier value)
        {
            return (_repository.Delete(value));
        }

        public async Task<List<Supplier>> GetSuppliers()
        {
            return await _repository.Get();
        }

        public async Task<Supplier> GetSupplierAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public async Task<Supplier> GetSupplierAsync(Supplier value)
        {
            return await _repository.GetAsync(value);
        }

        public async Task<List<Supplier>> GetSuppliersWithUrlAsync()
        {
            return await _repository.GetWithUrls();
        }
        public async Task<Supplier> GetSupplierBySiteAsync(string url)
        {
            return await _repository.GetBySiteAsync(new Supplier(){Site = url});
        }

        public async Task<int> UpdateSupplier(Supplier value)
        {
            return (await _repository.Update(value));
        }
        public async Task<int> SaveChanges()
        {
            return await _repository.SaveChanges();
        }

        public async Task<List<PreOrderItem>> SetAdditionalTerm(List<PreOrderItem> preOrderItems)
        {
            if (preOrderItems.Count == 0) return preOrderItems;
            var targetSupplier = await GetSupplierBySiteAsync(preOrderItems[0].SiteUrl);
            if (targetSupplier?.AdditionalTerm == null) return preOrderItems;
            foreach (var item in preOrderItems)
            {
                var originalDeliveryDaysMax = item.DeliveryDays;
                item.DeliveryDays = (Int32.Parse(item.DeliveryDays) + targetSupplier.AdditionalTerm).ToString();
                item.DeliveryDaysMax += targetSupplier.AdditionalTerm.Value;
                item.DeliveryDaysMin += targetSupplier.AdditionalTerm.Value;
                item.Description += $"\nВ срок {originalDeliveryDaysMax} добавлено {targetSupplier.AdditionalTerm} дней (из настроек поставщика {targetSupplier.Name})";
            }
            return preOrderItems;
        }
    }
}
