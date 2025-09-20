using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains
{
    public class DetailInformation
    {
        public string DetailID { get; set; }
        public string DetailExternalID { get; set; }
        public string Article { get; set; }
        public string ArticleDisplay { get; set; }
        public string BrandID { get; set; }
        public string BrandExternalID { get; set; }
        public string BrandName { get; set; }
        public string Description { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
    }

}
