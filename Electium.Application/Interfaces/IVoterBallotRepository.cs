using System;
using System.Collections.Generic;
using System.Text;

using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IVoterBallotRepository
{
    Task<VoterBallotDto?> GetActiveBallotAsync(
        int electionId,
        CancellationToken cancellationToken = default);

    Task<bool> HasParticipatedAsync(
        int electionId,
        int citizenId,
        CancellationToken cancellationToken = default);

    Task AddSubmittedBallotAsync(
        Ballot ballot,
        IReadOnlyList<VoteSelection> selections,
        ElectionParticipation participation,
        CancellationToken cancellationToken = default);
}