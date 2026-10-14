using Electium.Application.DTOs;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class ElectionRepository
    : GenericRepository<Election>, IElectionRepository
{


    public async Task<ElectionResultsDto?> GetResultsAsync(
    int electionId,
    CancellationToken cancellationToken = default)
    {
        var election = await _context.Elections
            .AsNoTracking()
            .Where(item => item.Id == electionId)
            .Select(item => new
            {
                item.Id,
                item.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (election is null)
        {
            return null;
        }

        var snapshots = await _context.ElectionCandidates
            .AsNoTracking()
            .Where(candidate => candidate.ElectionId == electionId)
            .ToListAsync(cancellationToken);

        var voteCounts = await _context.VoteSelections
            .AsNoTracking()
            .Where(selection => selection.Ballot.ElectionId == electionId)
            .GroupBy(selection => new
            {
                selection.ElectivePositionId,
                selection.ElectionCandidateId,
                selection.IsNone
            })
            .Select(group => new
            {
                group.Key.ElectivePositionId,
                group.Key.ElectionCandidateId,
                group.Key.IsNone,
                Votes = group.Count()
            })
            .ToListAsync(cancellationToken);

        var positions = snapshots
            .GroupBy(snapshot => new
            {
                snapshot.ElectivePositionId,
                snapshot.PositionName
            })
            .Select(positionGroup =>
            {
                var positionVoteCounts = voteCounts
                    .Where(vote =>
                        vote.ElectivePositionId ==
                        positionGroup.Key.ElectivePositionId)
                    .ToList();

                var totalVotes = positionVoteCounts.Sum(vote => vote.Votes);

                var noneVotes = positionVoteCounts
                    .Where(vote => vote.IsNone)
                    .Sum(vote => vote.Votes);

                var candidates = positionGroup
                    .Select(snapshot =>
                    {
                        var votes = positionVoteCounts
                            .Where(vote =>
                                vote.ElectionCandidateId == snapshot.Id &&
                                !vote.IsNone)
                            .Sum(vote => vote.Votes);

                        return new ElectionCandidateResultDto
                        {
                            ElectionCandidateId = snapshot.Id,
                            CandidateFirstName = snapshot.CandidateFirstName,
                            CandidateLastName = snapshot.CandidateLastName,
                            CandidatePhotoPath = snapshot.CandidatePhotoPath,
                            PartyName = snapshot.PartyName,
                            PartyAcronym = snapshot.PartyAcronym,
                            PartyLogoPath = snapshot.PartyLogoPath,
                            Votes = votes,
                            Percentage = totalVotes == 0
                                ? 0
                                : Math.Round(
                                    votes * 100m / totalVotes,
                                    2)
                        };
                    })
                    .OrderByDescending(candidate => candidate.Votes)
                    .ThenBy(candidate => candidate.CandidateLastName)
                    .ToList();

                return new ElectionPositionResultDto
                {
                    ElectivePositionId = positionGroup.Key.ElectivePositionId,
                    PositionName = positionGroup.Key.PositionName,
                    TotalVotes = totalVotes,
                    NoneVotes = noneVotes,
                    NonePercentage = totalVotes == 0
                        ? 0
                        : Math.Round(noneVotes * 100m / totalVotes, 2),
                    Candidates = candidates
                };
            })
            .OrderBy(position => position.PositionName)
            .ToList();

        return new ElectionResultsDto
        {
            ElectionId = election.Id,
            ElectionName = election.Name,
            Positions = positions
        };
    }

    public async Task<IReadOnlyList<ElectionSummaryDto>>
    GetElectionSummariesAsync(
        CancellationToken cancellationToken = default)
    {
        var electionCounts = await _context.ElectionCandidates
            .AsNoTracking()
            .GroupBy(candidate => candidate.ElectionId)
            .Select(group => new
            {
                ElectionId = group.Key,

                PoliticalPartyCount = group
                    .Select(candidate => candidate.PoliticalPartyId)
                    .Distinct()
                    .Count(),

                CandidateCount = group
                    .Select(candidate => candidate.CandidateId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync(cancellationToken);

        var countsByElectionId = electionCounts
            .ToDictionary(item => item.ElectionId);

        var participationCounts = await _context.ElectionParticipations
            .AsNoTracking()
            .GroupBy(participation => participation.ElectionId)
            .Select(group => new
            {
                ElectionId = group.Key,
                TotalVoteCount = group
                    .Select(participation => participation.CitizenId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync(cancellationToken);

        var participationsByElectionId = participationCounts
            .ToDictionary(item => item.ElectionId);

        var elections = await _context.Elections
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return elections
            .OrderByDescending(
                election => election.Status == ElectionStatus.Active)
            .ThenByDescending(election => election.StartsAtUtc)
            .Select(election =>
            {
                countsByElectionId.TryGetValue(
                    election.Id,
                    out var counts);

                participationsByElectionId.TryGetValue(
                    election.Id,
                    out var participations);

                return new ElectionSummaryDto
                {
                    Id = election.Id,
                    Name = election.Name,
                    Year = election.Year,
                    StartsAtUtc = election.StartsAtUtc,
                    Status = election.Status,
                    IsActive = election.IsActive,
                    PoliticalPartyCount =
                        counts?.PoliticalPartyCount ?? 0,
                    CandidateCount =
                        counts?.CandidateCount ?? 0,
                    TotalVoteCount =
                        participations?.TotalVoteCount ?? 0
                };
            })
            .ToList();
    }
    private readonly ElectiumDbContext _context;

    public ElectionRepository(ElectiumDbContext context)
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

    public async Task<ElectionCreationReadinessDto> GetCreationReadinessAsync(
        CancellationToken cancellationToken = default)
    {
        var hasActiveElection = await HasActiveElectionAsync(cancellationToken);

        var activePositions = await _context.ElectivePositions
            .AsNoTracking()
            .Where(position => position.IsActive)
            .Select(position => new
            {
                position.Id,
                position.Name
            })
            .ToListAsync(cancellationToken);

        var activeParties = await _context.PoliticalParties
            .AsNoTracking()
            .Where(party => party.IsActive)
            .Select(party => new
            {
                party.Id,
                party.Name,
                party.Acronym
            })
            .ToListAsync(cancellationToken);

        var activeNominations = await _context.CandidateNominations
            .AsNoTracking()
            .Where(nomination =>
                nomination.IsActive &&
                nomination.Candidate.IsActive &&
                nomination.Candidate.PoliticalParty.IsActive &&
                nomination.PoliticalParty.IsActive &&
                nomination.ElectivePosition.IsActive)
            .Select(nomination => new
            {
                nomination.PoliticalPartyId,
                nomination.ElectivePositionId
            })
            .ToListAsync(cancellationToken);

        var nominationPairs = activeNominations
            .Select(nomination =>
                (nomination.PoliticalPartyId, nomination.ElectivePositionId))
            .ToHashSet();

        var partiesMissingPositions = activeParties
            .Select(party =>
            {
                var missingPositionNames = activePositions
                    .Where(position =>
                        !nominationPairs.Contains((party.Id, position.Id)))
                    .Select(position => position.Name)
                    .ToList();

                return new PartyMissingPositionsDto
                {
                    PoliticalPartyId = party.Id,
                    PoliticalPartyName = party.Name,
                    PoliticalPartyAcronym = party.Acronym,
                    MissingPositionNames = missingPositionNames
                };
            })
            .Where(party => party.MissingPositionNames.Count > 0)
            .ToList();

        return new ElectionCreationReadinessDto
        {
            HasActiveElection = hasActiveElection,
            ActiveElectivePositionCount = activePositions.Count,
            ActivePoliticalPartyCount = activeParties.Count,
            PartiesMissingPositions = partiesMissingPositions
        };
    }

    public async Task AddCandidateSnapshotsAsync(
        Election election,
        CancellationToken cancellationToken = default)
    {
        var activeNominations = await _context.CandidateNominations
            .AsNoTracking()
            .Where(nomination =>
                nomination.IsActive &&
                nomination.Candidate.IsActive &&
                nomination.Candidate.PoliticalParty.IsActive &&
                nomination.PoliticalParty.IsActive &&
                nomination.ElectivePosition.IsActive)
            .Select(nomination => new
            {
                nomination.CandidateId,
                nomination.PoliticalPartyId,
                nomination.ElectivePositionId,
                CandidateFirstName = nomination.Candidate.FirstName,
                CandidateLastName = nomination.Candidate.LastName,
                CandidatePhotoPath = nomination.Candidate.PhotoPath,
                PartyName = nomination.PoliticalParty.Name,
                PartyAcronym = nomination.PoliticalParty.Acronym,
                PartyLogoPath = nomination.PoliticalParty.LogoPath,
                PositionName = nomination.ElectivePosition.Name
            })
            .ToListAsync(cancellationToken);

        var snapshots = activeNominations
            .Select(nomination => new ElectionCandidate
            {
                Election = election,
                CandidateId = nomination.CandidateId,
                PoliticalPartyId = nomination.PoliticalPartyId,
                ElectivePositionId = nomination.ElectivePositionId,
                CandidateFirstName = nomination.CandidateFirstName,
                CandidateLastName = nomination.CandidateLastName,
                CandidatePhotoPath = nomination.CandidatePhotoPath,
                PartyName = nomination.PartyName,
                PartyAcronym = nomination.PartyAcronym,
                PartyLogoPath = nomination.PartyLogoPath,
                PositionName = nomination.PositionName
            })
            .ToList();

        await _context.ElectionCandidates.AddRangeAsync(
            snapshots,
            cancellationToken);




    }
}