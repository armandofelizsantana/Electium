using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class ElectivePositionRepository
    : GenericRepository<ElectivePosition>, IElectivePositionRepository
{

    public async Task DeactivateCandidatesForPositionAsync(
    int electivePositionId,
    CancellationToken cancellationToken = default)
    {
        var candidates = await _context.CandidateNominations
            .Where(nomination =>
                nomination.ElectivePositionId == electivePositionId)
            .Select(nomination => nomination.Candidate)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            candidate.IsActive = false;
        }
    }

    private readonly ElectiumDbContext _context;

    public ElectivePositionRepository(ElectiumDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Elections
            .AsNoTracking()
            .AnyAsync(
                election =>
                    election.IsActive &&
                    election.Status == ElectionStatus.Active,
                cancellationToken);
    }
}