namespace Electium.Web.ViewModels;

public class ElectionResultsViewModel
{
    public int ElectionId { get; set; }

    public string ElectionName { get; set; } = string.Empty;

    public IReadOnlyList<ElectionPositionResultViewModel> Positions { get; set; }
        = Array.Empty<ElectionPositionResultViewModel>();
}

public class ElectionPositionResultViewModel
{
    public int ElectivePositionId { get; set; }

    public string PositionName { get; set; } = string.Empty;

    public int TotalVotes { get; set; }

    public int NoneVotes { get; set; }

    public decimal NonePercentage { get; set; }

    public IReadOnlyList<ElectionCandidateResultViewModel> Candidates { get; set; }
        = Array.Empty<ElectionCandidateResultViewModel>();
}

public class ElectionCandidateResultViewModel
{
    public int ElectionCandidateId { get; set; }

    public string CandidateFirstName { get; set; } = string.Empty;

    public string CandidateLastName { get; set; } = string.Empty;

    public string CandidatePhotoPath { get; set; } = string.Empty;

    public string PartyName { get; set; } = string.Empty;

    public string PartyAcronym { get; set; } = string.Empty;

    public string PartyLogoPath { get; set; } = string.Empty;

    public int Votes { get; set; }

    public decimal Percentage { get; set; }
}