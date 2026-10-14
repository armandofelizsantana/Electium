namespace Electium.Web.ViewModels;

public class VoterBallotPositionsViewModel
{
    public string ElectionName { get; set; } = string.Empty;

    public List<VoterBallotPositionViewModel> Positions { get; set; } = [];
}

public class VoterBallotPositionViewModel
{
    public int ElectivePositionId { get; set; }

    public string PositionName { get; set; } = string.Empty;

    public int PoliticalPartyCount { get; set; }

    public int CandidateCount { get; set; }

    public bool IsSelected { get; set; }

    public string? SelectedOptionText { get; set; }
}

public class VoterPositionViewModel
{
    public int ElectivePositionId { get; set; }

    public string ElectionName { get; set; } = string.Empty;

    public string PositionName { get; set; } = string.Empty;

    public int? SelectedElectionCandidateId { get; set; }

    public bool IsNoneSelected { get; set; }

    public List<VoterBallotCandidateViewModel> Candidates { get; set; } = [];
}

public class VoterBallotCandidateViewModel
{
    public int ElectionCandidateId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhotoPath { get; set; } = string.Empty;

    public string PartyName { get; set; } = string.Empty;

    public string PartyAcronym { get; set; } = string.Empty;

    public string PartyLogoPath { get; set; } = string.Empty;
}

public class SubmitVoteSelectionViewModel
{
    public int ElectivePositionId { get; set; }

    public int? ElectionCandidateId { get; set; }

    public bool IsNone { get; set; }
}

public class VoteCompletedViewModel
{
    public string ElectionName { get; set; } = string.Empty;

    public DateTime SubmittedAtLocal { get; set; }

    public IReadOnlyList<string> SelectionSummary { get; set; } = [];

    public bool EmailSent { get; set; }
}