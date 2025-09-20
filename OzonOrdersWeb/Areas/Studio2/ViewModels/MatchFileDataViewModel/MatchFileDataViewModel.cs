using OzonDomains.Models.MatchedRowSys;

namespace OzonOrdersWeb.ViewModels.MatchFileDataViewModel
{
    public class MatchFileDataViewModel
    {
        public List<SavedMatchingColumn> SavedMatchingColumns { get; set; }
        public List<MatchedResultInfo> MatchedResults { get; set; }
    }
}
