using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface ILeaderAllianceService
{
    Task<LeaderAllianceDashboardDto> GetDashboardAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvailableAlliancePartyDto>>
        GetAvailablePartiesAsync(
            int leaderUserId,
            CancellationToken cancellationToken = default);

    Task CreateRequestAsync(
        int leaderUserId,
        int receivingPartyId,
        CancellationToken cancellationToken = default);

    Task RespondToRequestAsync(
        int leaderUserId,
        int requestId,
        bool accept,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteOutgoingRequestAsync(
        int leaderUserId,
        int requestId,
        CancellationToken cancellationToken = default);
}