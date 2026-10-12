using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Security;

namespace Electium.Application.DTOs;

public class VoterEligibilityDto
{
    public VoterEligibilityStatus Status { get; set; }

    public int? ElectionId { get; set; }

    public string? ElectionName { get; set; }

    public int? CitizenId { get; set; }
}