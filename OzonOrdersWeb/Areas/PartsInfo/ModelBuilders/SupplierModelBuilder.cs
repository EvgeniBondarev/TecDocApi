using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models;
using PartsInfo.HttpUtils;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class SupplierModelBuilder
{
    private readonly IMemoryCache _memoryCache;
    private readonly ProxyHttpClientService _proxyHttpClientService;
    private readonly PartsFinderArticleModelBuilder _prBuilder;
    
    public SupplierModelBuilder(IMemoryCache memoryCache, 
                                ProxyHttpClientService proxyHttpClientService,
                                PartsFinderArticleModelBuilder prBuilder)
    {
        _memoryCache = memoryCache;
        _proxyHttpClientService = proxyHttpClientService;
        _prBuilder = prBuilder;
    }

    public async Task<List<SupplierModel>> BuildModel(string code, JsonDocument document)
    {
        var cacheKey = $"BuildSupplierModel_{code}";

        if (_memoryCache.TryGetValue(cacheKey, out List<SupplierModel> cachedResult))
        {
            return cachedResult;
        }

        var root = document.RootElement;

        var tdIds = new HashSet<int>();
        var usedRealIds = new HashSet<int>();
        var ratings = new Dictionary<int, int>();
        var jsIds = new Dictionary<int, int>();

        var result = new List<SupplierModel>();

        if (root.TryGetProperty("suppliersFromJs", out var jsArray))
        {
            CollectJsRatings(jsArray, tdIds, usedRealIds, ratings, jsIds);
        }
        if (root.TryGetProperty("suppliersFromTd", out var tdArray))
        {
            result.AddRange(BuildFromTd(tdArray, tdIds, ratings, jsIds));
        }
        if (root.TryGetProperty("suppliersFromJs", out jsArray))
        {
            result.AddRange(BuildRemainingJs(jsArray, tdIds, usedRealIds));
        }

        var finalResult = await SetAddition(result, _proxyHttpClientService, code);

        foreach (var item in finalResult)
        {
            if (item.JSSupplierId != 0)
            {
                item.PrData = await _prBuilder.Build(await _proxyHttpClientService.GetJsonAsync(
                    $"https://api.interparts.ru/pr-part/by-jc-id?jc_id={item.JSSupplierId}&article={code}"),
                    code, item.JSSupplierId);
            }
            if (item.TecdocSupplierId != 0 && item.PrData == null)
            {
                item.PrData = await _prBuilder.Build(await _proxyHttpClientService.GetJsonAsync(
                    $"https://api.interparts.ru/pr-part/by-td-id?td_id={item.TecdocSupplierId}&article={code}"),
                    code, item.TecdocSupplierId);
            }
        }
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        _memoryCache.Set(cacheKey, finalResult, cacheEntryOptions);

        return finalResult;
    }


    private void CollectJsRatings(JsonElement jsArray, 
        HashSet<int> tdIds, 
        HashSet<int> usedRealIds,
        Dictionary<int, int> ratings,
        Dictionary<int, int> jsIds)
    {
        foreach (var item in jsArray.EnumerateArray())
        {
            if (!item.TryGetProperty("tecdocSupplierId", out var tecdocIdProp) ||
                !item.TryGetProperty("realid", out var realIdProp) ||
                !item.TryGetProperty("id", out var idProp) ||
                !item.TryGetProperty("rating", out var ratingProp)) continue;

            int tecdocId = tecdocIdProp.GetInt32();
            int realId = realIdProp.GetInt32();
            int id = idProp.GetInt32();
            int rating = ratingProp.GetInt32();

            // Только для совпадающих id и realid
            if (id == realId && tecdocId != 0)
            {
                ratings[tecdocId] = rating;
                jsIds[tecdocId] = id;
            }
        }
    }

    private IEnumerable<SupplierModel> BuildFromTd(JsonElement tdArray, 
        HashSet<int> tdIds, 
        Dictionary<int, int> ratings, 
        Dictionary<int, int> jsIds)
    {
        foreach (var item in tdArray.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idElement)) continue;

            int id = idElement.GetInt32();
            tdIds.Add(id);

            ratings.TryGetValue(id, out int rating);
            jsIds.TryGetValue(id, out var jsId);

            yield return new SupplierModel
            {
                Name = item.TryGetProperty("matchcode", out var nameProp) ? nameProp.GetString() ?? "" : "",
                ImageUrl = item.TryGetProperty("img", out var imgProp) ? imgProp.GetString() ?? "" : "",
                Rating = rating,
                TecdocSupplierId = item.TryGetProperty("id", out var tdSupplierIdProp) && tdSupplierIdProp.TryGetInt32(out var tecdocSupplierId)
                    ? tecdocSupplierId
                    : 0,
                JSSupplierId = jsId,
            };
        }
    }

    private IEnumerable<SupplierModel> BuildRemainingJs(JsonElement jsArray, HashSet<int> tdIds, HashSet<int> usedRealIds)
    {
        foreach (var item in jsArray.EnumerateArray())
        {
            if (!item.TryGetProperty("tecdocSupplierId", out var tecdocIdProp) ||
                !item.TryGetProperty("realid", out var realIdProp) ||
                !item.TryGetProperty("id", out var idProp)) continue;

            int tecdocId = tecdocIdProp.GetInt32();
            int realId = realIdProp.GetInt32();
            int id = idProp.GetInt32();
            if (tecdocId != 0 && tdIds.Contains(tecdocId))
                continue;
            if (usedRealIds.Contains(realId) && id != realId)
                continue;

            if (id == realId)
                usedRealIds.Add(realId);
            else if (!usedRealIds.Contains(realId))
                continue;

            yield return new SupplierModel
            {
                Name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "",
                ImageUrl = item.TryGetProperty("img", out var imgProp) ? imgProp.GetString() ?? "" : "",
                Rating = item.TryGetProperty("rating", out var ratingProp) ? ratingProp.GetInt32() : 0,
                TecdocSupplierId = item.TryGetProperty("tecdocSupplierId", out var tdSupplierIdProp) && tdSupplierIdProp.TryGetInt32(out var tecdocSupplierId)
                ? tecdocSupplierId
                : 0, 
                JSSupplierId = item.TryGetProperty("id", out var jSSupplierIdProp) && jSSupplierIdProp.TryGetInt32(out var jSSupplierId)
                    ? jSSupplierId
                    : 0,
            };
        }
    }

    private async Task<List<SupplierModel>> SetAddition(
    List<SupplierModel> supplierModels,
    ProxyHttpClientService proxyHttpClientService,
    string code)
    {
        foreach (var supplierModel in supplierModels)
        {
            if (string.IsNullOrWhiteSpace(supplierModel.Description) && supplierModel.TecdocSupplierId != 0)
            {
                await SetDescriptionFromVolna(supplierModel, proxyHttpClientService, code);
            }
            await SetImageFromAvailableSources(supplierModel, proxyHttpClientService, code);

        }

        return supplierModels.OrderByDescending(m => m.Description).ToList();
    }

    private async Task SetDescriptionFromVolna(SupplierModel supplierModel, 
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
                    supplierModel.Description = description;
                }
            }
        }
    }

    private async Task SetImageFromAvailableSources(SupplierModel supplierModel, 
        ProxyHttpClientService proxyHttpClientService, string code)
    {
        string imageUrl = null;
        var imageSource = ImageSource.Unknown;

        // 1. Попытка получить картинку из s3 (если есть JSSupplierId)
        if (supplierModel.JSSupplierId != 0)
        {
            imageUrl = await GetS3ImageUrl(code, supplierModel.Name);
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                supplierModel.ImageUrl = imageUrl;
                supplierModel.ImageSource = ImageSource.S3;
                return;
            }
        }
        
        // Устанавливаем изображение только если оно еще не заполнено
        if (!string.IsNullOrWhiteSpace(supplierModel.ImageUrl))
        {
            supplierModel.ImageSource = ImageSource.Database;
            return;
        }

        // 2. Попытка получить картинку из volna (если есть TecdocSupplierId)
        if (supplierModel.TecdocSupplierId != 0)
        {
            imageUrl = await GetImageFromVolna(supplierModel.TecdocSupplierId, proxyHttpClientService, code);
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                supplierModel.ImageUrl = imageUrl;
                supplierModel.ImageSource = ImageSource.VolnaParser;
                return;
            }
        }
        // 4. Если картинка всё ещё не определена — ставим no-image
        supplierModel.ImageUrl = "https://s3.timeweb.cloud/25f554fc-6f66254e-9650-4d17-8e13-77b5b7d3242e/AppData/Studio2/IMG/no-image.svg";
        supplierModel.ImageSource = ImageSource.Unknown;
    }
    
    private async Task<string> GetS3ImageUrl(string article, string producer)
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
                return string.Empty;

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

            if (imageUrls.Count > 0)
            {
                return imageUrls[0];
            }
            return string.Empty;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get S3 images: {ex.Message}");
            return string.Empty;
        }
    }

    private async Task<string> GetImageFromVolna(int tecdocSupplierId, 
        ProxyHttpClientService proxyHttpClientService, string code)
    {
        var volnaData = await proxyHttpClientService.GetJsonAsync(
            $"https://api.interparts.ru/volna-parts/part/{code}/{tecdocSupplierId}");

        if (volnaData?.RootElement.ValueKind == JsonValueKind.Array && 
            volnaData.RootElement.GetArrayLength() > 0)
        {
            var item = volnaData.RootElement[0];
            if (item.TryGetProperty("images", out var imagesProp) &&
                imagesProp.ValueKind == JsonValueKind.Array &&
                imagesProp.GetArrayLength() > 0)
            {
                var img = imagesProp[0].GetString();
                if (!string.IsNullOrEmpty(img) && !img.EndsWith("no-image__photo.svg"))
                {
                    return img;
                }
            }
        }
        return null;
    }

    private async Task<string> GetImageFromInternet(SupplierModel supplierModel, 
        ProxyHttpClientService proxyHttpClientService, string code)
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
        
        if (duckDuckData?.RootElement.ValueKind == JsonValueKind.Array &&
            duckDuckData.RootElement.GetArrayLength() > 0)
        {
            foreach (var urlElement in duckDuckData.RootElement.EnumerateArray())
            {
                var img = urlElement.GetString();
                if (!string.IsNullOrWhiteSpace(img) && !img.EndsWith("no-image__photo.svg"))
                {
                    return img;
                }
            }
        }
        return null;
    }
}
