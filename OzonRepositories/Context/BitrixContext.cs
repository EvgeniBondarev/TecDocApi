using Microsoft.EntityFrameworkCore;
using OzonDomains.Models.BitrixModels;

namespace OzonRepositories.Context;

public class BitrixContext : DbContext
{
    public BitrixContext(DbContextOptions<BitrixContext> options)
        : base(options)
    {
    }

    public DbSet<BIblockElementProperty> BIblockElementProperties { get; set; }
    public DbSet<BCatalogPrice> BCatalogPrices { get; set; }
    public DbSet<BCatalogStoreProduct> BCatalogStoreProducts { get; set; }
    public DbSet<BCatalogStore> BCatalogStores { get; set; }
    public DbSet<BCatalogGroup> BCatalogGroups { get; set; }
    public DbSet<RemainingStockBitrix> RemainingStockBitrix { get; set; }
    public DbSet<RemainingStockFlat> RemainingStockFlat { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<RemainingStockBitrix>().HasNoKey();
        modelBuilder.Entity<RemainingStockFlat>().HasNoKey();
        
        modelBuilder.Entity<BCatalogStoreProduct>()
            .HasIndex(x => new { x.ProductId, x.StoreId })
            .IsUnique();
        modelBuilder.Entity<BIblockElementProperty>()
            .HasIndex(x => new { x.IblockElementId, x.IblockPropertyId })
            .HasDatabaseName("ix_iblock_element_property_1");
        modelBuilder.Entity<BIblockElementProperty>()
            .HasIndex(x => x.IblockPropertyId)
            .HasDatabaseName("ix_iblock_element_property_2");
        modelBuilder.Entity<BCatalogPrice>()
            .HasIndex(x => new { x.ProductId, x.CatalogGroupId })
            .HasDatabaseName("IXS_CAT_PRICE_PID");
        modelBuilder.Entity<BCatalogPrice>()
            .HasIndex(x => x.CatalogGroupId)
            .HasDatabaseName("IXS_CAT_PRICE_GID");
        modelBuilder.Entity<BCatalogPrice>()
            .HasIndex(x => x.PriceScale)
            .HasDatabaseName("IXS_CAT_PRICE_SCALE");
    }
}