using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace Electium.Infrastructure.Identity;

public class LeaderPartyAssignmentService : ILeaderPartyAssignmentService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPoliticalPartyRepository _partyRepository;
    private readonly IElectionRepository _electionRepository;

    public LeaderPartyAssignmentService(
        UserManager<ApplicationUser> userManager,
        IPoliticalPartyRepository partyRepository,
        IElectionRepository electionRepository)
    {
        _userManager = userManager;
        _partyRepository = partyRepository;
        _electionRepository = electionRepository;
    }

    public async Task<IReadOnlyList<LeaderPartyAssignmentDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var leaders = await _userManager.GetUsersInRoleAsync(
            RoleNames.PoliticalLeader);

        var parties = await _partyRepository.GetAllAsync(cancellationToken);
        var partiesById = parties.ToDictionary(party => party.Id);

        return leaders
            .Where(leader =>
                leader.PoliticalPartyId.HasValue &&
                partiesById.ContainsKey(leader.PoliticalPartyId.Value))
            .Select(leader =>
            {
                var party = partiesById[leader.PoliticalPartyId!.Value];

                return new LeaderPartyAssignmentDto
                {
                    LeaderUserId = leader.Id,
                    FirstName = leader.FirstName,
                    LastName = leader.LastName,
                    UserName = leader.UserName ?? string.Empty,
                    PartyId = party.Id,
                    PartyName = party.Name,
                    PartyAcronym = party.Acronym
                };
            })
            .OrderBy(item => item.UserName)
            .ToList();
    }

    public async Task<LeaderPartyAssignmentDto?> GetByLeaderIdAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default)
    {
        var leader = await _userManager.FindByIdAsync(
            leaderUserId.ToString());

        if (leader is null ||
            !await _userManager.IsInRoleAsync(
                leader,
                RoleNames.PoliticalLeader) ||
            !leader.PoliticalPartyId.HasValue)
        {
            return null;
        }

        var party = await _partyRepository.GetByIdAsync(
            leader.PoliticalPartyId.Value,
            cancellationToken);

        if (party is null)
        {
            return null;
        }

        return new LeaderPartyAssignmentDto
        {
            LeaderUserId = leader.Id,
            FirstName = leader.FirstName,
            LastName = leader.LastName,
            UserName = leader.UserName ?? string.Empty,
            PartyId = party.Id,
            PartyName = party.Name,
            PartyAcronym = party.Acronym
        };
    }

    public async Task<IReadOnlyList<AssignablePoliticalLeaderDto>>
        GetUnassignedActiveLeadersAsync(
            CancellationToken cancellationToken = default)
    {
        var leaders = await _userManager.GetUsersInRoleAsync(
            RoleNames.PoliticalLeader);

        return leaders
            .Where(leader =>
                leader.IsActive &&
                !leader.PoliticalPartyId.HasValue)
            .OrderBy(leader => leader.LastName)
            .ThenBy(leader => leader.FirstName)
            .Select(leader => new AssignablePoliticalLeaderDto
            {
                Id = leader.Id,
                DisplayName =
                    $"{leader.FirstName} {leader.LastName}",
                UserName = leader.UserName ?? string.Empty
            })
            .ToList();
    }

    public async Task<IReadOnlyList<AssignablePoliticalPartyDto>>
        GetActivePartiesAsync(
            CancellationToken cancellationToken = default)
    {
        var parties = await _partyRepository.GetActiveAsync(
            cancellationToken);

        return parties
            .OrderBy(party => party.Acronym)
            .Select(party => new AssignablePoliticalPartyDto
            {
                Id = party.Id,
                Name = party.Name,
                Acronym = party.Acronym
            })
            .ToList();
    }

    public async Task AssignAsync(
        CreateLeaderPartyAssignmentDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var leader = await _userManager.FindByIdAsync(
            dto.LeaderUserId.ToString());

        if (leader is null ||
            !await _userManager.IsInRoleAsync(
                leader,
                RoleNames.PoliticalLeader))
        {
            throw new BusinessRuleException(
                "Selecciona una cuenta con el rol de dirigente político.");
        }

        if (!leader.IsActive)
        {
            throw new BusinessRuleException(
                "Solo se puede asignar un dirigente activo.");
        }

        if (leader.PoliticalPartyId.HasValue)
        {
            throw new BusinessRuleException(
                "Este dirigente ya está relacionado con otro partido político.");
        }

        var party = await _partyRepository.GetByIdAsync(
            dto.PartyId,
            cancellationToken);

        if (party is null || !party.IsActive)
        {
            throw new BusinessRuleException(
                "Selecciona un partido político activo.");
        }

        leader.PoliticalPartyId = party.Id;

        var result = await _userManager.UpdateAsync(leader);

        if (!result.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join(
                    " ",
                    result.Errors.Select(error => error.Description)));
        }
    }

    public async Task<bool> RemoveAsync(
        int leaderUserId,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var leader = await _userManager.FindByIdAsync(
            leaderUserId.ToString());

        if (leader is null ||
            !await _userManager.IsInRoleAsync(
                leader,
                RoleNames.PoliticalLeader) ||
            !leader.PoliticalPartyId.HasValue)
        {
            return false;
        }

        leader.PoliticalPartyId = null;

        var result = await _userManager.UpdateAsync(leader);

        if (!result.Succeeded)
        {
            throw new BusinessRuleException(
                string.Join(
                    " ",
                    result.Errors.Select(error => error.Description)));
        }

        return true;
    }

    private async Task EnsureNoActiveElectionAsync(
        CancellationToken cancellationToken)
    {
        if (await _electionRepository.HasActiveElectionAsync(
                cancellationToken))
        {
            throw new BusinessRuleException(
                "No se pueden asignar ni desvincular dirigentes mientras haya una elección en proceso.");
        }
    }
}