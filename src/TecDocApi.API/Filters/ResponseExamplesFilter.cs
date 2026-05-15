using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
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
            operation.Responses != null &&
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

            if (response200.Content != null)
            {
                foreach (var content in response200.Content.Values)
                {
                    try
                    {
                        content.Example = JsonNode.Parse(jsonExample);
                    }
                    catch
                    {
                        content.Example = JsonValue.Create(jsonExample);
                    }
                }
            }
        }
    }
}

