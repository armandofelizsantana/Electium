using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;
using Electium.Domain.Enums;

namespace Electium.Application.Services;

public class LeaderAllianceService : ILeaderAllianceService
{
    private readonly ILeaderAllianceRepository _repository;
    private readonly IElectionRepository _electionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LeaderAllianceService(
        ILeaderAllianceRepository repository,
        IElectionRepository electionRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _electionRepository = electionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaderAllianceDashboardDto> GetDashboardAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default)
    {
        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        return new LeaderAllianceDashboardDto
        {
            IncomingRequests =
                await _repository.GetIncomingRequestsAsync(
                    partyId,
                    cancellationToken),

            OutgoingRequests =
                await _repository.GetOutgoingRequestsAsync(
                    partyId,
                    cancellationToken),

            ActiveAlliances =
                await _repository.GetActiveAlliancesAsync(
                    partyId,
                    cancellationToken)
        };
    }

    public async Task<IReadOnlyList<AvailableAlliancePartyDto>>
        GetAvailablePartiesAsync(
            int leaderUserId,
            CancellationToken cancellationToken = default)
    {
        var partyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        return await _repository.GetAvailablePartiesAsync(
            partyId,
            cancellationToken);
    }

    public async Task CreateRequestAsync(
        int leaderUserId,
        int receivingPartyId,
        CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

        var requestingPartyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        if (requestingPartyId == receivingPartyId)
        {
            throw new BusinessRuleException(
                "No puedes enviar una solicitud de alianza a tu propio partido.");
        }

        if (!await _repository.IsPartyActiveAsync(
                receivingPartyId,
                cancellationToken))
        {
            throw new BusinessRuleException(
                "El partido seleccionado no existe o está inactivo.");
        }

        if (await _repository.HasPendingRequestBetweenPartiesAsync(
                requestingPartyId,
                receivingPartyId,
                cancellationToken))
        {
            throw new BusinessRuleException(
                "Ya existe una solicitud de alianza pendiente entre esos partidos.");
        }

        if (await _repository.HasActiveAllianceBetweenPartiesAsync(
                requestingPartyId,
                receivingPartyId,
                cancellationToken))
        {
            throw new BusinessRuleException(
                "Ya existe una alianza activa entre esos partidos.");
        }

        await _repository.AddRequestAsync(
            new AllianceRequest
            {
                RequestingPartyId = requestingPartyId,
                ReceivingPartyId = receivingPartyId,
                Status = AllianceRequestStatus.Pending,
                RequestedAtUtc = DateTime.UtcNow,
                IsActive = true
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RespondToRequestAsync(
        int leaderUserId,
        int requestId,
        bool accept,
        CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

        var receivingPartyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var request = await _repository.GetPendingIncomingRequestAsync(
            requestId,
            receivingPartyId,
            cancellationToken);

        if (request is null)
        {
            throw new BusinessRuleException(
                "La solicitud ya fue respondida o no está dirigida a tu partido.");
        }

        if (!await _repository.IsPartyActiveAsync(
                request.RequestingPartyId,
                cancellationToken))
        {
            throw new BusinessRuleException(
                "El partido solicitante está inactivo. No se puede responder la solicitud.");
        }

        request.Status = accept
            ? AllianceRequestStatus.Accepted
            : AllianceRequestStatus.Rejected;
        request.RespondedAtUtc = DateTime.UtcNow;

        if (accept &&
            !await _repository.HasActiveAllianceBetweenPartiesAsync(
                request.RequestingPartyId,
                request.ReceivingPartyId,
                cancellationToken))
        {
            var firstPartyId = Math.Min(
                request.RequestingPartyId,
                request.ReceivingPartyId);

            var secondPartyId = Math.Max(
                request.RequestingPartyId,
                request.ReceivingPartyId);

            await _repository.AddAllianceAsync(
                new PoliticalAlliance
                {
                    FirstPartyId = firstPartyId,
                    SecondPartyId = secondPartyId,
                    AcceptedAtUtc = DateTime.UtcNow,
                    IsActive = true
                },
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteOutgoingRequestAsync(
    int leaderUserId,
    int requestId,
    CancellationToken cancellationToken = default)
    {
        await EnsureThereIsNoActiveElectionAsync(cancellationToken);

        var requestingPartyId = await GetLeaderPartyIdAsync(
            leaderUserId,
            cancellationToken);

        var request = await _repository.GetOutgoingRequestAsync(
            requestId,
            requestingPartyId,
            cancellationToken);

        if (request is null)
        {
            return false;
        }

        if (request.Status == AllianceRequestStatus.Accepted)
        {
            throw new BusinessRuleException(
                "La solicitud ya fue aceptada y no se puede retirar. " +
                "La alianza continúa vigente.");
        }

       
        request.IsActive = false;

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

    private async Task EnsureThereIsNoActiveElectionAsync(
        CancellationToken cancellationToken)
    {
        if (await _electionRepository.HasActiveElectionAsync(
                cancellationToken))
        {
            throw new BusinessRuleException(
                "No se pueden modificar las alianzas mientras haya una elección activa.");
        }
    }
}