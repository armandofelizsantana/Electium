using System;
using System.Collections.Generic;
using System.Text;

using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface IPoliticalLeaderService
{
    Task<IReadOnlyList<PoliticalLeaderDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<PoliticalLeaderDto> CreateAsync(
        CreatePoliticalLeaderDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<PoliticalLeaderDto?> GetByIdAsync(
    int id,
    CancellationToken cancellationToken = default);
}