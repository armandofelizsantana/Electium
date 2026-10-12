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
    private readonly IMapper _mapper;


    public Task<ElectionResultsDto?> GetResultsAsync(
    int electionId,
    CancellationToken cancellationToken = default)
    {
        return _repository.GetResultsAsync(electionId, cancellationToken);
    }

    public Task<IReadOnlyList<ElectionSummaryDto>> GetSummariesAsync(
    CancellationToken cancellationToken = default)
    {
        return _repository.GetElectionSummariesAsync(cancellationToken);
    }

    public ElectionService(
        IElectionRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
        : base(repository, unitOfWork, mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public override async Task<ElectionDto> CreateAsync(
        CreateElectionDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BusinessRuleException(
                "El nombre de la elección es obligatorio.");
        }

        if (dto.StartsAtUtc == default)
        {
            throw new BusinessRuleException(
                "La fecha de realización de la elección es obligatoria.");
        }

        var readiness = await _repository.GetCreationReadinessAsync(
            cancellationToken);

        if (readiness.HasActiveElection)
        {
            throw new BusinessRuleException(
                "Ya existe una elección activa. No se puede iniciar otra.");
        }

        if (readiness.ActiveElectivePositionCount == 0)
        {
            throw new BusinessRuleException(
                "No hay puestos electivos activos para realizar una elección.");
        }

        if (readiness.ActivePoliticalPartyCount < 2)
        {
            throw new BusinessRuleException(
                "No hay suficientes partidos políticos para realizar una elección.");
        }

        if (readiness.PartiesMissingPositions.Count > 0)
        {
            var messages = readiness.PartiesMissingPositions.Select(party =>
                $"El partido político {party.PoliticalPartyName} " +
                $"{party.PoliticalPartyAcronym} no tiene candidatos registrados " +
                "para los siguientes puestos electivos: " +
                $"{string.Join(", ", party.MissingPositionNames)}.");

            throw new BusinessRuleException(
                string.Join(Environment.NewLine, messages));
        }

        dto.Name = dto.Name.Trim();

        var election = _mapper.Map<Election>(dto);
        election.Status = ElectionStatus.Active;
        election.IsActive = true;
        election.Year = dto.Year;

        await _repository.AddAsync(election, cancellationToken);

        await _repository.AddCandidateSnapshotsAsync(
            election,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ElectionDto>(election);
    }

    public override async Task<ElectionDto?> UpdateAsync(
        int id,
        UpdateElectionDto dto,
        CancellationToken cancellationToken = default)
    {
        var election = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (election is null)
        {
            return null;
        }

        if (election.Status != ElectionStatus.Configuring)
        {
            throw new BusinessRuleException(
                "No se puede editar una elección que ya está en proceso o finalizada.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new BusinessRuleException(
                "El nombre de la elección es obligatorio.");
        }

        if (dto.StartsAtUtc == default)
        {
            throw new BusinessRuleException(
                "La fecha de realización de la elección es obligatoria.");
        }

        dto.Name = dto.Name.Trim();
        dto.Year = dto.StartsAtUtc.ToLocalTime().Year;

        return await base.UpdateAsync(id, dto, cancellationToken);
    }

    public override Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessRuleException(
            "El estado de una elección se controla mediante su creación y finalización.");
    }

    public async Task<bool> CompleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var election = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (election is null)
        {
            return false;
        }

        if (election.Status != ElectionStatus.Active)
        {
            throw new BusinessRuleException(
                "Solo se puede finalizar una elección que esté en proceso.");
        }

        election.Status = ElectionStatus.Completed;
        election.EndsAtUtc = DateTime.UtcNow;

        _repository.Update(election);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;




    }
}