using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IElectionRepository : IGenericRepository<Election>
{
    Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default);

    Task<ElectionCreationReadinessDto> GetCreationReadinessAsync(
        CancellationToken cancellationToken = default);

    Task AddCandidateSnapshotsAsync(
    Election election,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ElectionSummaryDto>> GetElectionSummariesAsync(
    CancellationToken cancellationToken = default);

    Task<ElectionResultsDto?> GetResultsAsync(
    int electionId,
    CancellationToken cancellationToken = default);
}


