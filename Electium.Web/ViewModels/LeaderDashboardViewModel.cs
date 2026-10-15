namespace Electium.Web.ViewModels;

public class LeaderDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string? PoliticalPartyName { get; set; }
    public string? PoliticalPartyAcronym { get; set; }
    public string? PoliticalPartyLogoPath { get; set; }

    public int ActiveCandidateCount { get; set; }
    public int InactiveCandidateCount { get; set; }
    public int ActiveAllianceCount { get; set; }
    public int PendingIncomingAllianceRequestCount { get; set; }
    public int AssignedCandidateCount { get; set; }
}