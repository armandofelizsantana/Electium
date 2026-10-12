using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class PartyMissingPositionsDto
{
    public int PoliticalPartyId { get; set; }

    public string PoliticalPartyName { get; set; } = string.Empty;

    public string PoliticalPartyAcronym { get; set; } = string.Empty;

    public IReadOnlyList<string> MissingPositionNames { get; set; }
        = new List<string>();
}