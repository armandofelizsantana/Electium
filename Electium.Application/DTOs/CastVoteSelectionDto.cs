using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class CastVoteSelectionDto
{
    public int ElectivePositionId { get; set; }

    public int? ElectionCandidateId { get; set; }

    public bool IsNone { get; set; }
}