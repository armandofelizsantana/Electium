using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Domain.Entities;

namespace Electium.Application.Services;

public class VoterBallotService : IVoterBallotService
{
    private readonly IVoterBallotRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IVoteReceiptEmailSender _emailSender;

    public VoterBallotService(
        IVoterBallotRepository repository,
        IUnitOfWork unitOfWork,
        IVoteReceiptEmailSender emailSender)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }
    public VoterBallotService(
        IVoterBallotRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public Task<VoterBallotDto?> GetActiveBallotAsync(
        int electionId,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetActiveBallotAsync(
            electionId,
            cancellationToken);
    }

    public async Task<VoteReceiptDto> CastVoteAsync(
        int electionId,
        int citizenId,
        IReadOnlyCollection<CastVoteSelectionDto> selections,
        CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0)
        {
            throw new BusinessRuleException(
                "No se pudo identificar al ciudadano.");
        }

        var recipientEmail = await _repository.GetCitizenEmailAsync(
    citizenId,
    cancellationToken);

        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new BusinessRuleException(
                "El ciudadano no tiene un correo electrónico válido registrado.");
        }

        var ballotData = await _repository.GetActiveBallotAsync(
            electionId,
            cancellationToken);

        if (ballotData is null)
        {
            throw new BusinessRuleException(
                "La elección ya no está activa. No se puede registrar el voto.");
        }

        if (ballotData.Positions.Count == 0)
        {
            throw new BusinessRuleException(
                "La elección no tiene puestos disponibles para votar.");
        }

        if (await _repository.HasParticipatedAsync(
                electionId,
                citizenId,
                cancellationToken))
        {
            throw new BusinessRuleException(
                "Ya has ejercido tu derecho al voto en esta elección.");
        }

        var positionsById = ballotData.Positions
            .ToDictionary(position => position.ElectivePositionId);

        if (selections.Count != positionsById.Count)
        {
            throw new BusinessRuleException(
                "Debes seleccionar una opción para cada puesto electivo.");
        }

        var selectedPositionIds = selections
            .Select(selection => selection.ElectivePositionId)
            .ToList();

        if (selectedPositionIds.Distinct().Count() != selections.Count ||
            !positionsById.Keys.ToHashSet().SetEquals(selectedPositionIds))
        {
            throw new BusinessRuleException(
                "La boleta contiene puestos duplicados o puestos que no pertenecen a esta elección.");
        }

        var receiptLines = new List<string>();
        var voteSelections = new List<VoteSelection>();

        foreach (var selection in selections)
        {
            var position = positionsById[selection.ElectivePositionId];

            if (selection.IsNone)
            {
                if (selection.ElectionCandidateId.HasValue)
                {
                    throw new BusinessRuleException(
                        "La opción Ninguno no debe incluir un candidato.");
                }

                receiptLines.Add($"{position.PositionName}: Ninguno");
            }
            else
            {
                if (!selection.ElectionCandidateId.HasValue)
                {
                    throw new BusinessRuleException(
                        $"Debes seleccionar un candidato para {position.PositionName} o elegir Ninguno.");
                }

                var candidate = position.Candidates.FirstOrDefault(item =>
                    item.ElectionCandidateId ==
                    selection.ElectionCandidateId.Value);

                if (candidate is null)
                {
                    throw new BusinessRuleException(
                        $"El candidato seleccionado no participa en el puesto {position.PositionName}.");
                }

                receiptLines.Add(
                    $"{position.PositionName}: {candidate.FirstName} " +
                    $"{candidate.LastName} — {candidate.PartyName} " +
                    $"({candidate.PartyAcronym})");
            }

            voteSelections.Add(new VoteSelection
            {
                ElectivePositionId = selection.ElectivePositionId,
                ElectionCandidateId = selection.IsNone
                    ? null
                    : selection.ElectionCandidateId,
                IsNone = selection.IsNone,
                IsActive = true
            });
        }

        var ballot = new Ballot
        {
            ElectionId = electionId,
            IsActive = true,
            SubmittedAtUtc = DateTime.UtcNow
        };

        foreach (var selection in voteSelections)
        {
            selection.Ballot = ballot;
        }

        var participation = new ElectionParticipation
        {
            ElectionId = electionId,
            CitizenId = citizenId,
            IsActive = true,
            ParticipatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddSubmittedBallotAsync(
            ballot,
            voteSelections,
            participation,
            cancellationToken);

       
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var receipt = new VoteReceiptDto
        {
            ElectionId = electionId,
            ElectionName = ballotData.ElectionName,
            SubmittedAtUtc = ballot.SubmittedAtUtc,
            SelectionSummary = receiptLines
        };

        receipt.EmailSent = await _emailSender.SendAsync(
            recipientEmail,
            receipt,
            cancellationToken);

        return receipt;
    }
}