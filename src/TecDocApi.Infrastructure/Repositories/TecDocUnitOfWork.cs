using Microsoft.EntityFrameworkCore;
using TecDocApi.Domain.Entities.TecDoc;
using TecDocApi.Domain.Interfaces;
using TecDocApi.Infrastructure.Data;

namespace TecDocApi.Infrastructure.Repositories;

public class TecDocUnitOfWork : IDisposable
{
    private readonly IDbContextFactory<TecDocContext> _contextFactory;
    private readonly TecDocContext? _context;

    public readonly ITecDocRepository<TdArticle> Articles;
    public readonly ITecDocRepository<TdSupplier> Suppliers;
    public readonly ITecDocRepository<TdSupplierDetail> SupplierDetails;
    public readonly ITecDocRepository<TdArticleCross> ArticleCrosses;
    public readonly ITecDocRepository<TdArticleOe> ArticleOes;
    public readonly ITecDocRepository<TdArticleAttribute> ArticleAttributes;
    public readonly ITecDocRepository<TdArticleImage> ArticleImages;
    public readonly ITecDocRepository<TdArticleLi> ArticleLis;
    public readonly ITecDocRepository<TdArticleEan> ArticleEans;
    public readonly ITecDocRepository<TdArticleInf> ArticleInfs;
    public readonly ITecDocRepository<TdArticleAcc> ArticleAccs;
    public readonly ITecDocRepository<TdArticleNn> ArticleNns;
    public readonly ITecDocRepository<TdManufacturer> Manufacturers;
    public readonly ITecDocRepository<TdModel> Models;
    public readonly ITecDocRepository<TdPassengerCar> PassengerCars;
    public readonly ITecDocRepository<TdPassengerCarAttribute> PassengerCarAttributes;

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

