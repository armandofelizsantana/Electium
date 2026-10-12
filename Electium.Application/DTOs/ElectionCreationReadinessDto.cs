using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class ElectionCreationReadinessDto
{
    public bool HasActiveElection { get; set; }

    public int ActiveElectivePositionCount { get; set; }

    public int ActivePoliticalPartyCount { get; set; }

    public IReadOnlyList<PartyMissingPositionsDto> PartiesMissingPositions
    { get; set; } = new List<PartyMissingPositionsDto>();
}