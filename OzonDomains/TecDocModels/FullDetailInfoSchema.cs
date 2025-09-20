using Servcies.ApiServcies.TecDocApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.TecDocModels
{
    public class FullDetailInfoSchema
    {
        public string? NormalizedArticle { get; set; }
        public List<ArticleAttributesSchema> DetailAttribute { get; set; }
        public List<string> ImgUrls { get; set; }
        public EtProducerSchema? SupplierFromJc { get; set; }
        public SuppliersSchema? SupplierFromTd { get; set; }
    }
}
