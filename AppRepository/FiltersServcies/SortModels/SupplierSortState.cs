namespace Servcies.FiltersServcies.SortModels
{
    public enum SupplierSortState
    {
        StandardState,

        NameAsc, 
        NameDesc,

        CostFactorAsc, 
        CostFactorDesc,

        WeightFactorAsc,
        WeightFactorDesc,

        CurrencyCodeAsc, 
        CurrencyCodeDesc,

        WeightFactorCurrencyCodeAsc,
        WeightFactorCurrencyCodeDesc,
        
        CsvUrlAsc,
        CsvUrlDesc,
        
        SiteAsc,
        SiteDesc,
        
        AdditionalTermAsc,
        AdditionalTermDesc,
        
        IsVatApplicableAsc,
        IsVatApplicableDesc,
    }
}
