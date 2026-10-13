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
    private readonly IElectionRepository _electionRepository;

    public CitizenService(
        ICitizenRepository citizenRepository,
        IElectionRepository electionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(citizenRepository, unitOfWork, mapper)
    {
        _citizenRepository = citizenRepository;
        _electionRepository = electionRepository;
    }

    public override async Task<CitizenDto> CreateAsync(
        CreateCitizenDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

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
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

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

    public override async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

        return await base.SetActiveAsync(
            id,
            isActive,
            cancellationToken);
    }

    private async Task EnsureThereIsNoActiveElectionAsync(
        CancellationToken cancellationToken)
    {
        var hasActiveElection =
            await _electionRepository.HasActiveElectionAsync(
                cancellationToken);

        if (hasActiveElection)
        {
            throw new BusinessRuleException(
                "No se pueden crear, editar, activar ni desactivar ciudadanos mientras haya una elección activa.");
        }
    }
}