using Microsoft.EntityFrameworkCore;
using OzonDomains;
using OzonDomains.Models;
using OzonDomains.Models.MatchedRowSys;
using OzonDomains.Models.OrderCarts;
using OzonRepositories.Data.Bitrix;

namespace OzonRepositories.Context
{
    public class OzonOrderContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Currency> Currencys { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<AppStatus> AppStatuses { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<OzonClient> OzonClients { get; set; }
        public DbSet<UserAccess> UserAccess { get; set; } 
        public DbSet<ColumnMapping> ColumnMappings { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<OrdersFileMetadata> OrdersFileMetadata { get; set; }
        public DbSet<MatchedRow> MatchedRows { get; set; }
        public DbSet<MatchedResult> MatchedResults { get; set; }
        public DbSet<MatchingColumn> MatchingColumns { get; set; }
        public DbSet<SavedMatchingColumn> SavedMatchingColumns { get; set; }
        public DbSet<FileUploadRecord> FileUploadRecords { get; set; }
        
        //Cart
        public DbSet<OrderCart> OrderCarts { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartStatus> CartStatuses { get; set; }
        public DbSet<ItemStatus> ItemStatuses { get; set; }
        public DbSet<StatusColor> StatusColors { get; set; }
        public DbSet<ExcludedArticle> ExcludedArticles { get; set; }
        public DbSet<ExcelMapping> ExcelMappings { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public virtual DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<WarehouseMapping> WarehouseMappings { get; set; } 
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<DeliveryProvider> DeliveryProviders { get; set; }

        public OzonOrderContext()
        {
        }

        public OzonOrderContext(DbContextOptions<OzonOrderContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Order>()
                .ToTable("Orders", t => t.IsTemporal());
        }

    }
}
