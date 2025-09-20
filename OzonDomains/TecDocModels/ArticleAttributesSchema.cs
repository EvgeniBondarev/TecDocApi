using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servcies.ApiServcies.TecDocApi.Models
{
    public class ArticleAttributesSchema
    {
        public int SupplierId { get; set; }
        public string DataSupplierArticleNumber { get; set; }
        public int Id { get; set; }
        public string? Description { get; set; }
        public string DisplayTitle { get; set; }
        public string DisplayValue { get; set; }
    }
}
