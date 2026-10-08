using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class CitizenService
    : GenericService<
        Citizen,
        CitizenDto,
        CreateCitizenDto,
        UpdateCitizenDto>,
      ICitizenService
{
    private readonly ICitizenRepository _citizenRepository;

    public CitizenService(
        ICitizenRepository citizenRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(citizenRepository, unitOfWork, mapper)
    {
        _citizenRepository = citizenRepository;
    }

    public override async Task<CitizenDto> CreateAsync(
        CreateCitizenDto dto,
        CancellationToken cancellationToken = default)
    {
        dto.IdentityNumber = dto.IdentityNumber.Trim();

        var alreadyExists =
            await _citizenRepository.IdentityNumberExistsAsync(
                dto.IdentityNumber,
                cancellationToken: cancellationToken);

        if (alreadyExists)
        {
            throw new BusinessRuleException(
                "Ya existe un ciudadano con ese número de documento.");
        }

        return await base.CreateAsync(dto, cancellationToken);
    }

    public override async Task<CitizenDto?> UpdateAsync(
        int id,
        UpdateCitizenDto dto,
        CancellationToken cancellationToken = default)
    {
        dto.IdentityNumber = dto.IdentityNumber.Trim();

        var citizen = await _citizenRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (citizen is null)
        {
            return null;
        }

        var alreadyExists =
            await _citizenRepository.IdentityNumberExistsAsync(
                dto.IdentityNumber,
                excludedCitizenId: id,
                cancellationToken: cancellationToken);

        if (alreadyExists)
        {
            throw new BusinessRuleException(
                "Ya existe otro ciudadano con ese número de documento.");
        }

        return await base.UpdateAsync(id, dto, cancellationToken);
    }
}