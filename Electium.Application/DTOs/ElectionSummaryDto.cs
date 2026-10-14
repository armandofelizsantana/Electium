using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Enums;

namespace Electium.Application.DTOs;

public class ElectionSummaryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public ElectionStatus Status { get; set; }

    public bool IsActive { get; set; }

    public int PoliticalPartyCount { get; set; }

    public int ElectivePositionCount { get; set; }

    public int CandidateCount { get; set; }

    public int TotalVoteCount { get; set; }
}