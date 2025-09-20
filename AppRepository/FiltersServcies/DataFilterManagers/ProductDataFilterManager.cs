using OServcies.FiltersServcies.FilterModels;
using OzonDomains.Models;


namespace Servcies.FiltersServcies.DataFilterManagers
{
    public class ProductDataFilterManager : IFilterManager<Product, ProductFilterModel>
    {
        private DataFilter<Product> _filter;

        public ProductDataFilterManager(DataFilter<Product> filter)
        {
            _filter = filter;
        }

        public List<Product> FilterByFilterData(List<Product> products, ProductFilterModel filterData)
        {
            products = _filter.FilterByString(products, pr => pr.Article, filterData.Article).ToList();
            products = _filter.FilterByString(products, pr => pr.FboOzonSkuId, filterData.FboOzonSkuId).ToList();
            products = _filter.FilterByString(products, pr => pr.FbsOzonSkuId, filterData.FbsOzonSkuId).ToList();
            products = _filter.FilterByString(products, pr => pr.CommercialCategory, filterData.CommercialCategory).ToList();
            products = _filter.FilterByDouble(products, pr => pr.Volume, filterData.Volume).ToList();
            products = _filter.FilterByDouble(products, pr => pr.VolumetricWeight, filterData.VolumetricWeight).ToList();
            products = _filter.FilterByDecimal(products, pr => pr.CurrentPriceWithDiscount, filterData.CurrentPriceWithDiscount).ToList();

            return products;
        }

        public List<Product> FilterByRadioButton(List<Product> filterModel, string buttonState)
        {
            throw new NotImplementedException();
        }
    }
}
