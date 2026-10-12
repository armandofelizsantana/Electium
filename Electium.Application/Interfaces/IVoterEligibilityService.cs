using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface IVoterEligibilityService
{
    Task<VoterEligibilityDto> CheckEligibilityAsync(
        string identityNumber,
        CancellationToken cancellationToken = default);
}