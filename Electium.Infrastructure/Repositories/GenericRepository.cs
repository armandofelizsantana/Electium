using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Repositories;

public class GenericRepository<TEntity>
    : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly ElectiumDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(ElectiumDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            entity => entity.Id == id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(entity => entity.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }
}