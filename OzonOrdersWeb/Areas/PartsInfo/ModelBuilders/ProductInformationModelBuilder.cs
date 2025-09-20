using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models.ProductInformation;

public class ProductInformationModelBuilder
{
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ProductInformationModelBuilder> _logger;

        public ProductInformationModelBuilder(
            IMemoryCache memoryCache,
            ILogger<ProductInformationModelBuilder> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<ProductInformationModel> Build(JsonDocument document, string articleNumber, string manufacturer)
        {
            var cacheKey = $"ProductInfo_{articleNumber}_{manufacturer}";

            if (_memoryCache.TryGetValue(cacheKey, out ProductInformationModel cachedModel))
            {
                return cachedModel;
            }

            var model = new ProductInformationModel();

            if (document?.RootElement.ValueKind == JsonValueKind.Object)
            {
                var root = document.RootElement;

                // Основные строковые свойства
                model.TOW_KOD = GetStringSafe(root, "TOW_KOD");
                model.IC_INDEX = GetStringSafe(root, "IC_INDEX");
                model.TEC_DOC = GetStringSafe(root, "TEC_DOC");
                model.ARTICLE_NUMBER = GetStringSafe(root, "ARTICLE_NUMBER");
                model.MANUFACTURER = GetStringSafe(root, "MANUFACTURER");
                model.SHORT_DESCRIPTION = GetStringSafe(root, "SHORT_DESCRIPTION");
                model.DESCRIPTION = GetStringSafe(root, "DESCRIPTION");
                model.BARCODES = GetStringSafe(root, "BARCODES");
                model.CUSTOM_CODE = GetStringSafe(root, "CUSTOM_CODE");

                // Числовые свойства
                model.TEC_DOC_PROD = GetIntSafe(root, "TEC_DOC_PROD");

                // Десятичные числа с обработкой разных форматов
                model.PACKAGE_WEIGHT = ParseDecimalSafe(GetStringSafe(root, "PACKAGE_WEIGHT"));
                model.PACKAGE_LENGTH = ParseDecimalSafe(GetStringSafe(root, "PACKAGE_LENGTH"));
                model.PACKAGE_WIDTH = ParseDecimalSafe(GetStringSafe(root, "PACKAGE_WIDTH"));
                model.PACKAGE_HEIGHT = ParseDecimalSafe(GetStringSafe(root, "PACKAGE_HEIGHT"));
            }

            _memoryCache.Set(cacheKey, model, TimeSpan.FromMinutes(30));
            return await Task.FromResult(model);
        }

        private string GetStringSafe(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) 
                ? prop.GetString() 
                : null;
        }

        private int GetIntSafe(JsonElement element, string propertyName, int defaultValue = 0)
        {
            return element.TryGetProperty(propertyName, out var prop) 
                ? prop.GetInt32() 
                : defaultValue;
        }

        private decimal? ParseDecimalSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Заменяем запятые на точки для корректного парсинга
            var normalizedValue = value.Replace(",", ".");

            if (decimal.TryParse(normalizedValue, 
                NumberStyles.Any, 
                CultureInfo.InvariantCulture, 
                out decimal result))
            {
                return result;
            }

            _logger?.LogWarning($"Failed to parse decimal value: {value}");
            return null; 
        }
}