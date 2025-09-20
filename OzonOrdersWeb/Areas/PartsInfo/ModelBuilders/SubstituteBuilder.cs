using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using OzonOrdersWeb.Areas.PartsInfo.Models.Substitute;
using Attribute = System.Attribute;

namespace OzonOrdersWeb.Areas.PartsInfo.ModelBuilders;

public class SubstituteBuilder
{
    private readonly IMemoryCache _memoryCache;
    
    public SubstituteBuilder(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    public IEnumerable<VehicleModel> BuildModel(JsonDocument json, string code, string supplier)
    {
        var cacheKey = $"SubstituteModelBuilder_{code}_{supplier}";

        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<VehicleModel> cachedResult))
        {
            return cachedResult; // Возвращаем кэшированный результат
        }

        var models = new List<VehicleModel>();
        var modelsElement = json.RootElement.GetProperty("Models");
        
        foreach (var modelElement in modelsElement.EnumerateArray())
        {
            if (!modelElement.TryGetProperty("ModelName", out var modelNameProp) || 
                !modelElement.TryGetProperty("ModelId", out var modelIdProp))
                continue;

            var vehicleModel = new VehicleModel
            {
                ModelName = modelNameProp.GetString() ?? "",
                ModelId = modelIdProp.GetInt32(),
                Substitutes = new List<Substitute>()
            };

            if (modelElement.TryGetProperty("Substitutes", out var substitutesProp))
            {
                foreach (var substituteElement in substitutesProp.EnumerateArray())
                {
                    var substitute = new Substitute
                    {
                        Type = substituteElement.TryGetProperty("Type", out var typeProp) ? typeProp.GetString() ?? "" : "",
                        Name = substituteElement.TryGetProperty("Name", out var nameProp) ? nameProp.GetString() ?? "" : "",
                        ModelId = substituteElement.TryGetProperty("ModelId", out var subModelIdProp) ? subModelIdProp.GetInt32() : 0,
                        Modification = new Modification(),
                        Attributes = new List<SubAttribute>()
                    };

                    if (substituteElement.TryGetProperty("Modification", out var modificationProp))
                    {
                        substitute.Modification = new Modification
                        {
                            Description = modificationProp.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "",
                            ConstructionInterval = modificationProp.TryGetProperty("construction_interval", out var intervalProp) ? intervalProp.GetString() ?? "" : ""
                        };
                    }

                    if (substituteElement.TryGetProperty("Attributes", out var attributesProp))
                    {
                        foreach (var attributeElement in attributesProp.EnumerateArray())
                        {
                            substitute.Attributes.Add(new SubAttribute
                            {
                                Title = attributeElement.TryGetProperty("Title", out var titleProp) ? titleProp.GetString() ?? "" : "",
                                Value = attributeElement.TryGetProperty("Value", out var valueProp) ? valueProp.GetString() ?? "" : ""
                            });
                        }
                    }

                    vehicleModel.Substitutes.Add(substitute);
                }
            }

            models.Add(vehicleModel);
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(30));

        _memoryCache.Set(cacheKey, models, cacheEntryOptions);

        return models;
    }
}