using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface ILeaderCandidateRepository
    : IGenericRepository<Candidate>
{
    Task<int?> GetActivePartyIdForLeaderAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Candidate>> GetByPartyAsync(
        int partyId,
        CancellationToken cancellationToken = default);

    Task<Candidate?> GetTrackedByIdAndPartyAsync(
        int candidateId,
        int partyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, string>>
        GetPositionNamesByCandidateAsync(
            int partyId,
            CancellationToken cancellationToken = default);

    Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default);

    Task DeactivateNominationsAsync(
        int candidateId,
        int partyId,
        CancellationToken cancellationToken = default);
}