using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class LeaderCandidateService : ILeaderCandidateService
{
    private readonly ILeaderCandidateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LeaderCandidateService(
        ILeaderCandidateRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public Task<bool> HasActiveElectionAsync(
    CancellationToken cancellationToken = default)
    {
        return _repository.HasActiveElectionAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CandidateDto>> GetAllForLeaderAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default)
    {
        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var candidates = await _repository.GetByPartyAsync(
            partyId,
            cancellationToken);

        var positions = await _repository.GetPositionNamesByCandidateAsync(
            partyId,
            cancellationToken);

        return candidates.Select(candidate =>
        {
            var dto = _mapper.Map<CandidateDto>(candidate);

            dto.ElectivePositionName =
                positions.TryGetValue(candidate.Id, out var positionName)
                    ? positionName
                    : null;

            return dto;
        }).ToList();
    }

    public async Task<CandidateDto?> GetForLeaderAsync(
        int leaderUserId,
        int candidateId,
        CancellationToken cancellationToken = default)
    {
        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var candidate = await _repository.GetTrackedByIdAndPartyAsync(
            candidateId,
            partyId,
            cancellationToken);

        return candidate is null
            ? null
            : _mapper.Map<CandidateDto>(candidate);
    }

    public async Task<CandidateDto> CreateAsync(
        int leaderUserId,
        CreateCandidateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        dto.FirstName = dto.FirstName.Trim();
        dto.LastName = dto.LastName.Trim();

        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.PhotoPath))
        {
            throw new BusinessRuleException(
                "El nombre, el apellido y la foto del candidato son obligatorios.");
        }

        var candidate = _mapper.Map<Candidate>(dto);
        candidate.PoliticalPartyId = partyId;
        candidate.IsActive = true;

        await _repository.AddAsync(candidate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CandidateDto>(candidate);
    }

    public async Task<CandidateDto?> UpdateAsync(
        int leaderUserId,
        int candidateId,
        UpdateCandidateDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var candidate = await _repository.GetTrackedByIdAndPartyAsync(
            candidateId,
            partyId,
            cancellationToken);

        if (candidate is null)
        {
            return null;
        }

        dto.FirstName = dto.FirstName.Trim();
        dto.LastName = dto.LastName.Trim();

        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName))
        {
            throw new BusinessRuleException(
                "El nombre y el apellido del candidato son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(dto.PhotoPath))
        {
            dto.PhotoPath = candidate.PhotoPath;
        }

        _mapper.Map(dto, candidate);
        _repository.Update(candidate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CandidateDto>(candidate);
    }

    public async Task<bool> SetActiveAsync(
        int leaderUserId,
        int candidateId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var candidate = await _repository.GetTrackedByIdAndPartyAsync(
            candidateId,
            partyId,
            cancellationToken);

        if (candidate is null)
        {
            return false;
        }

        if (!isActive)
        {
            await _repository.DeactivateNominationsAsync(
                candidateId,
                partyId,
                cancellationToken);
        }

        candidate.IsActive = isActive;
        _repository.Update(candidate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<int> GetLeaderPartyIdAsync(
        int leaderUserId,
        CancellationToken cancellationToken)
    {
        var partyId = await _repository.GetActivePartyIdForLeaderAsync(
            leaderUserId,
            cancellationToken);

        if (!partyId.HasValue)
        {
            throw new BusinessRuleException(
                "Tu cuenta no tiene asignado un partido activo.");
        }

        return partyId.Value;
    }

    private async Task EnsureNoActiveElectionAsync(
        CancellationToken cancellationToken)
    {
        if (await _repository.HasActiveElectionAsync(cancellationToken))
        {
            throw new BusinessRuleException(
                "No se pueden crear, editar, activar ni desactivar candidatos mientras haya una elección activa.");
        }
    }
}