using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Electium.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Electium.Infrastructure.Identity;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IElectionRepository _electionRepository;
    private readonly ElectiumDbContext _context;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        IElectionRepository electionRepository,
        ElectiumDbContext context)
    {
        _userManager = userManager;
        _electionRepository = electionRepository;
        _context = context;
    }

    public async Task<IReadOnlyList<SystemUserDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var administrators = await _userManager.GetUsersInRoleAsync(
            RoleNames.Administrator);

        var leaders = await _userManager.GetUsersInRoleAsync(
            RoleNames.PoliticalLeader);

        var users = administrators
            .Select(user => Map(user, RoleNames.Administrator))
            .Concat(leaders.Select(user => Map(user, RoleNames.PoliticalLeader)))
            .GroupBy(user => user.Id)
            .Select(group => group.First())
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToList();

        return users;
    }

    public async Task<SystemUserDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count != 1 || !IsAllowedRole(roles[0]))
        {
            return null;
        }

        return Map(user, roles[0]);
    }

    public async Task<SystemUserDto> CreateAsync(
        CreateSystemUserDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        dto.FirstName = dto.FirstName.Trim();
        dto.LastName = dto.LastName.Trim();
        dto.Email = dto.Email.Trim();
        dto.UserName = dto.UserName.Trim();

        ValidateRequiredFields(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.UserName,
            dto.RoleName);

        if (string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            throw new BusinessRuleException(
                "La contraseña y su confirmación son obligatorias.");
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            throw new BusinessRuleException(
                "La contraseña y su confirmación no coinciden.");
        }

        if (!IsAllowedRole(dto.RoleName))
        {
            throw new BusinessRuleException(
                "Selecciona un rol permitido.");
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

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.UserName,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,

            // La relación con el partido se administra en otra pantalla.
            PoliticalPartyId = null
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
            dto.RoleName);

        if (!roleResult.Succeeded)
        {
            throw new BusinessRuleException(
                FormatIdentityErrors(roleResult));
        }

        await transaction.CommitAsync(cancellationToken);

        return Map(user, dto.RoleName);
    }

    public async Task<SystemUserDto?> UpdateAsync(
        UpdateSystemUserDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        dto.FirstName = dto.FirstName.Trim();
        dto.LastName = dto.LastName.Trim();
        dto.Email = dto.Email.Trim();
        dto.UserName = dto.UserName.Trim();

        ValidateRequiredFields(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.UserName,
            dto.RoleName);

        if (!IsAllowedRole(dto.RoleName))
        {
            throw new BusinessRuleException(
                "Selecciona un rol permitido.");
        }

        var hasPassword = !string.IsNullOrWhiteSpace(dto.Password);

        if (hasPassword &&
            dto.Password != dto.ConfirmPassword)
        {
            throw new BusinessRuleException(
                "La contraseña y su confirmación no coinciden.");
        }

        if (!hasPassword &&
            !string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            throw new BusinessRuleException(
                "Escribe una contraseña antes de confirmarla.");
        }

        var user = await _userManager.FindByIdAsync(dto.Id.ToString());

        if (user is null)
        {
            return null;
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Count != 1 ||
            !IsAllowedRole(currentRoles[0]))
        {
            throw new BusinessRuleException(
                "No se puede editar este usuario porque no tiene un único rol permitido.");
        }

        var existingUserName =
            await _userManager.FindByNameAsync(dto.UserName);

        if (existingUserName is not null &&
            existingUserName.Id != user.Id)
        {
            throw new BusinessRuleException(
                "Ese nombre de usuario ya está registrado.");
        }

        var existingEmail =
            await _userManager.FindByEmailAsync(dto.Email);

        if (existingEmail is not null &&
            existingEmail.Id != user.Id)
        {
            throw new BusinessRuleException(
                "Ese correo electrónico ya está registrado.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.UserName = dto.UserName;

        if (dto.RoleName == RoleNames.Administrator)
        {
            // Un administrador no tiene partido asignado.
            user.PoliticalPartyId = null;
        }

        if (currentRoles[0] != dto.RoleName)
        {
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(
                user,
                currentRoles);

            if (!removeRolesResult.Succeeded)
            {
                throw new BusinessRuleException(
                    FormatIdentityErrors(removeRolesResult));
            }

            var addRoleResult = await _userManager.AddToRoleAsync(
                user,
                dto.RoleName);

            if (!addRoleResult.Succeeded)
            {
                throw new BusinessRuleException(
                    FormatIdentityErrors(addRoleResult));
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new BusinessRuleException(
                FormatIdentityErrors(updateResult));
        }

        if (hasPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.HasPasswordAsync(user)
                ? await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    dto.Password!)
                : await _userManager.AddPasswordAsync(
                    user,
                    dto.Password!);

            if (!passwordResult.Succeeded)
            {
                throw new BusinessRuleException(
                    FormatIdentityErrors(passwordResult));
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return Map(user, dto.RoleName);
    }

    public async Task<bool> SetActiveAsync(
        int id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureNoActiveElectionAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return false;
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count != 1 || !IsAllowedRole(roles[0]))
        {
            return false;
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
                "No se pueden crear, editar, activar ni desactivar usuarios mientras haya una elección en proceso.");
        }
    }

    private static void ValidateRequiredFields(
        string firstName,
        string lastName,
        string email,
        string userName,
        string roleName)
    {
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrWhiteSpace(roleName))
        {
            throw new BusinessRuleException(
                "Completa todos los campos obligatorios del usuario.");
        }
    }

    private static bool IsAllowedRole(string roleName)
    {
        return roleName == RoleNames.Administrator ||
               roleName == RoleNames.PoliticalLeader;
    }

    private static SystemUserDto Map(
        ApplicationUser user,
        string roleName)
    {
        return new SystemUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            RoleName = roleName,
            IsActive = user.IsActive
        };
    }

    private static string FormatIdentityErrors(IdentityResult result)
    {
        return string.Join(
            " ",
            result.Errors.Select(error => error.Description));
    }
}