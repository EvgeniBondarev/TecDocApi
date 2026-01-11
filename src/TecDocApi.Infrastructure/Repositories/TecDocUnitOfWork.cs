using Microsoft.EntityFrameworkCore;
using TecDocApi.Domain.Entities.TecDoc;
using TecDocApi.Domain.Interfaces;
using TecDocApi.Infrastructure.Data;

namespace TecDocApi.Infrastructure.Repositories;

public class TecDocUnitOfWork : IDisposable
{
    private readonly IDbContextFactory<TecDocContext> _contextFactory;
    private TecDocContext? _context;
    
    public ITecDocRepository<TdArticle> Articles { get; }
    public ITecDocRepository<TdSupplier> Suppliers { get; }
    public ITecDocRepository<TdSupplierDetail> SupplierDetails { get; }
    public ITecDocRepository<TdArticleCross> ArticleCrosses { get; }
    public ITecDocRepository<TdArticleOe> ArticleOes { get; }
    public ITecDocRepository<TdArticleAttribute> ArticleAttributes { get; }
    public ITecDocRepository<TdArticleImage> ArticleImages { get; }
    public ITecDocRepository<TdArticleLi> ArticleLis { get; }
    public ITecDocRepository<TdArticleEan> ArticleEans { get; }
    public ITecDocRepository<TdArticleInf> ArticleInfs { get; }
    public ITecDocRepository<TdArticleAcc> ArticleAccs { get; }
    public ITecDocRepository<TdArticleNn> ArticleNns { get; }
    public ITecDocRepository<TdManufacturer> Manufacturers { get; }
    public ITecDocRepository<TdModel> Models { get; }
    public ITecDocRepository<TdPassengerCar> PassengerCars { get; }
    public ITecDocRepository<TdPassengerCarAttribute> PassengerCarAttributes { get; }

    public TecDocUnitOfWork(IDbContextFactory<TecDocContext> contextFactory)
    {
        _contextFactory = contextFactory;
        _context = _contextFactory.CreateDbContext();
        
        Articles = new TecDocRepository<TdArticle>(_context);
        Suppliers = new TecDocRepository<TdSupplier>(_context);
        SupplierDetails = new TecDocRepository<TdSupplierDetail>(_context);
        ArticleCrosses = new TecDocRepository<TdArticleCross>(_context);
        ArticleOes = new TecDocRepository<TdArticleOe>(_context);
        ArticleAttributes = new TecDocRepository<TdArticleAttribute>(_context);
        ArticleImages = new TecDocRepository<TdArticleImage>(_context);
        ArticleLis = new TecDocRepository<TdArticleLi>(_context);
        ArticleEans = new TecDocRepository<TdArticleEan>(_context);
        ArticleInfs = new TecDocRepository<TdArticleInf>(_context);
        ArticleAccs = new TecDocRepository<TdArticleAcc>(_context);
        ArticleNns = new TecDocRepository<TdArticleNn>(_context);
        Manufacturers = new TecDocRepository<TdManufacturer>(_context);
        Models = new TecDocRepository<TdModel>(_context);
        PassengerCars = new TecDocRepository<TdPassengerCar>(_context);
        PassengerCarAttributes = new TecDocRepository<TdPassengerCarAttribute>(_context);
    }

    /// <summary>
    /// Создает новый контекст для параллельных запросов
    /// </summary>
    public TecDocContext CreateContext() => _contextFactory.CreateDbContext();

    public void Dispose()
    {
        _context?.Dispose();
    }
}

