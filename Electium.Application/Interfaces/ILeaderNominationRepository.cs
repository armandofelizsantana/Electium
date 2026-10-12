using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface ILeaderNominationRepository
{
    Task<int?> GetActivePartyIdForLeaderAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaderNominationDto>> GetNominationsByPartyAsync(
        int partyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NominationPositionOptionDto>>
        GetAvailablePositionsAsync(
            int partyId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NominationCandidateOptionDto>>
        GetAvailableCandidatesAsync(
            int partyId,
            int electivePositionId,
            CancellationToken cancellationToken = default);

    Task<Candidate?> GetActiveCandidateAsync(
        int candidateId,
        CancellationToken cancellationToken = default);

    Task<ElectivePosition?> GetActivePositionAsync(
        int electivePositionId,
        CancellationToken cancellationToken = default);

    Task<bool> ArePartiesAlliedAsync(
        int firstPartyId,
        int secondPartyId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveNominationForCandidateAsync(
        int partyId,
        int candidateId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveNominationForPositionAsync(
        int partyId,
        int electivePositionId,
        CancellationToken cancellationToken = default);

    Task<bool> HasOriginPartyNominationAsync(
        int originPartyId,
        int candidateId,
        int electivePositionId,
        CancellationToken cancellationToken = default);

    Task<CandidateNomination?> GetNominationForPartyAsync(
        int nominationId,
        int partyId,
        CancellationToken cancellationToken = default);

    Task AddNominationAsync(
        CandidateNomination nomination,
        CancellationToken cancellationToken = default);
}