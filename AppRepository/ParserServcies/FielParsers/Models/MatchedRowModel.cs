using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servcies.ParserServcies.FielParsers.Models
{
    public class MatchedRowModel
    {
        public Dictionary<string, string> File1Data { get; set; }
        public List<Dictionary<string, string>> File2Data { get; set; }
    }
}
