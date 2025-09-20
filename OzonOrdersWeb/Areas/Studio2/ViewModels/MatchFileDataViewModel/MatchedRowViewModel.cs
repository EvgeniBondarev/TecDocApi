using OzonDomains;
using OzonDomains.Models.MatchedRowSys;
using Servcies.ParserServcies.FielParsers.Models;

namespace OzonOrdersWeb.ViewModels.MatchFileDataViewModel
{
    public class MatchedRowViewModel
    {
        public int? MatchedResultsId { get; set; }
        public List<MatchedRowModel> MatchedResults { get; set; }
        public List<MatchingColumn> MatchingColumns { get; set; }
        public string? MainFileName { get; set; }
        public string? ScondaryFileName { get; set; }
    }
}
