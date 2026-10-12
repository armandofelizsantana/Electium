using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface ILeaderCandidateService
{
    Task<IReadOnlyList<CandidateDto>> GetAllForLeaderAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<CandidateDto?> GetForLeaderAsync(
        int leaderUserId,
        int candidateId,
        CancellationToken cancellationToken = default);

    Task<CandidateDto> CreateAsync(
        int leaderUserId,
        CreateCandidateDto dto,
        CancellationToken cancellationToken = default);

    Task<CandidateDto?> UpdateAsync(
        int leaderUserId,
        int candidateId,
        UpdateCandidateDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        int leaderUserId,
        int candidateId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveElectionAsync(
    CancellationToken cancellationToken = default);
}