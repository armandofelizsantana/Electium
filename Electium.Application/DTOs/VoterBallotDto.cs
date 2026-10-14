using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class VoterBallotDto
{
    public int ElectionId { get; set; }

    public string ElectionName { get; set; } = string.Empty;

    public IReadOnlyList<VoterBallotPositionDto> Positions { get; set; }
        = Array.Empty<VoterBallotPositionDto>();
}

public class VoterBallotPositionDto
{
    public int ElectivePositionId { get; set; }

    public string PositionName { get; set; } = string.Empty;

    public int PoliticalPartyCount { get; set; }

 
    public int CandidateCount { get; set; }

    public IReadOnlyList<VoterBallotCandidateDto> Candidates { get; set; }
        = Array.Empty<VoterBallotCandidateDto>();
}

public class VoterBallotCandidateDto
{
    
    public int ElectionCandidateId { get; set; }

    public int CandidateId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhotoPath { get; set; } = string.Empty;

    public string PartyName { get; set; } = string.Empty;

    public string PartyAcronym { get; set; } = string.Empty;

    public string PartyLogoPath { get; set; } = string.Empty;
}