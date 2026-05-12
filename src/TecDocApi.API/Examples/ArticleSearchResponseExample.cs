using Swashbuckle.AspNetCore.Filters;
using TecDocApi.API.DTOs;

namespace TecDocApi.API.Examples;

/// <summary>
/// Пример ответа для поиска артикулов
/// </summary>
public class ArticleSearchResponseExample : IExamplesProvider<ArticleSearchResponseDto>
{
    public ArticleSearchResponseDto GetExamples()
    {
        return new ArticleSearchResponseDto
        {
            Count = 1,
            Results =
            [
                new()
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
                        new()
                        {
                            PictureName = "7_12400G.BMP",
                            Description = "Рисунок",
                            AdditionalDescription = "",
                            DocumentName = "",
                            DocumentType = "Picture",
                            ShowImmediately = true
                        },
                        new()
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
                        new()
                        {
                            LinkageTypeId = "PassengerCar",
                            LinkageId = 8971
                        }
                    },
                    EanCodes = new List<EanCodeDto>(),
                    Information = new List<InformationDto>
                    {
                        new()
                        {
                            InformationTypeKey = "2",
                            InformationType = "Общая информация",
                            InformationText = "EBERSPÄCHER-VERSION\n"
                        },
                        new()
                        {
                            InformationTypeKey = "2",
                            InformationType = "Общая информация",
                            InformationText = "Eberspächer-Katalysator: siehe unter Suchbaum \"Katalysator\".\n"
                        }
                    },
                    Accessories = new List<AccessoryDto>(),
                    NewNumbers = new List<NewNumberDto>()
                }
            ]
        };
    }
}

