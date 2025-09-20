 using Newtonsoft.Json;

 public class ArticleResponse
    {
        [JsonProperty("distributorId")]
        public int DistributorId { get; set; }

        [JsonProperty("grp")]
        public string Grp { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("numberFix")]
        public string NumberFix { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("availability")]
        public int? Availability { get; set; }

        [JsonProperty("packing")]
        public int? Packing { get; set; } 

        [JsonProperty("deliveryPeriod")]
        public int? DeliveryPeriod { get; set; }  
    
        [JsonProperty("deliveryPeriodMax")]
        public int? DeliveryPeriodMax { get; set; } 

        [JsonProperty("deadlineReplace")]
        public string DeadlineReplace { get; set; }

        [JsonProperty("distributorCode")]
        public string DistributorCode { get; set; }

        [JsonProperty("supplierCode")]
        public long SupplierCode { get; set; }

        [JsonProperty("supplierColor")]
        public string SupplierColor { get; set; }

        [JsonProperty("supplierDescription")]
        public string SupplierDescription { get; set; }

        [JsonProperty("itemKey")]
        public string ItemKey { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("maxPrice")]
        public decimal MaxPrice { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

        [JsonProperty("volume")]
        public double? Volume { get; set; }

        [JsonProperty("lastUpdateTime")]
        public DateTime LastUpdateTime { get; set; }

        [JsonProperty("additionalPrice")]
        public decimal AdditionalPrice { get; set; }

        [JsonProperty("noReturn")]
        public bool NoReturn { get; set; }

        [JsonProperty("isUsed")]
        public bool IsUsed { get; set; }

        [JsonProperty("meta")]
        public ArticleMeta Meta { get; set; }

        [JsonProperty("deliveryProbability")]
        public int DeliveryProbability { get; set; }

        [JsonProperty("descriptionOfDeliveryProbability")]
        public string DescriptionOfDeliveryProbability { get; set; }

        [JsonProperty("priceIn")]
        public decimal PriceIn { get; set; }

        [JsonProperty("priceRate")]
        public double PriceRate { get; set; }

        [JsonProperty("isAnalog")]
        public bool IsAnalog { get; set; }
    }