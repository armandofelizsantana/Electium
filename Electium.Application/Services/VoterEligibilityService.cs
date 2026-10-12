using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Interfaces;

namespace Electium.Application.Services;

public class VoterEligibilityService : IVoterEligibilityService
{
    private readonly IVoterEligibilityRepository _repository;

    public VoterEligibilityService(IVoterEligibilityRepository repository)
    {
        _repository = repository;
    }

    public Task<VoterEligibilityDto> CheckEligibilityAsync(
        string identityNumber,
        CancellationToken cancellationToken = default)
    {
        return _repository.CheckEligibilityAsync(
            identityNumber,
            cancellationToken);
    }
}