using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class LeaderNominationService : ILeaderNominationService
{
    private readonly ILeaderNominationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public LeaderNominationService(
        ILeaderNominationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public Task<bool> HasActiveElectionAsync(
        CancellationToken cancellationToken = default)
    {
        return _repository.HasActiveElectionAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LeaderNominationDto>> GetAllAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default)
    {
        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        return await _repository.GetNominationsByPartyAsync(
            partyId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<NominationPositionOptionDto>>
        GetAvailablePositionsAsync(
            int leaderUserId,
            CancellationToken cancellationToken = default)
    {
        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        return await _repository.GetAvailablePositionsAsync(
            partyId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<NominationCandidateOptionDto>>
        GetAvailableCandidatesAsync(
            int leaderUserId,
            int electivePositionId,
            CancellationToken cancellationToken = default)
    {
        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        return await _repository.GetAvailableCandidatesAsync(
            partyId,
            electivePositionId,
            cancellationToken);
    }

    public async Task CreateAsync(
        int leaderUserId,
        int candidateId,
        int electivePositionId,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var candidate = await _repository.GetActiveCandidateAsync(
            candidateId,
            cancellationToken);

        if (candidate is null)
        {
            throw new BusinessRuleException(
                "El candidato no existe o no está activo.");
        }

        var position = await _repository.GetActivePositionAsync(
            electivePositionId,
            cancellationToken);

        if (position is null)
        {
            throw new BusinessRuleException(
                "El puesto no existe o no está activo.");
        }

        if (await _repository.HasActiveNominationForPositionAsync(
                partyId,
                electivePositionId,
                cancellationToken))
        {
            throw new BusinessRuleException(
                "Tu partido ya tiene un candidato asignado a ese puesto.");
        }

        if (await _repository.HasActiveNominationForCandidateAsync(
                partyId,
                candidateId,
                cancellationToken))
        {
            throw new BusinessRuleException(
                "Ese candidato ya representa a tu partido en otro puesto.");
        }

        if (candidate.PoliticalPartyId != partyId)
        {
            var partiesAreAllied = await _repository.ArePartiesAlliedAsync(
                partyId,
                candidate.PoliticalPartyId,
                cancellationToken);

            if (!partiesAreAllied)
            {
                throw new BusinessRuleException(
                    "Solo puedes asignar candidatos de tu partido o de un partido aliado.");
            }

            var originHasNomination =
                await _repository.HasOriginPartyNominationAsync(
                    candidate.PoliticalPartyId,
                    candidateId,
                    electivePositionId,
                    cancellationToken);

            if (!originHasNomination)
            {
                throw new BusinessRuleException(
                    "El partido de origen debe tener postulado a ese candidato para el mismo puesto.");
            }
        }

        await _repository.AddNominationAsync(
            new CandidateNomination
            {
                CandidateId = candidateId,
                PoliticalPartyId = partyId,
                ElectivePositionId = electivePositionId,
                IsActive = true
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeactivateAsync(
        int leaderUserId,
        int nominationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var nomination = await _repository.GetNominationForPartyAsync(
            nominationId,
            partyId,
            cancellationToken);

        if (nomination is null || !nomination.IsActive)
        {
            return false;
        }

        
        nomination.IsActive = false;

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
                "No se pueden modificar las asignaciones mientras haya una elección activa.");
        }
    }
}