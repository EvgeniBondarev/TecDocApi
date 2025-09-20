using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonDomains.Models.MatchedRowSys
{
    public class MatchedResult
    {
        public int Id { get; set; }
        public string? MainFileName { get; set; }
        public string? ScondaryFileName { get; set; }
        public DateTime СreationDate { get; set; }
        public ICollection<MatchedRow>? MatchedRows { get; set;}
        public ICollection<MatchingColumn>? MatchedColumns { get; set;}
    }
}
