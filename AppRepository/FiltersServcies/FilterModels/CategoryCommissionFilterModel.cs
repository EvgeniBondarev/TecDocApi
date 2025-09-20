namespace OServcies.FiltersServcies.FilterModels
{
    public class CategoryCommissionFilterModel: ITableFilterModel
    {
        public string? CategoryName { get; set; }
        public decimal? CommissionOZON_FBO { get; set; }
        public decimal? CommissionOZON_FBS { get; set; }
    }
}
