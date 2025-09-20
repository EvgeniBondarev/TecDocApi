namespace Servcies.ApiServcies.InterpartsApi.Models
{
    public class SupplierDetailedInformation
    {
        public string Direction { get; set; }
        public string SupplierName { get; set; }    
        public int DeliveryDaysMax { get; set; }
        public int DeliveryDaysMid { get; set; }
        public decimal Wight {  get; set; }
        public string Code { get; set; } 
        public decimal Price { get; set; }
        public decimal SupplierPrice { get; set; }
        public decimal DeliveryWeightTax { get; set; }
        public string Description { get; set; }
        public string Packing { get; set; }
        public string Brand { get; set; }
    }
}
