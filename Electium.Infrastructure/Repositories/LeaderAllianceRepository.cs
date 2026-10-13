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

public class LeaderAllianceRepository : ILeaderAllianceRepository
{
    private readonly ElectiumDbContext _context;

    public LeaderAllianceRepository(ElectiumDbContext context)
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

    public async Task<IReadOnlyList<IncomingAllianceRequestDto>>
        GetIncomingRequestsAsync(
            int partyId,
            CancellationToken cancellationToken = default)
    {
        return await _context.AllianceRequests
            .AsNoTracking()
            .Where(request =>
                request.IsActive &&
                request.Status == AllianceRequestStatus.Pending &&
                request.ReceivingPartyId == partyId)
            .OrderByDescending(request => request.RequestedAtUtc)
            .Select(request => new IncomingAllianceRequestDto
            {
                Id = request.Id,
                RequestingPartyName = request.RequestingParty.Name,
                RequestingPartyAcronym = request.RequestingParty.Acronym,
                RequestedAtUtc = request.RequestedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OutgoingAllianceRequestDto>>
        GetOutgoingRequestsAsync(
            int partyId,
            CancellationToken cancellationToken = default)
    {
        return await _context.AllianceRequests
            .AsNoTracking()
            .Where(request =>
                request.IsActive &&
                request.RequestingPartyId == partyId)
            .OrderByDescending(request => request.RequestedAtUtc)
            .Select(request => new OutgoingAllianceRequestDto
            {
                Id = request.Id,
                ReceivingPartyName = request.ReceivingParty.Name,
                ReceivingPartyAcronym = request.ReceivingParty.Acronym,
                RequestedAtUtc = request.RequestedAtUtc,
                Status = request.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ActiveAllianceDto>>
        GetActiveAlliancesAsync(
            int partyId,
            CancellationToken cancellationToken = default)
    {
        return await _context.PoliticalAlliances
            .AsNoTracking()
            .Where(alliance =>
                alliance.IsActive &&
                alliance.FirstParty.IsActive &&
                alliance.SecondParty.IsActive &&
                (alliance.FirstPartyId == partyId ||
                 alliance.SecondPartyId == partyId))
            .OrderBy(alliance => alliance.AcceptedAtUtc)
            .Select(alliance => new ActiveAllianceDto
            {
                Id = alliance.Id,
                AlliedPartyName =
                    alliance.FirstPartyId == partyId
                        ? alliance.SecondParty.Name
                        : alliance.FirstParty.Name,
                AlliedPartyAcronym =
                    alliance.FirstPartyId == partyId
                        ? alliance.SecondParty.Acronym
                        : alliance.FirstParty.Acronym,
                AcceptedAtUtc = alliance.AcceptedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableAlliancePartyDto>>
        GetAvailablePartiesAsync(
            int partyId,
            CancellationToken cancellationToken = default)
    {
        return await _context.PoliticalParties
            .AsNoTracking()
            .Where(party =>
                party.IsActive &&
                party.Id != partyId &&
                !_context.AllianceRequests.Any(request =>
                    request.IsActive &&
                    request.Status == AllianceRequestStatus.Pending &&
                    (
                        (request.RequestingPartyId == partyId &&
                         request.ReceivingPartyId == party.Id) ||
                        (request.RequestingPartyId == party.Id &&
                         request.ReceivingPartyId == partyId)
                    )) &&
                !_context.PoliticalAlliances.Any(alliance =>
                    alliance.IsActive &&
                    (
                        (alliance.FirstPartyId == partyId &&
                         alliance.SecondPartyId == party.Id) ||
                        (alliance.FirstPartyId == party.Id &&
                         alliance.SecondPartyId == partyId)
                    )))
            .OrderBy(party => party.Name)
            .Select(party => new AvailableAlliancePartyDto
            {
                Id = party.Id,
                Name = party.Name,
                Acronym = party.Acronym
            })
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsPartyActiveAsync(
        int partyId,
        CancellationToken cancellationToken = default)
    {
        return _context.PoliticalParties
            .AsNoTracking()
            .AnyAsync(
                party => party.Id == partyId && party.IsActive,
                cancellationToken);
    }

    public Task<bool> HasPendingRequestBetweenPartiesAsync(
        int firstPartyId,
        int secondPartyId,
        CancellationToken cancellationToken = default)
    {
        return _context.AllianceRequests
            .AsNoTracking()
            .AnyAsync(
                request =>
                    request.IsActive &&
                    request.Status == AllianceRequestStatus.Pending &&
                    (
                        (request.RequestingPartyId == firstPartyId &&
                         request.ReceivingPartyId == secondPartyId) ||
                        (request.RequestingPartyId == secondPartyId &&
                         request.ReceivingPartyId == firstPartyId)
                    ),
                cancellationToken);
    }

    public Task<bool> HasActiveAllianceBetweenPartiesAsync(
        int firstPartyId,
        int secondPartyId,
        CancellationToken cancellationToken = default)
    {
        return _context.PoliticalAlliances
            .AsNoTracking()
            .AnyAsync(
                alliance =>
                    alliance.IsActive &&
                    (
                        (alliance.FirstPartyId == firstPartyId &&
                         alliance.SecondPartyId == secondPartyId) ||
                        (alliance.FirstPartyId == secondPartyId &&
                         alliance.SecondPartyId == firstPartyId)
                    ),
                cancellationToken);
    }

    public Task<AllianceRequest?> GetPendingIncomingRequestAsync(
        int requestId,
        int receivingPartyId,
        CancellationToken cancellationToken = default)
    {
        return _context.AllianceRequests
            .FirstOrDefaultAsync(
                request =>
                    request.Id == requestId &&
                    request.IsActive &&
                    request.Status == AllianceRequestStatus.Pending &&
                    request.ReceivingPartyId == receivingPartyId,
                cancellationToken);
    }

    public Task<AllianceRequest?> GetOutgoingRequestAsync(
        int requestId,
        int requestingPartyId,
        CancellationToken cancellationToken = default)
    {
        return _context.AllianceRequests
            .FirstOrDefaultAsync(
                request =>
                    request.Id == requestId &&
                    request.IsActive &&
                    request.RequestingPartyId == requestingPartyId,
                cancellationToken);
    }

    public async Task AddRequestAsync(
        AllianceRequest request,
        CancellationToken cancellationToken = default)
    {
        await _context.AllianceRequests.AddAsync(
            request,
            cancellationToken);
    }

    public async Task AddAllianceAsync(
        PoliticalAlliance alliance,
        CancellationToken cancellationToken = default)
    {
        await _context.PoliticalAlliances.AddAsync(
            alliance,
            cancellationToken);
    }
}