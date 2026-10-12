namespace Electium.Web.ViewModels;

public class CandidateIndexViewModel
{
    public List<CandidateListItemViewModel> Candidates { get; set; } = [];
    public bool HasActiveElection { get; set; }
}