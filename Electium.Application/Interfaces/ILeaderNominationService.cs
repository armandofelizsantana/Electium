using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface ILeaderNominationService
{
    Task<IReadOnlyList<LeaderNominationDto>> GetAllAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NominationPositionOptionDto>> GetAvailablePositionsAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NominationCandidateOptionDto>>
        GetAvailableCandidatesAsync(
            int leaderUserId,
            int electivePositionId,
            CancellationToken cancellationToken = default);

    Task CreateAsync(
        int leaderUserId,
        int candidateId,
        int electivePositionId,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        int leaderUserId,
        int nominationId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default);
}