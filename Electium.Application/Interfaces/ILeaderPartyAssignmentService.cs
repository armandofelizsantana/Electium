using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface ILeaderPartyAssignmentService
{
    Task<IReadOnlyList<LeaderPartyAssignmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<LeaderPartyAssignmentDto?> GetByLeaderIdAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignablePoliticalLeaderDto>>
        GetUnassignedActiveLeadersAsync(
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignablePoliticalPartyDto>>
        GetActivePartiesAsync(
            CancellationToken cancellationToken = default);

    Task AssignAsync(
        CreateLeaderPartyAssignmentDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);
}