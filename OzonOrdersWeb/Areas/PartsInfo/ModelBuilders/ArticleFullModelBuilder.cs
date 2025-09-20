using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models;
using OzonOrdersWeb.Areas.PartsInfo.Models.FullInfo;
using PartsInfo.HttpUtils;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class ArticleFullModelBuilder
{
    private readonly IMemoryCache _memoryCache;
    private readonly ProxyHttpClientService _proxyHttpClientService;
    private readonly SupplierModelBuilder _supplierModelBuilder;
    private readonly PartsFinderArticleModelBuilder _prBuilder;
    
    public ArticleFullModelBuilder(IMemoryCache memoryCache, 
        ProxyHttpClientService proxyHttpClientService,
        PartsFinderArticleModelBuilder prBuilder)
    {
        _memoryCache = memoryCache;
        _proxyHttpClientService = proxyHttpClientService;
        _prBuilder = prBuilder;
    }
    
    public async Task<ArticleFullModel> BuildModel(JsonDocument document, string code, string supplier)
    {
        var cacheKey = $"BuildArticleFullModelBuilder_{code}_{supplier}";

        if (_memoryCache.TryGetValue(cacheKey, out ArticleFullModel cachedResult))
        {
            return cachedResult;
        }
        
        var root = document.RootElement;

        var model = new ArticleFullModel
        {
            NormalizedArticle = root.GetProperty("normalized_article").GetString(),

            ArticleEan = root.TryGetProperty("article_ean", out var articleEanEl) && articleEanEl.ValueKind != JsonValueKind.Null
                ? BuildArticleEan(articleEanEl)
                : null,

            ArticleSchema = root.TryGetProperty("article_schema", out var schemaEl) && schemaEl.ValueKind != JsonValueKind.Null
                ? BuildArticleSchema(schemaEl)
                : null,

            DetailAttributes = root.TryGetProperty("detail_attribute", out var attrs) && attrs.ValueKind == JsonValueKind.Array
                ? attrs.EnumerateArray().Select(BuildDetailAttribute).ToList()
                : new List<DetailAttributeModel>(),

            ImgUrls = root.TryGetProperty("img_urls", out var imgs) && imgs.ValueKind == JsonValueKind.Array
                ? imgs.EnumerateArray().Select(i => i.GetString()).Where(i => i != null).ToList()
                : new List<string>(),

            Supplier = BuildCombinedSupplier(
                root.TryGetProperty("supplier_from_jc", out var supplierJs) && supplierJs.ValueKind != JsonValueKind.Null ? supplierJs : (JsonElement?)null,
                root.TryGetProperty("supplier_from_td", out var supplierTd) && supplierTd.ValueKind != JsonValueKind.Null ? supplierTd : (JsonElement?)null
            ),
        };

        model.ImgUrls.AddRange(await SetImageFromAvailableSources(model.Supplier, _proxyHttpClientService, code));
        

        if (model.Supplier.TecdocSupplierId != 0)
        {
            model.Description = await SetDescriptionFromVolna(model.Supplier, _proxyHttpClientService, code);
        }
        
        if (model.Supplier.JSSupplierId != 0)
        {
            model.PrData = await _prBuilder.Build(await _proxyHttpClientService.GetJsonAsync(
                    $"https://api.interparts.ru/pr-part/by-jc-id?jc_id={model.Supplier?.JSSupplierId}&article={code}"),
                code, model.Supplier.JSSupplierId);
        }
        if (model.Supplier.TecdocSupplierId != 0 && model.Supplier.PrData == null)
        {
            model.PrData = await _prBuilder.Build(await _proxyHttpClientService.GetJsonAsync(
                    $"https://api.interparts.ru/pr-part/by-td-id?td_id={model.Supplier?.TecdocSupplierId}&article={code}"),
                code, model.Supplier.TecdocSupplierId);
        }
        if (model.PrData?.ImgUrls != null)
        {
            model.ImgUrls.AddRange(model.PrData.ImgUrls);
        }
        model.ImgUrls = model.ImgUrls.Distinct().ToList();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        _memoryCache.Set(cacheKey, model, cacheEntryOptions);
        
        return model;
    }

    private ArticleEanModel BuildArticleEan(JsonElement el) => new()
    {
        SupplierId = el.GetProperty("supplierid").GetInt32(),
        DataSupplierArticleNumber = el.GetProperty("datasupplierarticlenumber").GetString(),
        Ean = el.GetProperty("ean").GetString()
    };

    private ArticleSchemaModel BuildArticleSchema(JsonElement el) => new()
    {
        SupplierId = el.GetProperty("supplierId").GetInt32(),
        DataSupplierArticleNumber = el.GetProperty("DataSupplierArticleNumber").GetString(),
        ArticleStateDisplayValue = el.GetProperty("ArticleStateDisplayValue").GetString(),
        Description = el.GetProperty("Description").GetString(),
        FlagAccessory = el.GetProperty("FlagAccessory").GetBoolean(),
        FlagMaterialCertification = el.GetProperty("FlagMaterialCertification").GetBoolean(),
        FlagRemanufactured = el.GetProperty("FlagRemanufactured").GetBoolean(),
        FlagSelfServicePacking = el.GetProperty("FlagSelfServicePacking").GetBoolean(),
        FoundString = el.GetProperty("FoundString").GetString(),
        HasAxle = el.GetProperty("HasAxle").GetBoolean(),
        HasCommercialVehicle = el.GetProperty("HasCommercialVehicle").GetBoolean(),
        HasCVManuID = el.GetProperty("HasCVManuID").GetBoolean(),
        HasEngine = el.GetProperty("HasEngine").GetBoolean(),
        HasLinkitems = el.GetProperty("HasLinkitems").GetBoolean(),
        HasMotorbike = el.GetProperty("HasMotorbike").GetBoolean(),
        HasPassengerCar = el.GetProperty("HasPassengerCar").GetBoolean(),
        IsValid = el.GetProperty("IsValid").GetBoolean(),
        NormalizedDescription = el.GetProperty("NormalizedDescription").GetString(),
        PackingUnit = el.GetProperty("PackingUnit").GetInt32(),
        QuantityPerPackingUnit = el.GetProperty("QuantityPerPackingUnit").GetInt32()
    };

    private DetailAttributeModel BuildDetailAttribute(JsonElement el) => new()
    {
        SupplierId = el.GetProperty("supplierid").GetInt32(),
        DataSupplierArticleNumber = el.GetProperty("datasupplierarticlenumber").GetString(),
        Id = el.GetProperty("id").GetInt32(),
        Description = el.GetProperty("description").GetString(),
        DisplayTitle = el.GetProperty("displaytitle").GetString(),
        DisplayValue = el.GetProperty("displayvalue").GetString()
    };

    private SupplierModel BuildCombinedSupplier(JsonElement? jsElement, JsonElement? tdElement)
    {
        var supplier = new SupplierModel();

        if (jsElement.HasValue)
        {
            var js = jsElement.Value;
            supplier.Name = js.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
            supplier.Rating = js.TryGetProperty("rating", out var ratingProp) ? ratingProp.GetInt32() : 0;
            supplier.ImageUrl = js.TryGetProperty("img", out var imgProp) ? imgProp.GetString() ?? "" : "";
            supplier.TecdocSupplierId = js.TryGetProperty("tecdocSupplierId", out var tdProp) ? tdProp.GetInt32() : 0;
            supplier.JSSupplierId = js.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
            supplier.Code = js.TryGetProperty("marketPrefix", out var prefixProp) ? prefixProp.GetString() ?? "" : "";
        }

        if (tdElement.HasValue)
        {
            var td = tdElement.Value;

            if (string.IsNullOrWhiteSpace(supplier.Description))
            {
                supplier.Description = td.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "";
            }

            if (supplier.TecdocSupplierId == 0)
            {
                supplier.TecdocSupplierId = td.TryGetProperty("id", out var tdIdProp) ? tdIdProp.GetInt32() : 0;
            }

            if (string.IsNullOrWhiteSpace(supplier.ImageUrl))
            {
                supplier.ImageUrl = td.TryGetProperty("img", out var tdImgProp) ? tdImgProp.GetString() ?? "" : "";
            }
        }
        return supplier;
    }

     private async Task<List<string>> SetImageFromAvailableSources(SupplierModel supplierModel, 
        ProxyHttpClientService proxyHttpClientService, string code)
    {
        List<string> imageUrls = new List<string>();
        
        // 1. Попытка получить картинку из s3 (если есть JSSupplierId)
        if (supplierModel.JSSupplierId != 0)
        {
            var s3ImageUrls = await GetS3ImageUrls(code, supplierModel.Name);
            if (s3ImageUrls.Count > 0)
            {
                imageUrls.AddRange(s3ImageUrls);
            }
        }

        // 2. Попытка получить картинку из volna (если есть TecdocSupplierId)
        if (supplierModel.TecdocSupplierId != 0)
        {
            var volnaImageUrls = await GetImageFromVolna(supplierModel.TecdocSupplierId, proxyHttpClientService, code);
            if (volnaImageUrls.Count > 0)
            {
                imageUrls.AddRange(volnaImageUrls);
            }
        }

        if (imageUrls.Count == 0)
        {
            imageUrls.Add("https://s3.timeweb.cloud/25f554fc-6f66254e-9650-4d17-8e13-77b5b7d3242e/AppData/Studio2/IMG/300x200no-image.svg");
        }
        return imageUrls;
    }

    private async Task<List<string>> GetS3ImageUrls(string article, string producer)
    {
        try
        {
            var s3Url = "https://api.interparts.ru/s3/multifinderbrands";
                
            // Подготавливаем данные для запроса
            var requestData = new[]
            {
                new { brand = producer.Trim(), article = article.Trim() }
            };

            var response = await _proxyHttpClientService.PostJsonAsync(s3Url, requestData);

            if (response == null)
                return new List<string>();

            var root = response.RootElement;
            var imageUrls = new List<string>();

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("url", out JsonElement urlElement) && 
                        urlElement.ValueKind == JsonValueKind.String)
                    {
                        imageUrls.Add(urlElement.GetString());
                    }
                }
            }

            return imageUrls;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get S3 images: {ex.Message}");
            return new List<string>();
        }
    }
    
    


    private async Task<List<string>> GetImageFromVolna(int tecdocSupplierId, 
        ProxyHttpClientService proxyHttpClientService, string code)
    {
        var volnaData = await proxyHttpClientService.GetJsonAsync(
            $"https://api.interparts.ru/volna-parts/part/{code}/{tecdocSupplierId}");

        var item = volnaData.RootElement[0];
        var imageList = new List<string>();

        if (item.TryGetProperty("images", out var imagesProp) &&
            imagesProp.ValueKind == JsonValueKind.Array &&
            imagesProp.GetArrayLength() > 0)
        {
            foreach (var imageElement in imagesProp.EnumerateArray())
            {
                var img = imageElement.GetString();
                if (!string.IsNullOrEmpty(img) && !img.EndsWith("no-image__photo.svg"))
                {
                    imageList.Add(img);
                }
            }
        }

        return imageList;
    }

    private async Task<List<string>> GetImagesFromInternetAsync(
        SupplierModel supplierModel,
        ProxyHttpClientService proxyHttpClientService,
        string code)
    {
        var queryParts = new List<string>
        {
            "Car part",
            code,
            supplierModel.Name
        };

        if (!string.IsNullOrWhiteSpace(supplierModel.Description))
        {
            queryParts.Add(supplierModel.Description);
        }

        var query = string.Join("%20", queryParts.Where(p => !string.IsNullOrWhiteSpace(p)));
        var duckDuckUrl = $"https://api.interparts.ru/s3/duck-duck-go-search-images?query={query}&count=5";

        var duckDuckData = await proxyHttpClientService.GetJsonAsync(duckDuckUrl);
        var result = new List<string>();

        if (duckDuckData?.RootElement.ValueKind == JsonValueKind.Array &&
            duckDuckData.RootElement.GetArrayLength() > 0)
        {
            foreach (var urlElement in duckDuckData.RootElement.EnumerateArray())
            {
                var img = urlElement.GetString();
                if (!string.IsNullOrWhiteSpace(img) && !img.EndsWith("no-image__photo.svg"))
                {
                    result.Add(img);
                }
            }
        }
        return result.Count > 0 ? result : null;
    }
    
    private async Task<string> SetDescriptionFromVolna(SupplierModel supplierModel, 
        ProxyHttpClientService proxyHttpClientService, string code)
    {
        var volnaData = await proxyHttpClientService.GetJsonAsync(
            $"https://api.interparts.ru/volna-parts/part/{code}/{supplierModel.TecdocSupplierId}");

        if (volnaData?.RootElement.ValueKind == JsonValueKind.Array && 
            volnaData.RootElement.GetArrayLength() > 0)
        {
            var item = volnaData.RootElement[0];
            if (item.TryGetProperty("name", out var nameProp))
            {
                var description = nameProp.GetString();
                if (!string.IsNullOrWhiteSpace(description))
                {
                    return description;
                }
            }
        }
        return null;
    }
}