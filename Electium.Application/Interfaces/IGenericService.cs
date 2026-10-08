using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IGenericService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
{
    Task<IReadOnlyList<TDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<TDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<TDto> CreateAsync(
        TCreateDto dto,
        CancellationToken cancellationToken = default);

    Task<TDto?> UpdateAsync(
        int id,
        TUpdateDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default);
}