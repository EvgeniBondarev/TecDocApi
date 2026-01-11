using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
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
                Results = new List<ArticleDto>
                {
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
                        Crosses = new List<CrossDto>(),
                        OeNumbers = new List<OeNumberDto>(),
                        Attributes = new List<AttributeDto>(),
                        Images = new List<ImageDto>
                        {
                            new ImageDto
                            {
                                PictureName = "7_12400G.BMP",
                                Description = "Рисунок",
                                AdditionalDescription = "",
                                DocumentName = "",
                                DocumentType = "Picture",
                                ShowImmediately = true
                            }
                        },
                        Linkages = new List<LinkageDto>
                        {
                            new LinkageDto
                            {
                                LinkageTypeId = "PassengerCar",
                                LinkageId = 8971
                            }
                        },
                        EanCodes = new List<EanCodeDto>(),
                        Information = new List<InformationDto>
                        {
                            new InformationDto
                            {
                                InformationTypeKey = "2",
                                InformationType = "Общая информация",
                                InformationText = "EBERSPÄCHER-VERSION\n"
                            }
                        },
                        Accessories = new List<AccessoryDto>(),
                        NewNumbers = new List<NewNumberDto>()
                    }
                }
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
                    using var doc = System.Text.Json.JsonDocument.Parse(jsonExample);
                    content.Example = ConvertToOpenApiAny(doc.RootElement);
                }
                catch
                {
                    content.Example = new Microsoft.OpenApi.Any.OpenApiString(jsonExample);
                }
            }
        }
    }

    private static Microsoft.OpenApi.Any.IOpenApiAny ConvertToOpenApiAny(System.Text.Json.JsonElement element)
    {
        return element.ValueKind switch
        {
            System.Text.Json.JsonValueKind.Object => ConvertObject(element),
            System.Text.Json.JsonValueKind.Array => ConvertArray(element),
            System.Text.Json.JsonValueKind.String => new Microsoft.OpenApi.Any.OpenApiString(element.GetString() ?? string.Empty),
            System.Text.Json.JsonValueKind.Number => element.TryGetInt64(out var intValue) 
                ? new Microsoft.OpenApi.Any.OpenApiInteger((int)intValue)
                : new Microsoft.OpenApi.Any.OpenApiDouble(element.GetDouble()),
            System.Text.Json.JsonValueKind.True => new Microsoft.OpenApi.Any.OpenApiBoolean(true),
            System.Text.Json.JsonValueKind.False => new Microsoft.OpenApi.Any.OpenApiBoolean(false),
            System.Text.Json.JsonValueKind.Null => new Microsoft.OpenApi.Any.OpenApiNull(),
            _ => new Microsoft.OpenApi.Any.OpenApiString(element.ToString())
        };
    }

    private static Microsoft.OpenApi.Any.OpenApiObject ConvertObject(System.Text.Json.JsonElement element)
    {
        var obj = new Microsoft.OpenApi.Any.OpenApiObject();
        foreach (var prop in element.EnumerateObject())
        {
            obj[prop.Name] = ConvertToOpenApiAny(prop.Value);
        }
        return obj;
    }

    private static Microsoft.OpenApi.Any.OpenApiArray ConvertArray(System.Text.Json.JsonElement element)
    {
        var array = new Microsoft.OpenApi.Any.OpenApiArray();
        foreach (var item in element.EnumerateArray())
        {
            array.Add(ConvertToOpenApiAny(item));
        }
        return array;
    }
}

