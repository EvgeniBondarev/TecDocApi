using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TecDocApi.API.DTOs;

namespace TecDocApi.API.Filters;

/// <summary>
/// Фильтр для добавления примеров ответов в Swagger
/// </summary>
public class ResponseExamplesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name == "SearchByArticle" && 
            operation.Responses.TryGetValue("200", out var response200))
        {
            var example = new ArticleSearchResponseDto
            {
                Count = 1,
                Results =
                [
                    new ArticleDto
                    {
                        Article = new ArticleInfoDto
                        {
                            SupplierId = 7,
                            DataSupplierArticleNumber = "12_23",
                            FoundString = "1223                               ",
                            NormalizedDescription = "Глушитель",
                            Description = "",
                            ArticleStateDisplayValue = "псевдо-изделие",
                            QuantityPerPackingUnit = 1,
                            Flags = new ArticleFlagsDto
                            {
                                FlagAccessory = false,
                                FlagMaterialCertification = false,
                                FlagRemanufactured = false,
                                FlagSelfServicePacking = false,
                                HasAxle = false,
                                HasCommercialVehicle = false,
                                HasEngine = false,
                                HasLinkItems = true,
                                HasMotorbike = false,
                                HasPassengerCar = true,
                                IsValid = true
                            }
                        },
                        Supplier = new SupplierInfoDto
                        {
                            Id = 7,
                            Description = "EBERSPÄCHER",
                            Matchcode = "EBERSPÄCHER",
                            DataVersion = "1117",
                            NbrOfArticles = 2633666
                        },
                        Crosses = [],
                        OeNumbers = [],
                        Attributes = [],
                        Images =
                        [
                            new()
                            {
                                PictureName = "7_12400G.BMP",
                                Description = "Рисунок",
                                AdditionalDescription = "",
                                DocumentName = "",
                                DocumentType = "Picture",
                                ShowImmediately = true
                            }
                        ],
                        Linkages =
                        [
                            new()
                            {
                                LinkageTypeId = "PassengerCar",
                                LinkageId = 8971
                            }
                        ],
                        EanCodes = [],
                        Information =
                        [
                            new InformationDto
                            {
                                InformationTypeKey = "2",
                                InformationType = "Общая информация",
                                InformationText = "EBERSPÄCHER-VERSION\n"
                            }
                        ],
                        Accessories = [],
                        NewNumbers = []
                    }
                ]
            };

            var jsonExample = JsonSerializer.Serialize(example, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            response200.Content ??= new Dictionary<string, OpenApiMediaType>();
            foreach (var content in response200.Content.Values)
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonExample);
                    content.Example = ConvertToOpenApiAny(doc.RootElement);
                }
                catch
                {
                    content.Example = new OpenApiString(jsonExample);
                }
            }
        }
    }

    private static IOpenApiAny ConvertToOpenApiAny(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertObject(element),
            JsonValueKind.Array => ConvertArray(element),
            JsonValueKind.String => new OpenApiString(element.GetString() ?? string.Empty),
            JsonValueKind.Number => element.TryGetInt64(out var intValue) 
                ? new OpenApiInteger((int)intValue)
                : new OpenApiDouble(element.GetDouble()),
            JsonValueKind.True => new OpenApiBoolean(true),
            JsonValueKind.False => new OpenApiBoolean(false),
            JsonValueKind.Null => new OpenApiNull(),
            _ => new OpenApiString(element.ToString())
        };
    }

    private static OpenApiObject ConvertObject(JsonElement element)
    {
        var obj = new OpenApiObject();
        foreach (var prop in element.EnumerateObject())
        {
            obj[prop.Name] = ConvertToOpenApiAny(prop.Value);
        }
        return obj;
    }

    private static OpenApiArray ConvertArray(JsonElement element)
    {
        var array = new OpenApiArray();
        foreach (var item in element.EnumerateArray())
        {
            array.Add(ConvertToOpenApiAny(item));
        }
        return array;
    }
}

