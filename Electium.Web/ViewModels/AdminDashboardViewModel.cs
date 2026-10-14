namespace Electium.Web.ViewModels;

public class AdminDashboardViewModel
{
    public List<int> AvailableYears { get; set; } = [];

    public int? SelectedYear { get; set; }

    public List<AdminElectionSummaryViewModel> ElectionSummaries
    { get; set; } = [];
}

public class AdminElectionSummaryViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public int PoliticalPartyCount { get; set; }

    public int CandidateCount { get; set; }

    public int TotalVoteCount { get; set; }
}