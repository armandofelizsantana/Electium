using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class ElectionResultsDto
{
    public int ElectionId { get; set; }

    public string ElectionName { get; set; } = string.Empty;

    public IReadOnlyList<ElectionPositionResultDto> Positions { get; set; }
        = Array.Empty<ElectionPositionResultDto>();
}