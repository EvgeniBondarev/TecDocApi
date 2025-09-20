using Newtonsoft.Json.Linq;
using OzonDomains.TecDocModels;
using Servcies.ApiServcies.TecDocApi.Models;

namespace Servcies.ParserServcies
{
    public class FullDetailInfoCaster
    {
        public async Task<FullDetailInfoSchema> CastToFullDetailInfoSchema(JObject json)
        {
            var result = new FullDetailInfoSchema
            {
                NormalizedArticle = json["normalized_article"]?.ToString(),
                DetailAttribute = json["detail_attribute"]?.ToObject<List<ArticleAttributesSchema>>() ?? new List<ArticleAttributesSchema>(),
                ImgUrls = json["img_urls"]?.ToObject<List<string>>() ?? new List<string>(),
                SupplierFromJc = json["supplier_from_jc"]?.ToObject<EtProducerSchema>(),
                SupplierFromTd = json["supplier_from_td"]?.ToObject<SuppliersSchema>()
            };

            result.SupplierFromJc ??= null;
            result.SupplierFromTd ??= null;

            if (result.ImgUrls.Count == 0) 
            {
                result.ImgUrls = Enumerable.Repeat("https://s3.timeweb.cloud/25f554fc-6f66254e-9650-4d17-8e13-77b5b7d3242e/AppData/Studio2/IMG/no-image.svg", 6).ToList();
            }

            return await Task.FromResult(result); 
        }
    }
}
