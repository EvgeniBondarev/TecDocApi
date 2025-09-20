using OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;
using OzonOrdersWeb.Services.Cookies;
using OzonRepositories.Data;
using OzonRepositories.Data.Bitrix;
using PartsInfo.HttpUtils;
using Servcies.ApiServcies.OzonApi;
using Servcies.ApiServcies.TradesoftApi.Models.Response;
using Servcies.Builders;
using Servcies.CacheServcies.Cache.OrderSummaryCache;
using Servcies.DataServcies;
using Servcies.DataServcies.ExcelMapping;
using Servcies.FiltersServcies;
using Servcies.FiltersServcies.DataFilterManagers;
using Servcies.HangfireService;
using Servcies.ImportProductPricesServcies;
using Servcies.ParserServcies;
using Servcies.ParserServcies.FielParsers;
using Servcies.ParserServcies.HelpDictEnum;
using Servcies.PriceСlculationServcies;
using Servcies.TransactionUtilsServcies;
using TreeGrouping.Application.CategoryService;
using TreeGrouping.Application.DbService;

namespace OzonOrdersWeb.Extensions;

public static class RepositoryExtensions
{
    public static void AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<MainRepository>();

        services.AddTransient<OrderRepository>();
        services.AddTransient<OrdersDataServcies>();

        services.AddTransient<ProductRepository>();
        services.AddTransient<ProductsDataServcies>();

        services.AddTransient<AppStatusRepository>();
        services.AddTransient<AppStatusDataServcies>();

        services.AddTransient<SupplierRepository>();
        services.AddTransient<SupplierDataServcies>();

        services.AddTransient<TransactionRepository>();
        services.AddTransient<TransactionDataServcies>();

        services.AddTransient<DuplicateOrdersRepository>();
        services.AddTransient<DuplicateOrdersServcies>();

        services.AddTransient<OzonClientRepository>();
        services.AddTransient<OzonClientServcies>();

        services.AddTransient<WarehouseRepository>();
        services.AddTransient<WarehouseDataServcies>();

        services.AddTransient<ColumnMappingRepository>();
        services.AddTransient<ColumnMappingDataServcies>();

        services.AddTransient<ManufacturerRepository>();
        services.AddTransient<ManufacturerDataService>();

        services.AddTransient<OrdersFileMetadataRepository>();
        services.AddTransient<OrdersFileMetadataDataService>();

        services.AddTransient<MatchedResultRepository>();
        services.AddTransient<MatchedResultDataServices>();

        services.AddTransient<MatchedRowRepository>();
        services.AddTransient<MatchedRowDataServices>();

        services.AddTransient<UserAccessRepository>();
        services.AddTransient<UserAccessDataServices>();

        services.AddTransient<SavedMatchingColumnRepository>();
        services.AddTransient<SavedMatchingColumnDataServcies>();

        services.AddTransient<EtProducerRepository>();
        services.AddTransient<EtProducerDataServices>();

        services.AddTransient<FileUploadRecordRepository>();
        services.AddTransient<FileUploadRecordDataService>();

        services.AddTransient<StockDataService>();
        
        services.AddTransient<CryptographyServcies>(serviceProvider =>
        {
            var secret = configuration.GetConnectionString("Secret");
            var cryptographyServcies = new CryptographyServcies();
            if (secret != null) cryptographyServcies.SetSecret(secret);
            return cryptographyServcies;
        });


        services.AddTransient<ExcelParser>();
        services.AddTransient<ExcelExporter>();
        services.AddTransient<ExcelExporterBuilder>();

        services.AddTransient<CsvUrlParser>();
        services.AddTransient<StocksCaster>();
        
        

        services.AddTransient<OrderToSupplierTransactionManager>();

        services.AddTransient<CurrencyRateFetcher>();
        services.AddTransient<OrderPriceManager>();
        
        services.AddTransient<CookiesManeger>();
        services.AddTransient(typeof(DataFilter<>));
        services.AddTransient(typeof(QueryableDataFilter<>));
        services.AddTransient<OrderDataFilterManager>();
        services.AddTransient<ProductDataFilterManager>();
        services.AddTransient<TransactionDataFilterManager>();
        services.AddTransient<ManufacturerFilterManager>();
        services.AddTransient<SupplierDataFilterManager>();
        services.AddTransient<AppStatusDataFilterManager>();
        services.AddTransient<OzonClientDataFilterManager>();
        services.AddTransient<UserFilterManager>();
        services.AddTransient<ColumnMappingDataFilterManager>();
        services.AddTransient<EtProducerFilterManager>();   
        
        services.AddTransient<FullDetailInfoCaster>();
        services.AddTransient<SubstituteResultCaster>();
        services.AddTransient<OrderCaster>();
        
        
        services.AddScoped<IDatabaseService, DatabaseService>();

        services.AddScoped<CategoryCacheService>();
        services.AddScoped<CategoryFilterService>();
        services.AddScoped<CategoryTreeService>();

        services.AddScoped<OrderCartRepository>();
        services.AddScoped<OrderCartServcies>();
        services.AddScoped<OrderItemRepository>();
        
        services.AddTransient<IHangfireService, HangfireService>();
        
        services.AddScoped<IBitrixRepository, BitrixRepository>();
        services.AddTransient<BitrixStockRepository>();

        services.AddTransient<OrderSummaryTableBuilder>();
        services.AddTransient<SupplierModelBuilder>();
        services.AddTransient<ArticleFullModelBuilder>();
        services.AddTransient<CrossCodeBuilder>();
        services.AddTransient<SubstituteBuilder>();
        services.AddTransient<PartsFinderArticleModelBuilder>();
        services.AddTransient<CrossArticleModelBuilder>(); 
        services.AddTransient<ProductInformationModelBuilder>();
        services.AddTransient<TipsModelBuilder>();
        services.AddScoped<ArticleSearchModelBuilder>();

        services.AddTransient<OzonApiDataManager>();
        
        services.AddScoped<IExcelMappingRepository, ExcelMappingRepository>();
        services.AddScoped<IExcelMappingService, ExcelMappingService>();
        
        services.AddScoped<ExcludedArticleRepository>();
        services.AddScoped<ExcludedArticleDataServcies>();
        
        services.AddScoped<PriceHistoryRepository>();
        services.AddScoped<IPriceHistoryDataService, PriceHistoryDataService>();

        services.AddScoped<IOrderHistoryRepository, OrderHistoryRepository>();
        services.AddScoped<IOrderHistoryDataService, OrderHistoryDataService>();
        
        services.AddScoped<WarehouseMappingRepository>();
        services.AddScoped<WarehouseMappingDataServcies>();

        services.AddScoped<DeliveryRepository>();
        services.AddScoped<DeliveryDataServcies>();
        
        services.AddTransient<ImportProductPricesManager>();

    }
}