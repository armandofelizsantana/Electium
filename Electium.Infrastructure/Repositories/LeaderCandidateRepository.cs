using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;
using Electium.Infrastructure.Identity;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class LeaderCandidateRepository
    : GenericRepository<Candidate>, ILeaderCandidateRepository
{
    private readonly ElectiumDbContext _context;

    public LeaderCandidateRepository(ElectiumDbContext context)
        : base(context)
    {
        _context = context;
    }

    public Task<int?> GetActivePartyIdForLeaderAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default)
    {
        return _context.Users
            .AsNoTracking()
            .Where(user =>
                user.Id == leaderUserId &&
                user.IsActive &&
                user.PoliticalPartyId.HasValue &&
                user.PoliticalParty!.IsActive)
            .Select(user => user.PoliticalPartyId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Candidate>> GetByPartyAsync(
        int partyId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Candidates
            .AsNoTracking()
            .Where(candidate => candidate.PoliticalPartyId == partyId)
            .OrderBy(candidate => candidate.LastName)
            .ThenBy(candidate => candidate.FirstName)
            .ToListAsync(cancellationToken);
    }

    public Task<Candidate?> GetTrackedByIdAndPartyAsync(
        int candidateId,
        int partyId,
        CancellationToken cancellationToken = default)
    {
        return _context.Candidates.FirstOrDefaultAsync(
            candidate =>
                candidate.Id == candidateId &&
                candidate.PoliticalPartyId == partyId,
            cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, string>>
        GetPositionNamesByCandidateAsync(
            int partyId,
            CancellationToken cancellationToken = default)
    {
        var rows = await _context.CandidateNominations
            .AsNoTracking()
            .Where(nomination =>
                nomination.IsActive &&
                nomination.PoliticalPartyId == partyId)
            .Select(nomination => new
            {
                nomination.CandidateId,
                PositionName = nomination.ElectivePosition.Name
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.CandidateId)
            .ToDictionary(
                group => group.Key,
                group => group.First().PositionName);
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

    public async Task DeactivateNominationsAsync(
        int candidateId,
        int partyId,
        CancellationToken cancellationToken = default)
    {
        var nominations = await _context.CandidateNominations
            .Where(nomination =>
                nomination.CandidateId == candidateId &&
                nomination.PoliticalPartyId == partyId)
            .ToListAsync(cancellationToken);

        foreach (var nomination in nominations)
        {
            nomination.IsActive = false;
        }
    }
}