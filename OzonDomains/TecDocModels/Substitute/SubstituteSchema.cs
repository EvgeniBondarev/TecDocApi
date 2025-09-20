using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.TecDocModels.Substitute
{
    public class SubstituteSchema
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public Modification Modification { get; set; }
        public List<Attribute> Attributes { get; set; }
    }
}
