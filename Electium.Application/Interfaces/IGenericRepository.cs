using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;

namespace Electium.Application.Interfaces;

public interface IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity);
}