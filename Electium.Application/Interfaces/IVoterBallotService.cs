using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface IVoterBallotService
{
    Task<VoterBallotDto?> GetActiveBallotAsync(
        int electionId,
        CancellationToken cancellationToken = default);

    Task<VoteReceiptDto> CastVoteAsync(
        int electionId,
        int citizenId,
        IReadOnlyCollection<CastVoteSelectionDto> selections,
        CancellationToken cancellationToken = default);
}