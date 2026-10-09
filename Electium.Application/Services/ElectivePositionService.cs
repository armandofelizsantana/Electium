using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class ElectivePositionService
    : GenericService<
        ElectivePosition,
        ElectivePositionDto,
        CreateElectivePositionDto,
        UpdateElectivePositionDto>,
      IElectivePositionService
{
    private readonly IElectivePositionRepository _repository;

    public ElectivePositionService(
        IElectivePositionRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(repository, unitOfWork, mapper)
    {
        _repository = repository;
    }

    public override async Task<ElectivePositionDto> CreateAsync(
        CreateElectivePositionDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

        return await base.CreateAsync(dto, cancellationToken);
    }

    public override async Task<ElectivePositionDto?> UpdateAsync(
        int id,
        UpdateElectivePositionDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    public override async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

        return await base.SetActiveAsync(id, isActive, cancellationToken);
    }

    private async Task EnsureThereIsNoActiveElectionAsync(
        CancellationToken cancellationToken)
    {
        var hasActiveElection =
            await _repository.HasActiveElectionAsync(cancellationToken);

        if (hasActiveElection)
        {
            throw new BusinessRuleException(
                "No se pueden crear, editar, activar ni desactivar puestos mientras haya una elección activa.");
        }
    }
}