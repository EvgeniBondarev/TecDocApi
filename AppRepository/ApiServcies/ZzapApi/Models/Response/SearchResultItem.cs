using Newtonsoft.Json;

public class SearchResultItem
{
    [JsonProperty("code_doc_b")]
    public long CodeDocB { get; set; }

    [JsonProperty("code_cat")]
    public long CodeCat { get; set; }

    [JsonProperty("descr_type_search")]
    public string DescrTypeSearch { get; set; }

    [JsonProperty("type_search")]
    public int TypeSearch { get; set; }

    [JsonProperty("class_man")]
    public string Brand { get; set; } // Renamed from ManufacturerClass for consistency

    [JsonProperty("logopath")]
    public string LogoPath { get; set; }

    [JsonProperty("partnumber")]
    public string PartNumber { get; set; }

    [JsonProperty("class_cat")]
    public string Name { get; set; } // Renamed from ClassCat for clarity

    [JsonProperty("imagepath")]
    public string ImagePath { get; set; }

    [JsonProperty("qty")]
    public string QuantityDescription { get; set; } // e.g., "9 шт."

    [JsonProperty("instock")]
    public int InStock { get; set; }

    [JsonProperty("wholesale")]
    public int Wholesale { get; set; }

    [JsonProperty("local")]
    public int Local { get; set; }

    [JsonProperty("price")]
    public string PriceDescription { get; set; } // e.g., "144р."

    [JsonProperty("price_date")]
    public DateTime? PriceDate { get; set; }

    [JsonProperty("descr_price")]
    public string DescrPrice { get; set; }

    [JsonProperty("descr_qty")]
    public string DescrQty { get; set; }

    [JsonProperty("class_user")]
    public string Warehouse { get; set; } // Renamed from ClassUser

    [JsonProperty("descr_rating_count")]
    public string DescrRatingCount { get; set; }

    [JsonProperty("rating")]
    public int Rating { get; set; }

    [JsonProperty("descr_address")]
    public string DescrAddress { get; set; }

    [JsonProperty("phone1")]
    public string Phone1 { get; set; }

    [JsonProperty("order_text")]
    public string OrderText { get; set; }

    [JsonProperty("user_key")]
    public string UserKey { get; set; }

    [JsonProperty("addr_map_geo1")]
    public double AddrMapGeo1 { get; set; }

    [JsonProperty("addr_map_geo2")]
    public double AddrMapGeo2 { get; set; }

    [JsonProperty("used")]
    public int Used { get; set; }

    [JsonProperty("apply")]
    public string Apply { get; set; }

    [JsonProperty("min_sum_order")]
    public decimal MinSumOrder { get; set; }

    [JsonProperty("descr_min_sum_order")]
    public string DescrMinSumOrder { get; set; }

    [JsonProperty("shipment")]
    public string Shipment { get; set; }

    [JsonProperty("courier")]
    public bool IsCourier { get; set; }

    [JsonProperty("instockV2")]
    public bool IsInStockV2 { get; set; }

    [JsonProperty("wholesaleV2")]
    public bool IsWholesaleV2 { get; set; }

    [JsonProperty("localV2")]
    public bool IsLocalV2 { get; set; }

    [JsonProperty("usedV2")]
    public bool IsUsedV2 { get; set; }

    [JsonProperty("priceV2")]
    public decimal Price { get; set; } // Clean price

    [JsonProperty("descr_priceV2")]
    public string DescrPriceV2 { get; set; }

    [JsonProperty("price_orig")]
    public decimal PriceOrig { get; set; }

    [JsonProperty("descr_price_orig")]
    public string DescrPriceOrig { get; set; }

    [JsonProperty("descr_type_price")]
    public string DescrTypePrice { get; set; }

    [JsonProperty("qtyV2")]
    public int Quantity { get; set; } // Clean quantity

    [JsonProperty("qty_max")]
    public int QuantityMax { get; set; }

    [JsonProperty("descr_qtyV2")]
    public string DescrQtyV2 { get; set; }

    [JsonProperty("delivery_days")]
    public int DeliveryDays { get; set; }

    [JsonProperty("descr_delivery")]
    public string DeliveryTime { get; set; } // Clean delivery time

    [JsonProperty("type_user")]
    public string TypeUser { get; set; }

    [JsonProperty("type_user2")]
    public string TypeUser2 { get; set; }

    [JsonProperty("type_price")]
    public string TypePrice { get; set; }

    [JsonProperty("imagepathV2")]
    public List<string> ImagePathV2 { get; set; }

    [JsonProperty("descr_price_date")]
    public string DescrPriceDate { get; set; }

    [JsonProperty("pack")]
    public int Pack { get; set; }

    [JsonProperty("descr_pack")]
    public string DescrPack { get; set; }

    [JsonProperty("type_chain_search")]
    public int TypeChainSearch { get; set; }

    [JsonProperty("noorig")]
    public bool IsNoOrig { get; set; }

    [JsonProperty("code_man")]
    public int CodeMan { get; set; }

    [JsonProperty("location")]
    public string Region { get; set; } // Renamed from Location

    [JsonProperty("descr_rating_year_count")]
    public string DescrRatingYearCount { get; set; }

    [JsonProperty("rating_year")]
    public int RatingYear { get; set; }

    [JsonProperty("descr_rating_total_count")]
    public string DescrRatingTotalCount { get; set; }

    [JsonProperty("rating_total")]
    public int RatingTotal { get; set; }

}