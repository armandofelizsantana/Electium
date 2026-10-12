using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class LeaderNominationRepository : ILeaderNominationRepository
{
    private readonly ElectiumDbContext _context;

    public LeaderNominationRepository(ElectiumDbContext context)
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

    public async Task<IReadOnlyList<LeaderNominationDto>>
        GetNominationsByPartyAsync(
            int partyId,
            CancellationToken cancellationToken = default)
    {
        return await _context.CandidateNominations
            .AsNoTracking()
            .Where(nomination => nomination.PoliticalPartyId == partyId)
            .OrderBy(nomination => nomination.ElectivePosition.Name)
            .ThenBy(nomination => nomination.Candidate.LastName)
            .Select(nomination => new LeaderNominationDto
            {
                Id = nomination.Id,
                CandidateId = nomination.CandidateId,
                CandidateFullName =
                    nomination.Candidate.FirstName + " " +
                    nomination.Candidate.LastName,
                CandidatePhotoPath = nomination.Candidate.PhotoPath,
                CandidatePartyName = nomination.Candidate.PoliticalParty.Name,
                ElectivePositionId = nomination.ElectivePositionId,
                ElectivePositionName = nomination.ElectivePosition.Name,
                IsActive = nomination.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NominationPositionOptionDto>>
        GetAvailablePositionsAsync(
            int partyId,
            CancellationToken cancellationToken = default)
    {
        return await _context.ElectivePositions
            .AsNoTracking()
            .Where(position =>
                position.IsActive &&
                !_context.CandidateNominations.Any(nomination =>
                    nomination.IsActive &&
                    nomination.PoliticalPartyId == partyId &&
                    nomination.ElectivePositionId == position.Id))
            .OrderBy(position => position.Name)
            .Select(position => new NominationPositionOptionDto
            {
                Id = position.Id,
                Name = position.Name
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NominationCandidateOptionDto>>
        GetAvailableCandidatesAsync(
            int partyId,
            int electivePositionId,
            CancellationToken cancellationToken = default)
    {
        var positionIsAvailable = await _context.ElectivePositions
            .AsNoTracking()
            .AnyAsync(
                position =>
                    position.Id == electivePositionId &&
                    position.IsActive &&
                    !_context.CandidateNominations.Any(nomination =>
                        nomination.IsActive &&
                        nomination.PoliticalPartyId == partyId &&
                        nomination.ElectivePositionId == electivePositionId),
                cancellationToken);

        if (!positionIsAvailable)
        {
            return [];
        }

        return await _context.Candidates
            .AsNoTracking()
            .Where(candidate =>
                candidate.IsActive &&
                candidate.PoliticalParty.IsActive &&
                !_context.CandidateNominations.Any(nomination =>
                    nomination.IsActive &&
                    nomination.PoliticalPartyId == partyId &&
                    nomination.CandidateId == candidate.Id) &&
                (
                    candidate.PoliticalPartyId == partyId ||
                    (
                        _context.PoliticalAlliances.Any(alliance =>
                            alliance.IsActive &&
                            alliance.FirstParty.IsActive &&
                            alliance.SecondParty.IsActive &&
                            (
                                (alliance.FirstPartyId == partyId &&
                                 alliance.SecondPartyId ==
                                     candidate.PoliticalPartyId) ||
                                (alliance.SecondPartyId == partyId &&
                                 alliance.FirstPartyId ==
                                     candidate.PoliticalPartyId)
                            )
                        ) &&
                        _context.CandidateNominations.Any(originNomination =>
                            originNomination.IsActive &&
                            originNomination.PoliticalPartyId ==
                                candidate.PoliticalPartyId &&
                            originNomination.CandidateId == candidate.Id &&
                            originNomination.ElectivePositionId ==
                                electivePositionId)
                    )
                ))
            .OrderBy(candidate => candidate.PoliticalParty.Name)
            .ThenBy(candidate => candidate.LastName)
            .ThenBy(candidate => candidate.FirstName)
            .Select(candidate => new NominationCandidateOptionDto
            {
                Id = candidate.Id,
                FullName =
                    candidate.FirstName + " " + candidate.LastName,
                OriginPartyName = candidate.PoliticalParty.Name
            })
            .ToListAsync(cancellationToken);
    }

    public Task<Candidate?> GetActiveCandidateAsync(
        int candidateId,
        CancellationToken cancellationToken = default)
    {
        return _context.Candidates
            .AsNoTracking()
            .FirstOrDefaultAsync(
                candidate => candidate.Id == candidateId &&
                             candidate.IsActive &&
                             candidate.PoliticalParty.IsActive,
                cancellationToken);
    }

    public Task<ElectivePosition?> GetActivePositionAsync(
        int electivePositionId,
        CancellationToken cancellationToken = default)
    {
        return _context.ElectivePositions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                position =>
                    position.Id == electivePositionId &&
                    position.IsActive,
                cancellationToken);
    }

    public Task<bool> ArePartiesAlliedAsync(
        int firstPartyId,
        int secondPartyId,
        CancellationToken cancellationToken = default)
    {
        return _context.PoliticalAlliances
            .AsNoTracking()
            .AnyAsync(
                alliance =>
                    alliance.IsActive &&
                    alliance.FirstParty.IsActive &&
                    alliance.SecondParty.IsActive &&
                    (
                        (alliance.FirstPartyId == firstPartyId &&
                         alliance.SecondPartyId == secondPartyId) ||
                        (alliance.SecondPartyId == firstPartyId &&
                         alliance.FirstPartyId == secondPartyId)
                    ),
                cancellationToken);
    }

    public Task<bool> HasActiveNominationForCandidateAsync(
        int partyId,
        int candidateId,
        CancellationToken cancellationToken = default)
    {
        return _context.CandidateNominations
            .AsNoTracking()
            .AnyAsync(
                nomination =>
                    nomination.IsActive &&
                    nomination.PoliticalPartyId == partyId &&
                    nomination.CandidateId == candidateId,
                cancellationToken);
    }

    public Task<bool> HasActiveNominationForPositionAsync(
        int partyId,
        int electivePositionId,
        CancellationToken cancellationToken = default)
    {
        return _context.CandidateNominations
            .AsNoTracking()
            .AnyAsync(
                nomination =>
                    nomination.IsActive &&
                    nomination.PoliticalPartyId == partyId &&
                    nomination.ElectivePositionId == electivePositionId,
                cancellationToken);
    }

    public Task<bool> HasOriginPartyNominationAsync(
        int originPartyId,
        int candidateId,
        int electivePositionId,
        CancellationToken cancellationToken = default)
    {
        return _context.CandidateNominations
            .AsNoTracking()
            .AnyAsync(
                nomination =>
                    nomination.IsActive &&
                    nomination.PoliticalPartyId == originPartyId &&
                    nomination.CandidateId == candidateId &&
                    nomination.ElectivePositionId == electivePositionId,
                cancellationToken);
    }

    public Task<CandidateNomination?> GetNominationForPartyAsync(
        int nominationId,
        int partyId,
        CancellationToken cancellationToken = default)
    {
        return _context.CandidateNominations
            .FirstOrDefaultAsync(
                nomination =>
                    nomination.Id == nominationId &&
                    nomination.PoliticalPartyId == partyId,
                cancellationToken);
    }

    public async Task AddNominationAsync(
        CandidateNomination nomination,
        CancellationToken cancellationToken = default)
    {
        await _context.CandidateNominations.AddAsync(
            nomination,
            cancellationToken);
    }
}