using Electium.Application.DTOs;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IElectionService
    : IGenericService<
        Election,
        ElectionDto,
        CreateElectionDto,
        UpdateElectionDto>
{
    Task<bool> CompleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ElectionSummaryDto>> GetSummariesAsync(
    CancellationToken cancellationToken = default);

    Task<ElectionResultsDto?> GetResultsAsync(
    int electionId,
    CancellationToken cancellationToken = default);
}