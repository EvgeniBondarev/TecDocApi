using Servcies.ParserServcies.FielParsers.Models;

namespace OzonOrdersWeb.ViewModels.MatchFileDataViewModel
{
    public class MatchedRowsRequestModel
    {
        public int MatchedResultsId { get; set; }
        public List<MatchedRowModel> MatchedRows { get; set; }
    }
}
