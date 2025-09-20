namespace OServcies.FiltersServcies.FilterModels
{
    public class ProductFilterModel : ITableFilterModel
    {
        public string? Article { get; set; }
        public string? OzonProductId { get; set; }
        public string? FboOzonSkuId { get; set; }
        public string? FbsOzonSkuId { get; set; }
        public string? CommercialCategory { get; set; }
        public double? Volume { get; set; }
        public double? VolumetricWeight { get; set; }
        public decimal? CurrentPriceWithDiscount { get; set; }
    }
}
