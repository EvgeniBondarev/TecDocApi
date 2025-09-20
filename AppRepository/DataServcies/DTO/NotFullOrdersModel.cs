using OzonDomains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servcies.DataServcies.DTO
{
    public class NotFullOrdersModel
    {
        public List<Order> UniqueOrders { get; set; }

        public Dictionary<Order, List<Order>> OrdersWithMultipleMatches { get; set; }

        public Dictionary<Order, Order> OrdersWithOneMatches { get; set; }
    }
}
