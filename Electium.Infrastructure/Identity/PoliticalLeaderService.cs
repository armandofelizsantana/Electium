using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace Electium.Infrastructure.Identity;

public class PoliticalLeaderService : IPoliticalLeaderService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPoliticalPartyRepository _partyRepository;
    private readonly IElectionRepository _electionRepository;

    public PoliticalLeaderService(
        UserManager<ApplicationUser> userManager,
        IPoliticalPartyRepository partyRepository,
        IElectionRepository electionRepository)
    {
        _userManager = userManager;
        _partyRepository = partyRepository;
        _electionRepository = electionRepository;
    }

    public async Task<IReadOnlyList<PoliticalLeaderDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(
            RoleNames.PoliticalLeader);

        var parties = await _partyRepository.GetAllAsync(cancellationToken);
        var partyNames = parties.ToDictionary(
            party => party.Id,
            party => party.Name);

        return users
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .Select(user => new PoliticalLeaderDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PoliticalPartyId = user.PoliticalPartyId,
                PoliticalPartyName =
                    user.PoliticalPartyId is int partyId &&
                    partyNames.TryGetValue(partyId, out var partyName)
                        ? partyName
                        : null,
                IsActive = user.IsActive
            })
            .ToList();
    }

    public async Task<PoliticalLeaderDto> CreateAsync(
        CreatePoliticalLeaderDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        dto.FirstName = dto.FirstName.Trim();
        dto.LastName = dto.LastName.Trim();
        dto.UserName = dto.UserName.Trim();
        dto.Email = dto.Email.Trim();

        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.UserName) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new BusinessRuleException(
                "Completa todos los datos obligatorios del dirigente.");
        }

        var party = await _partyRepository.GetByIdAsync(
            dto.PoliticalPartyId,
            cancellationToken);

        if (party is null || !party.IsActive)
        {
            throw new BusinessRuleException(
                "Selecciona un partido político activo.");
        }

        if (await _userManager.FindByNameAsync(dto.UserName) is not null)
        {
            throw new BusinessRuleException(
                "Ese nombre de usuario ya está registrado.");
        }

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
        {
            throw new BusinessRuleException(
                "Ese correo electrónico ya está registrado.");
        }

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            PoliticalPartyId = party.Id
        };

        var createResult = await _userManager.CreateAsync(
            user,
            dto.Password);

        if (!createResult.Succeeded)
        {
            throw new BusinessRuleException(
                FormatIdentityErrors(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            RoleNames.PoliticalLeader);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            throw new BusinessRuleException(
                "La cuenta se creó, pero no se pudo asignar el rol de dirigente: " +
                FormatIdentityErrors(roleResult));
        }

        return new PoliticalLeaderDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PoliticalPartyId = party.Id,
            PoliticalPartyName = party.Name,
            IsActive = user.IsActive
        };
    }

    public async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null ||
            !await _userManager.IsInRoleAsync(
                user,
                RoleNames.PoliticalLeader))
        {
            return false;
        }

        if (isActive)
        {
            if (user.PoliticalPartyId is null)
            {
                throw new BusinessRuleException(
                    "No se puede activar un dirigente sin partido asignado.");
            }

            var party = await _partyRepository.GetByIdAsync(
                user.PoliticalPartyId.Value,
                cancellationToken);

            if (party is null || !party.IsActive)
            {
                throw new BusinessRuleException(
                    "Activa primero el partido asignado al dirigente.");
            }
        }

        user.IsActive = isActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new BusinessRuleException(FormatIdentityErrors(result));
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
                "No se pueden crear, activar ni desactivar dirigentes mientras haya una elección en proceso.");
        }
    }

    private static string FormatIdentityErrors(IdentityResult result)
    {
        return string.Join(
            " ",
            result.Errors.Select(error => error.Description));
    }
}