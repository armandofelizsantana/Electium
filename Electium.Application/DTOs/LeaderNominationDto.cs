using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class LeaderNominationDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string CandidateFullName { get; set; } = string.Empty;
    public string CandidatePhotoPath { get; set; } = string.Empty;
    public string CandidatePartyName { get; set; } = string.Empty;
    public int ElectivePositionId { get; set; }
    public string ElectivePositionName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}