using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class PoliticalPartyService
    : GenericService<
        PoliticalParty,
        PoliticalPartyDto,
        CreatePoliticalPartyDto,
        UpdatePoliticalPartyDto>,
      IPoliticalPartyService
{
    private readonly IPoliticalPartyRepository _repository;

    public PoliticalPartyService(
        IPoliticalPartyRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(repository, unitOfWork, mapper)
    {
        _repository = repository;
    }

    public override async Task<PoliticalPartyDto> CreateAsync(
        CreatePoliticalPartyDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        dto.Name = dto.Name.Trim();
        dto.Acronym = dto.Acronym.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BusinessRuleException(
                "El nombre del partido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Acronym))
        {
            throw new BusinessRuleException(
                "La sigla del partido es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(dto.LogoPath))
        {
            throw new BusinessRuleException(
                "Debes seleccionar un logo para el partido.");
        }

        if (await _repository.AcronymExistsAsync(
                dto.Acronym,
                cancellationToken: cancellationToken))
        {
            throw new BusinessRuleException(
                "Ya existe un partido con esa sigla.");
        }

        return await base.CreateAsync(dto, cancellationToken);
    }

    public override async Task<PoliticalPartyDto?> UpdateAsync(
        int id,
        UpdatePoliticalPartyDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var existingParty = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (existingParty is null)
        {
            return null;
        }

        dto.Name = dto.Name.Trim();
        dto.Acronym = dto.Acronym.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BusinessRuleException(
                "El nombre del partido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Acronym))
        {
            throw new BusinessRuleException(
                "La sigla del partido es obligatoria.");
        }

        if (await _repository.AcronymExistsAsync(
                dto.Acronym,
                excludedPartyId: id,
                cancellationToken: cancellationToken))
        {
            throw new BusinessRuleException(
                "Ya existe otro partido con esa sigla.");
        }

        // Si no se subió un logo nuevo, conserva el existente.
        dto.LogoPath = string.IsNullOrWhiteSpace(dto.LogoPath)
            ? existingParty.LogoPath
            : dto.LogoPath;

        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    public override async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        if (!isActive)
        {
            await _repository.DeactivateRelatedDataAsync(
                id,
                cancellationToken);
        }

        return await base.SetActiveAsync(
            id,
            isActive,
            cancellationToken);
    }

    private async Task EnsureNoActiveElectionAsync(
        CancellationToken cancellationToken)
    {
        if (await _repository.HasActiveElectionAsync(cancellationToken))
        {
            throw new BusinessRuleException(
                "No se pueden crear, editar, activar ni desactivar partidos mientras haya una elección en proceso.");
        }
    }
}