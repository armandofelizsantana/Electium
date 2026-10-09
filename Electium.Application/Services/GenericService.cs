using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class GenericService<TEntity, TDto, TCreateDto, TUpdateDto>
    : IGenericService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GenericService(
        IGenericRepository<TEntity> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);

        return _mapper.Map<List<TDto>>(entities);
    }

    public async Task<IReadOnlyList<TDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetActiveAsync(cancellationToken);

        return _mapper.Map<List<TDto>>(entities);
    }

    public async Task<TDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);

        return entity is null ? default : _mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto> CreateAsync(
        TCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<TEntity>(dto);

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<TDto?> UpdateAsync(
        int id,
        TUpdateDto dto,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return default;
        }

        _mapper.Map(dto, entity);
        _repository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TDto>(entity);
    }

    public virtual async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsActive = isActive;
        _repository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}