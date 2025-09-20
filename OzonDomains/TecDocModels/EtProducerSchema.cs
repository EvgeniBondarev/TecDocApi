using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servcies.ApiServcies.TecDocApi.Models
{
    public class EtProducerSchema
    {
        public string Id { get; set; }
        public string? RealId { get; set; }
        public string? Prefix { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Www { get; set; }
        public string? Rating { get; set; }
        public string? ExistName { get; set; }
        public string? ExistId { get; set; }
        public string? Domain { get; set; }
        public string? TecdocSupplierId { get; set; }
        public string? MarketPrefix { get; set; }
    }
}
