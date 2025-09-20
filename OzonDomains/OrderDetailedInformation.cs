using OzonDomains.TecDocModels;
using Servcies.ApiServcies.InterpartsApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains
{
    public class OrderDetailedInformation
    {
        public string Description { get; set; }
        public string Article {  get; set; }
        public string Supplier { get; set; }
        public FullDetailInfoSchema FullDetailInfoSchema { get; set; }
        public List<SupplierDetailedInformation> SuppliersDetailedInformation { get; set; }
    }
}
