using System.Globalization;
using OzonDomains.Models;
using Servcies.ApiServcies.OzonApi;
using Servcies.ApiServcies.OzonApi.Filters;

namespace Servcies.ImportProductPricesServcies;

public class ImportProductPricesManager
{
    private OzonApiDataManager _ozonApiDataManager;

    public ImportProductPricesManager()
    {
    }
    
    public async Task<ProductImportPricesResponse> SetProductPrices(string article, 
                                                                    OzonClient ozonClient,
                                                                    double YourPrice, double OldPrice,
                                                                    double MinPrice, double CostPrice)
    { 
        var pricesForArticle = await GetProductPrices(article, ozonClient);if (pricesForArticle == null)
        {
            return new ProductImportPricesResponse
            {
                Result = new List<ProductImportPriceResult>
                {
                    new ProductImportPriceResult
                    {
                        OfferId = article,
                        ProductId = 0,
                        Updated = false,
                        Errors = [new ProductImportPriceError()
                        {
                            Message = "Цены не найдены",
                            Code = "SERVER_ERROR"
                        }]
                    }
                }
            };
        }
        try
        {
            var result = await SetUserPrice(pricesForArticle.Items[0], ozonClient, 
                                                                  YourPrice, OldPrice, MinPrice, CostPrice);
            return result;
        }
        catch (Exception ex)
        {
            return new ProductImportPricesResponse
            {
                Result = new List<ProductImportPriceResult>
                {
                    new ProductImportPriceResult
                    {
                        OfferId = article,
                        ProductId = 0,
                        Updated = false,
                        Errors = [new ProductImportPriceError()
                        {
                            Message = $"Ошибка при расчёте цены: {ex.Message}",
                            Code = "SERVER_ERROR"
                        }]
                    }
                }
            };
        }
    }

    public async Task<ProductImportPricesResponse> UpdateProductPrices(string article, 
                                                                 OzonClient ozonClient,  
                                                                 double constantIndex, 
                                                                 double percent)
    { 
        var pricesForArticle = await GetProductPrices(article, ozonClient);if (pricesForArticle == null)
        {
            return new ProductImportPricesResponse
            {
                Result = new List<ProductImportPriceResult>
                {
                    new ProductImportPriceResult
                    {
                        OfferId = article,
                        ProductId = 0,
                        Updated = false,
                        Errors = [new ProductImportPriceError()
                        {
                        Message = "Цены не найдены",
                        Code = "SERVER_ERROR"
                    }]
                    }
                }
            };
        }
        try
        {
            var newPrice = CalculateNewPrice(pricesForArticle.Items[0]);
            var result = await SetNewPrice(pricesForArticle.Items[0], newPrice, ozonClient);
            return result;
        }
        catch (Exception ex)
        {
            return new ProductImportPricesResponse
            {
                Result = new List<ProductImportPriceResult>
                {
                    new ProductImportPriceResult
                    {
                        OfferId = article,
                        ProductId = 0,
                        Updated = false,
                        Errors = [new ProductImportPriceError()
                        {
                            Message = $"Ошибка при расчёте цены: {ex.Message}",
                            Code = "SERVER_ERROR"
                        }]
                    }
                }
            };
        }
    }

    private async Task<ProductPricesResponse> GetProductPrices(string article, OzonClient ozonClient)
    {
        if (_ozonApiDataManager == null)
        {
            _ozonApiDataManager = new OzonApiDataManager(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
        }
        else
        {
            _ozonApiDataManager.SetClient(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
        }
        
        return await _ozonApiDataManager.GetProductPricesByArticlesModel(new[] { article });
    }
    
    private double CalculateNewPrice(ProductPriceItemResponse productPriceItemResponse, double constantIndex = 0.91, double percent = 2)
    {
        if (productPriceItemResponse == null)
            throw new ArgumentNullException(nameof(productPriceItemResponse), "Объект цены не может быть null");

        if (productPriceItemResponse.Price == null)
            throw new ArgumentException("Данные о цене отсутствуют", nameof(productPriceItemResponse));

        if (productPriceItemResponse.PriceIndexes == null ||
            productPriceItemResponse.PriceIndexes.OzonIndexData == null)
            throw new ArgumentException("Отсутствуют данные об индексах цен", nameof(productPriceItemResponse));

        if (productPriceItemResponse.PriceIndexes.OzonIndexData.MinPrice <= 0)
            throw new DivideByZeroException("Минимальная цена Ozon равна 0 или меньше — деление невозможно");

        if (constantIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(constantIndex), "Коэффициент должен быть больше 0");

        if (percent <= 0)
            throw new ArgumentOutOfRangeException(nameof(percent), "Процент должен быть больше 0");

        double truePrice = GetTruePrice(productPriceItemResponse.Price.MarketingPrice, constantIndex);
        double index = truePrice / productPriceItemResponse.PriceIndexes.OzonIndexData.MinPrice;

        if (index == 0)
            throw new DivideByZeroException("Расчитанный индекс равен 0 — деление невозможно");
        
        double price = productPriceItemResponse.Price.MarketingSellerPrice / index;
        if (price <= 0)
            throw new InvalidOperationException("Рассчитанная базовая цена получилась меньше или равна 0");

        return ApplyPercent(price, percent);
    }
    public double GetTruePrice(double MarketingPrice, double ConstantIndex)
    {
        return MarketingPrice * Math.Round(ConstantIndex, 2);
    }
    private double ApplyPercent(double price, double percent)
    {
        return price * (1 - percent / 100);
    }
    
    private async Task<ProductImportPricesResponse> SetUserPrice(
        ProductPriceItemResponse productPriceItemResponse,
        OzonClient ozonClient,
        double YourPrice, double OldPrice,
        double MinPrice, double CostPrice)
    {
        if (_ozonApiDataManager == null)
        {
            _ozonApiDataManager = new OzonApiDataManager(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
        }
        else
        {
            _ozonApiDataManager.SetClient(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
        }
        var request = new List<ProductPriceItem>
        {
            new ProductPriceItem
            {
                OfferId = productPriceItemResponse.OfferId,
                ProductId = productPriceItemResponse.ProductId,
                NetPrice = Math.Round(CostPrice, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture),
                MinPrice = Math.Round(MinPrice, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture),
                Price = Math.Round(YourPrice, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture),
                OldPrice = Math.Round(OldPrice, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture),
                Vat = productPriceItemResponse.Price.Vat.ToString(CultureInfo.InvariantCulture),
            }
        };

        var response = await _ozonApiDataManager.ImportProductPrices(request);
        return response;
    }

    private async Task<ProductImportPricesResponse> SetNewPrice(
    ProductPriceItemResponse productPriceItemResponse,
    double newPrice,
    OzonClient ozonClient)
    {
        if (_ozonApiDataManager == null)
        {
            _ozonApiDataManager = new OzonApiDataManager(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
        }
        else
        {
            _ozonApiDataManager.SetClient(ozonClient.DecryptClientId, ozonClient.DecryptApiKey);
        }

        var roundedNewPrice = Math.Round(newPrice, MidpointRounding.AwayFromZero);
        var oldPrice = productPriceItemResponse.Price.MarketingSellerPrice;

        if (oldPrice > 0)
        {
            double minDifference;
            if (roundedNewPrice < 400)
                minDifference = 20;
            else if (roundedNewPrice <= 10000)
                minDifference = roundedNewPrice * 0.05;
            else
                minDifference = 500;

            if (oldPrice - roundedNewPrice < minDifference)
            {
                oldPrice = roundedNewPrice + minDifference;
            }
        }


        var request = new List<ProductPriceItem>
        {
            new ProductPriceItem
            {
                OfferId = productPriceItemResponse.OfferId,
                ProductId = productPriceItemResponse.ProductId,
                NetPrice = Math.Round(productPriceItemResponse.Price.NetPrice, MidpointRounding.AwayFromZero)
                    .ToString(CultureInfo.InvariantCulture),
                MinPrice = roundedNewPrice.ToString(CultureInfo.InvariantCulture),
                Price = roundedNewPrice.ToString(CultureInfo.InvariantCulture),
                Vat = productPriceItemResponse.Price.Vat.ToString(CultureInfo.InvariantCulture),
            }
        };

        var response = await _ozonApiDataManager.ImportProductPrices(request);
        return response;
    }

}