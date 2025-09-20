using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.Models.MatchedRowSys
{
    public class SavedMatchingColumn
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<MatchingColumn> MatchingColumns { get; set; }
        public int StartRow1 { get; set; }
        public int StartColumn1 {  get; set; }
        public int StartRow2 {  get; set; }
        public int StartColumn2 { get; set; }
    }
}
