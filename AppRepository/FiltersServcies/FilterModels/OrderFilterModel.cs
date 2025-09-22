namespace OServcies.FiltersServcies.FilterModels
{
    public class OrderFilterModel: ITableFilterModel
    {
        public string? Key { get; set; }


        public string? ShipmentNumber { get; set; }

        public DateTime? ProcessingDate { get; set; }

        public DateTime? ShippingDate { get; set; }

        public int? TimeLeftDay { get; set; }

        private string? _status;
        public string? Status 
        {
            get { return _status; }
            set
            {
                if (value == "Все")
                {
                    _status = null;
                }
                else
                {
                    _status = value;
                }
            }
        }

        private string? _appStatus;

        public string? AppStatus
        {
            get { return _appStatus; }
            set
            {
                if (value == "Все")
                {
                    _appStatus = null;
                }
                else
                {
                    _appStatus = value;
                }
            }
        }


        public decimal? ShipmentAmount { get; set; }

        public string? ProductName { get; set; }

        public string? Article { get; set; }

        public string? Manufacturer { get; set; }
        public decimal? Price { get; set; }

        public int? Quantity { get; set; }

        public string? _shipmentWarehouse { get; set; }
        public string? ShipmentWarehouse
        {
            get { return _shipmentWarehouse; }
            set
            {
                if (value == "Все")
                {
                    _shipmentWarehouse = null;
                }
                else
                {
                    _shipmentWarehouse = value;
                }
            }
        }

        public string? Сurrency { get; set; }

        private string? _supplier;

        public string? Supplier
        {
            get { return _supplier; }
            set
            {
                if (value == "Все")
                {
                    _supplier = null;
                }
                else
                {
                    _supplier = value;
                }
            }
        }
        public string? OrderNumberToSupplier { get; set; } = null;

        public decimal? PurchasePrice { get; set; }

        public string? CommercialCategory { get; set; }

        public double? Volume { get; set; }

        public double? VolumetricWeight { get; set; }

        public decimal? CurrentPriceWithDiscount { get; set; }

        public decimal? OzonCommission { get; set; }

        public decimal? Profit { get; set; }

        public decimal? Discount { get; set; }

        public string? DeliveryCity { get; set; }

        public string? NewCategory { get; set; }

        private string? _ozonClient;

        public string? OzonClient
        {
            get { return _ozonClient; }
            set
            {
                if (value == "Все")
                {
                    _ozonClient = null;
                }
                else
                {
                    _ozonClient = value;
                }
            }
        }

        public decimal? CostPrice {  get; set; }

        public string? DeliveryPeriod { get; set; }

        public DateTime? LastStatusChangeDate { get; set; }
        
        public string? Delivery   { get; set; }
        public string? Provider {get; set; }
    }
}
