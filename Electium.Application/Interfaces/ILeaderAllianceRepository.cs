using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface ILeaderAllianceRepository
{
    Task<int?> GetActivePartyIdForLeaderAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IncomingAllianceRequestDto>>
        GetIncomingRequestsAsync(
            int partyId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OutgoingAllianceRequestDto>>
        GetOutgoingRequestsAsync(
            int partyId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActiveAllianceDto>>
        GetActiveAlliancesAsync(
            int partyId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvailableAlliancePartyDto>>
        GetAvailablePartiesAsync(
            int partyId,
            CancellationToken cancellationToken = default);

    Task<bool> IsPartyActiveAsync(
        int partyId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingRequestBetweenPartiesAsync(
        int firstPartyId,
        int secondPartyId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveAllianceBetweenPartiesAsync(
        int firstPartyId,
        int secondPartyId,
        CancellationToken cancellationToken = default);

    Task<AllianceRequest?> GetPendingIncomingRequestAsync(
        int requestId,
        int receivingPartyId,
        CancellationToken cancellationToken = default);

    Task<AllianceRequest?> GetOutgoingRequestAsync(
        int requestId,
        int requestingPartyId,
        CancellationToken cancellationToken = default);

    Task AddRequestAsync(
        AllianceRequest request,
        CancellationToken cancellationToken = default);

    Task AddAllianceAsync(
        PoliticalAlliance alliance,
        CancellationToken cancellationToken = default);
}