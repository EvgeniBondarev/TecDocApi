using Microsoft.EntityFrameworkCore;
using TecDocApi.Domain.Entities.TecDoc;

namespace TecDocApi.Infrastructure.Data;

/// <summary>
/// Контекст базы данных TecDoc
/// </summary>
public class TecDocContext : DbContext
{
    public TecDocContext(DbContextOptions<TecDocContext> options) : base(options) { }

    /// <summary>
    /// Артикулы - основная таблица с информацией об артикулах запчастей
    /// </summary>
    public DbSet<TdArticle> Articles => Set<TdArticle>();
    
    /// <summary>
    /// Поставщики - таблица с информацией о производителях неоригинальных запчастей
    /// </summary>
    public DbSet<TdSupplier> Suppliers => Set<TdSupplier>();
    
    /// <summary>
    /// Детали поставщиков - таблица с адресной информацией и контактами поставщиков
    /// </summary>
    public DbSet<TdSupplierDetail> SupplierDetails => Set<TdSupplierDetail>();
    
    /// <summary>
    /// Кросс-номера - таблица с оригинальными номерами производителей ТС для артикулов
    /// </summary>
    public DbSet<TdArticleCross> ArticleCrosses => Set<TdArticleCross>();
    
    /// <summary>
    /// Оригинальные кросс-номера - таблица с OEM номерами для артикулов
    /// </summary>
    public DbSet<TdArticleOe> ArticleOes => Set<TdArticleOe>();
    
    /// <summary>
    /// Характеристики/Критерии - таблица с характеристиками и критериями артикулов
    /// </summary>
    public DbSet<TdArticleAttribute> ArticleAttributes => Set<TdArticleAttribute>();
    
    /// <summary>
    /// Изображения и файлы - таблица с изображениями и документами для артикулов
    /// </summary>
    public DbSet<TdArticleImage> ArticleImages => Set<TdArticleImage>();
    
    /// <summary>
    /// Применяемость - таблица с информацией о применяемости артикулов к транспортным средствам и узлам
    /// </summary>
    public DbSet<TdArticleLi> ArticleLis => Set<TdArticleLi>();
    
    /// <summary>
    /// Штрих-коды - таблица с EAN штрих-кодами для артикулов
    /// </summary>
    public DbSet<TdArticleEan> ArticleEans => Set<TdArticleEan>();
    
    /// <summary>
    /// Информация/Описание - таблица с дополнительной информацией и описаниями для артикулов
    /// </summary>
    public DbSet<TdArticleInf> ArticleInfs => Set<TdArticleInf>();
    
    /// <summary>
    /// Сопутствующие товары/Аксессуары - таблица со связями артикулов с сопутствующими товарами и аксессуарами
    /// </summary>
    public DbSet<TdArticleAcc> ArticleAccs => Set<TdArticleAcc>();
    
    /// <summary>
    /// Новые номера - таблица с информацией о новых номерах артикулов (замена старых номеров)
    /// </summary>
    public DbSet<TdArticleNn> ArticleNns => Set<TdArticleNn>();
    
    /// <summary>
    /// Производители - таблица с информацией о производителях транспортных средств
    /// </summary>
    public DbSet<TdManufacturer> Manufacturers => Set<TdManufacturer>();
    
    /// <summary>
    /// Модели - таблица с информацией о моделях транспортных средств
    /// </summary>
    public DbSet<TdModel> Models => Set<TdModel>();
    
    /// <summary>
    /// Легковые автомобили - таблица с информацией о легковых транспортных средствах
    /// </summary>
    public DbSet<TdPassengerCar> PassengerCars => Set<TdPassengerCar>();
    
    /// <summary>
    /// Атрибуты легковых автомобилей - таблица с характеристиками и атрибутами легковых автомобилей
    /// </summary>
    public DbSet<TdPassengerCarAttribute> PassengerCarAttributes => Set<TdPassengerCarAttribute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Артикулы (articles) - составной ключ: supplierid + datasupplierarticlenumber
        modelBuilder.Entity<TdArticle>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber });
            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Articles)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Конвертация boolean полей из enum('True','False') в bool
            entity.Property(e => e.FlagAccessory)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.FlagMaterialCertification)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.FlagRemanufactured)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.FlagSelfServicePacking)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.HasAxle)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.HasCommercialVehicle)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.HasEngine)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.HasLinkItems)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.HasMotorbike)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.HasPassengerCar)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsValid)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
        });

        // Поставщики (suppliers) - первичный ключ: id
        modelBuilder.Entity<TdSupplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Указываем явные типы согласно SQL схеме
            entity.Property(e => e.Id)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataVersion)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.NbrOfArticles)
                .HasColumnType("int unsigned");
            // Конвертация boolean полей из enum('True','False')
            entity.Property(e => e.HasNewVersionArticles)
                .HasConversion(
                    v => v.HasValue ? (v.Value ? "True" : "False") : null,
                    v => v == null ? null : (v == "True"));
            entity.Property(e => e.Description)
                .HasColumnType("varchar(32)")
                .IsRequired(false);
            entity.Property(e => e.Matchcode)
                .HasColumnType("varchar(32)")
                .IsRequired(false);
        });

        // Детали поставщиков (supplier_details) - составной ключ: supplierid + addresstypeid
        modelBuilder.Entity<TdSupplierDetail>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.AddressTypeId });
            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Details)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.AddressTypeId)
                .HasColumnType("char(1)");
        });

        // Кросс-номера (article_cross) - составной ключ: manufacturerid + oenbr + supplierid + partsdatasupplierarticlenumber
        modelBuilder.Entity<TdArticleCross>(entity =>
        {
            entity.HasKey(e => new { e.ManufacturerId, e.OENbr, e.SupplierId, e.PartsDataSupplierArticleNumber });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Crosses)
                .HasForeignKey(e => new { e.SupplierId, e.PartsDataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Manufacturer)
                .WithMany(m => m.ArticleCrosses)
                .HasForeignKey(e => e.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.ManufacturerId)
                .HasColumnType("int unsigned");
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.OENbr)
                .HasColumnType("varchar(64)");
            entity.Property(e => e.PartsDataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
        });

        // Оригинальные кросс-номера (article_oe) - составной ключ: supplierid + datasupplierarticlenumber + oenbr
        modelBuilder.Entity<TdArticleOe>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber, e.OENbr });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.OeNumbers)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.OENbr)
                .HasColumnType("varchar(64)");
            // Конвертация boolean полей из enum('True','False') в bool
            entity.Property(e => e.IsAdditive)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
        });

        // Характеристики/Критерии (article_attributes) - составной ключ: supplierid + datasupplierarticlenumber + id
        modelBuilder.Entity<TdArticleAttribute>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber, e.Id });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Attributes)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.Id)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.Description)
                .HasColumnType("varchar(128)");
            entity.Property(e => e.DisplayTitle)
                .HasColumnType("varchar(128)");
            entity.Property(e => e.DisplayValue)
                .HasColumnType("varchar(4000)");
        });

        // Изображения и файлы (article_images) - составной ключ: supplierid + datasupplierarticlenumber + picturename
        modelBuilder.Entity<TdArticleImage>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber, e.PictureName });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Images)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.PictureName)
                .HasColumnType("varchar(64)");
            entity.Property(e => e.Description)
                .HasColumnType("varchar(64)");
            entity.Property(e => e.AdditionalDescription)
                .HasColumnType("varchar(64)");
            entity.Property(e => e.DocumentName)
                .HasColumnType("varchar(128)");
            entity.Property(e => e.DocumentType)
                .HasColumnType("varchar(8)");
            entity.Property(e => e.NormedDescriptionId)
                .HasColumnType("smallint unsigned");
            // Конвертация boolean полей из enum('True','False') в bool
            entity.Property(e => e.ShowImmediately)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
        });

        // Применяемость (article_li) - составной ключ: supplierid + datasupplierarticlenumber + linkagetypeid + linkageid
        modelBuilder.Entity<TdArticleLi>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber, e.LinkageTypeId, e.LinkageId });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Linkages)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.LinkageTypeId)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.LinkageId)
                .HasColumnType("int unsigned");
        });

        // Штрих-коды (article_ean) - составной ключ: supplierid + datasupplierarticlenumber + ean
        modelBuilder.Entity<TdArticleEan>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber, e.Ean });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.EanCodes)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.Ean)
                .HasColumnType("varchar(24)");
        });

        // Информация/Описание (article_inf) - составной ключ: supplierid + datasupplierarticlenumber + informationtypekey
        modelBuilder.Entity<TdArticleInf>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber, e.InformationTypeKey });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Information)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.InformationTypeKey)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.InformationText)
                .HasColumnType("text");
            entity.Property(e => e.InformationType)
                .HasColumnType("varchar(64)");
        });

        // Сопутствующие товары/Аксессуары (article_acc) - составной ключ: supplierid + datasupplierarticlenumber + accsupplierid + accdatasupplierarticlenumber
        modelBuilder.Entity<TdArticleAcc>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber, e.AccSupplierId, e.AccDataSupplierArticleNumber });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.Accessories)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AccSupplier)
                .WithMany()
                .HasForeignKey(e => e.AccSupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.AccSupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.AccDataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
        });

        // Новые номера (article_nn) - составной ключ: supplierid + datasupplierarticlenumber
        modelBuilder.Entity<TdArticleNn>(entity =>
        {
            entity.HasKey(e => new { e.SupplierId, e.DataSupplierArticleNumber });
            entity.HasOne(e => e.Article)
                .WithMany(a => a.NewNumbers)
                .HasForeignKey(e => new { e.SupplierId, e.DataSupplierArticleNumber })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.NewSupplier)
                .WithMany()
                .HasForeignKey(e => e.NewSupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.DataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            entity.Property(e => e.NewSupplierId)
                .HasColumnType("smallint unsigned");
            entity.Property(e => e.NewDataSupplierArticleNumber)
                .HasColumnType("varchar(32)");
            // Примечание: Колонка newbr может отсутствовать в некоторых версиях БД
            // Делаем поле опциональным и не используем в запросах
            entity.Ignore(e => e.NewNbr);
        });

        // Производители (manufacturers) - первичный ключ: id
        modelBuilder.Entity<TdManufacturer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnType("int unsigned");
        });

        // Модели (models) - первичный ключ: id
        modelBuilder.Entity<TdModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnType("int unsigned");
            
            entity.HasOne(e => e.Manufacturer)
                .WithMany()
                .HasForeignKey(e => e.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.ManufacturerId)
                .HasColumnType("int unsigned");
            
            // Конвертация boolean полей из enum('True','False') в bool
            entity.Property(e => e.CanBeDisplayed)
                .HasConversion(
                    v => v.HasValue ? (v.Value ? "True" : "False") : null,
                    v => v == "True" ? true : v == "False" ? false : null);
            entity.Property(e => e.HasLink)
                .HasConversion(
                    v => v.HasValue ? (v.Value ? "True" : "False") : null,
                    v => v == "True" ? true : v == "False" ? false : null);
            
            entity.Property(e => e.IsAxle)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsCommercialVehicle)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsEngine)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsMotorbike)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsPassengerCar)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsTransporter)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
        });

        // Легковые автомобили (passanger_cars) - первичный ключ: id
        modelBuilder.Entity<TdPassengerCar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnType("int unsigned");
            
            entity.HasOne(e => e.Model)
                .WithMany(m => m.PassengerCars)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.ModelId)
                .HasColumnType("int unsigned");
            
            // Конвертация boolean полей из enum('True','False') в bool
            entity.Property(e => e.CanBeDisplayed)
                .HasConversion(
                    v => v.HasValue ? (v.Value ? "True" : "False") : null,
                    v => v == "True" ? true : v == "False" ? false : null);
            entity.Property(e => e.HasLink)
                .HasConversion(
                    v => v.HasValue ? (v.Value ? "True" : "False") : null,
                    v => v == "True" ? true : v == "False" ? false : null);
            
            entity.Property(e => e.IsAxle)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsCommercialVehicle)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsCvManufacturerId)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsEngine)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsMotorbike)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsPassengerCar)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
            entity.Property(e => e.IsTransporter)
                .HasConversion(
                    v => v ? "True" : "False",
                    v => v == "True");
        });

        // Атрибуты легковых автомобилей (passanger_car_attributes) - составной ключ: passangercarid + attributegroup + attributetype
        modelBuilder.Entity<TdPassengerCarAttribute>(entity =>
        {
            entity.HasKey(e => new { e.PassengerCarId, e.AttributeGroup, e.AttributeType });
            entity.HasOne(e => e.PassengerCar)
                .WithMany(pc => pc.Attributes)
                .HasForeignKey(e => e.PassengerCarId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.PassengerCarId)
                .HasColumnType("int unsigned");
        });
    }
}

