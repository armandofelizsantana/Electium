using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;

namespace Electium.Application.Services;

public class ElectionService
    : GenericService<
        Election,
        ElectionDto,
        CreateElectionDto,
        UpdateElectionDto>,
      IElectionService
{
    private readonly IElectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ElectionService(
        IElectionRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(repository, unitOfWork, mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<ElectionDto> CreateAsync(
        CreateElectionDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidateDates(dto.StartsAtUtc, dto.EndsAtUtc);

        return await base.CreateAsync(dto, cancellationToken);
    }

    public override async Task<ElectionDto?> UpdateAsync(
        int id,
        UpdateElectionDto dto,
        CancellationToken cancellationToken = default)
    {
        var election = await _repository.GetByIdAsync(id, cancellationToken);

        if (election is null)
        {
            return null;
        }

        if (election.Status != ElectionStatus.Configuring)
        {
            throw new BusinessRuleException(
                "Solo se pueden editar elecciones que estén en configuración.");
        }

        ValidateDates(dto.StartsAtUtc, dto.EndsAtUtc);

        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    public override async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var election = await _repository.GetByIdAsync(id, cancellationToken);

        if (election is null)
        {
            return false;
        }

        if (election.Status != ElectionStatus.Configuring)
        {
            throw new BusinessRuleException(
                "Solo se puede activar o desactivar una elección que esté en configuración.");
        }

        election.IsActive = isActive;
        _repository.Update(election);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ActivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var election = await _repository.GetByIdAsync(id, cancellationToken);

        if (election is null)
        {
            return false;
        }

        if (!election.IsActive)
        {
            throw new BusinessRuleException(
                "No se puede iniciar una elección desactivada.");
        }

        if (election.Status != ElectionStatus.Configuring)
        {
            throw new BusinessRuleException(
                "Solo se puede iniciar una elección que esté en configuración.");
        }

        if (await _repository.HasActiveElectionAsync(cancellationToken))
        {
            throw new BusinessRuleException(
                "Ya existe otra elección activa. Complétala antes de iniciar una nueva.");
        }

        election.Status = ElectionStatus.Active;
        _repository.Update(election);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CompleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var election = await _repository.GetByIdAsync(id, cancellationToken);

        if (election is null)
        {
            return false;
        }

        if (election.Status != ElectionStatus.Active)
        {
            throw new BusinessRuleException(
                "Solo se puede completar una elección que esté activa.");
        }

        election.Status = ElectionStatus.Completed;
        _repository.Update(election);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateDates(
        DateTime startsAtUtc,
        DateTime? endsAtUtc)
    {
        if (startsAtUtc == default)
        {
            throw new BusinessRuleException(
                "La fecha de inicio de la elección es obligatoria.");
        }

        if (endsAtUtc.HasValue && endsAtUtc.Value <= startsAtUtc)
        {
            throw new BusinessRuleException(
                "La fecha de finalización debe ser posterior a la fecha de inicio.");
        }
    }
}