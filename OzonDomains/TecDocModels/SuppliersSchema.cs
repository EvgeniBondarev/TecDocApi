using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servcies.ApiServcies.TecDocApi.Models
{
    public class SuppliersSchema
    {
        public int Id { get; set; }
        public int? DataVersion { get; set; }
        public string? Description { get; set; }
        public string? Matchcode { get; set; }
        public int? NumberOfArticles { get; set; }
        public string? HasNewVersionArticles { get; set; }
    }
}
