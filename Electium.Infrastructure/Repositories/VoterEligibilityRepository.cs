using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Domain.Enums;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class VoterEligibilityRepository : IVoterEligibilityRepository
{
    private readonly ElectiumDbContext _context;

    public VoterEligibilityRepository(ElectiumDbContext context)
    {
        _context = context;
    }

    public async Task<VoterEligibilityDto> CheckEligibilityAsync(
        string identityNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityNumber))
        {
            return new VoterEligibilityDto
            {
                Status = VoterEligibilityStatus.InvalidIdentityNumber
            };
        }

        var election = await _context.Elections
            .AsNoTracking()
            .Where(item =>
                item.IsActive &&
                item.Status == ElectionStatus.Active)
            .OrderByDescending(item => item.StartsAtUtc)
            .Select(item => new
            {
                item.Id,
                item.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (election is null)
        {
            return new VoterEligibilityDto
            {
                Status = VoterEligibilityStatus.NoActiveElection
            };
        }

        var normalizedIdentityNumber = identityNumber.Trim();

        var citizen = await _context.Citizens
            .AsNoTracking()
            .Where(item => item.IdentityNumber == normalizedIdentityNumber)
            .Select(item => new
            {
                item.Id,
                item.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (citizen is null)
        {
            return new VoterEligibilityDto
            {
                Status = VoterEligibilityStatus.CitizenNotFound,
                ElectionId = election.Id,
                ElectionName = election.Name
            };
        }

        if (!citizen.IsActive)
        {
            return new VoterEligibilityDto
            {
                Status = VoterEligibilityStatus.CitizenInactive,
                ElectionId = election.Id,
                ElectionName = election.Name
            };
        }

        // La participacion es un registro historico no se filtra por
        // IsActive para impedir que una participacion inactivada permita
        // votar de nuevo
        var alreadyParticipated = await _context.ElectionParticipations
            .AsNoTracking()
            .AnyAsync(
                participation =>
                    participation.ElectionId == election.Id &&
                    participation.CitizenId == citizen.Id,
                cancellationToken);

        return new VoterEligibilityDto
        {
            Status = alreadyParticipated
                ? VoterEligibilityStatus.AlreadyParticipated
                : VoterEligibilityStatus.Eligible,
            ElectionId = election.Id,
            ElectionName = election.Name,
            CitizenId = alreadyParticipated ? null : citizen.Id,
        };
    }
}