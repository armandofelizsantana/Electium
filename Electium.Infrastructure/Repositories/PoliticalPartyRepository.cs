using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class PoliticalPartyRepository
    : GenericRepository<PoliticalParty>, IPoliticalPartyRepository
{
    private readonly ElectiumDbContext _context;

    public PoliticalPartyRepository(ElectiumDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.Elections
            .AsNoTracking()
            .AnyAsync(
                election =>
                    election.IsActive &&
                    election.Status == ElectionStatus.Active,
                cancellationToken);
    }

    public Task<bool> AcronymExistsAsync(
        string acronym,
        int? excludedPartyId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedAcronym = acronym.Trim().ToUpper();

        return _context.PoliticalParties
            .AsNoTracking()
            .AnyAsync(
                party =>
                    party.Acronym.ToUpper() == normalizedAcronym &&
                    (!excludedPartyId.HasValue ||
                     party.Id != excludedPartyId.Value),
                cancellationToken);
    }

    public async Task DeactivateRelatedDataAsync(
        int politicalPartyId,
        CancellationToken cancellationToken = default)
    {
        var candidates = await _context.Candidates
            .Where(candidate =>
                candidate.PoliticalPartyId == politicalPartyId)
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            candidate.IsActive = false;
        }

        var nominations = await _context.CandidateNominations
            .Where(nomination =>
                nomination.PoliticalPartyId == politicalPartyId ||
                nomination.Candidate.PoliticalPartyId == politicalPartyId)
            .ToListAsync(cancellationToken);

        foreach (var nomination in nominations)
        {
            nomination.IsActive = false;
        }
    }
}