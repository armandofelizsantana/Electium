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

public class VoterBallotRepository : IVoterBallotRepository
{
    private readonly ElectiumDbContext _context;

    public VoterBallotRepository(ElectiumDbContext context)
    {
        _context = context;
    }

    public async Task<VoterBallotDto?> GetActiveBallotAsync(
        int electionId,
        CancellationToken cancellationToken = default)
    {
        var election = await _context.Elections
            .AsNoTracking()
            .Where(item =>
                item.Id == electionId &&
                item.IsActive &&
                item.Status == ElectionStatus.Active)
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

        // Se usan las instantáneas de la elección para que los cambios
        // posteriores en candidatos o partidos no alteren la boleta.
        var snapshots = await _context.ElectionCandidates
            .AsNoTracking()
            .Where(candidate =>
                candidate.ElectionId == electionId &&
                candidate.IsActive)
            .ToListAsync(cancellationToken);

        var positions = snapshots
            .GroupBy(candidate => new
            {
                candidate.ElectivePositionId,
                candidate.PositionName
            })
            .Select(positionGroup =>
            {
                var candidates = positionGroup
                    .OrderBy(candidate => candidate.CandidateLastName)
                    .ThenBy(candidate => candidate.CandidateFirstName)
                    .ThenBy(candidate => candidate.PartyName)
                    .Select(candidate => new VoterBallotCandidateDto
                    {
                        ElectionCandidateId = candidate.Id,
                        CandidateId = candidate.CandidateId,
                        FirstName = candidate.CandidateFirstName,
                        LastName = candidate.CandidateLastName,
                        PhotoPath = candidate.CandidatePhotoPath,
                        PartyName = candidate.PartyName,
                        PartyAcronym = candidate.PartyAcronym,
                        PartyLogoPath = candidate.PartyLogoPath
                    })
                    .ToList();

                return new VoterBallotPositionDto
                {
                    ElectivePositionId =
                        positionGroup.Key.ElectivePositionId,
                    PositionName = positionGroup.Key.PositionName,
                    PoliticalPartyCount = positionGroup
                        .Select(candidate => candidate.PoliticalPartyId)
                        .Distinct()
                        .Count(),
                    CandidateCount = positionGroup
                        .Select(candidate => candidate.CandidateId)
                        .Distinct()
                        .Count(),
                    Candidates = candidates
                };
            })
            .OrderBy(position => position.PositionName)
            .ToList();

        return new VoterBallotDto
        {
            ElectionId = election.Id,
            ElectionName = election.Name,
            Positions = positions
        };
    }

    public Task<bool> HasParticipatedAsync(
        int electionId,
        int citizenId,
        CancellationToken cancellationToken = default)
    {
        // Se busca el registro aunque esté inactivo: la participación pasada
        // no debe permitir que la persona vote otra vez.
        return _context.ElectionParticipations
            .AsNoTracking()
            .AnyAsync(
                participation =>
                    participation.ElectionId == electionId &&
                    participation.CitizenId == citizenId,
                cancellationToken);
    }

    public async Task AddSubmittedBallotAsync(
        Ballot ballot,
        IReadOnlyList<VoteSelection> selections,
        ElectionParticipation participation,
        CancellationToken cancellationToken = default)
    {
        await _context.Ballots.AddAsync(ballot, cancellationToken);

        await _context.VoteSelections.AddRangeAsync(
            selections,
            cancellationToken);

        await _context.ElectionParticipations.AddAsync(
            participation,
            cancellationToken);
    }
}