using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;
using Electium.Application.Interfaces;
using Electium.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace Electium.Infrastructure.Identity;

public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityAuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<LoginResultDto> LoginAsync(
        string userName,
        string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user is null)
        {
            return new LoginResultDto
            {
                Status = LoginStatus.InvalidCredentials
            };
        }

        var passwordResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                password,
                lockoutOnFailure: true);

        if (!passwordResult.Succeeded)
        {
            return new LoginResultDto
            {
                Status = LoginStatus.InvalidCredentials
            };
        }

        if (!user.IsActive)
        {
            return new LoginResultDto
            {
                Status = LoginStatus.Inactive
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Count != 1)
        {
            return new LoginResultDto
            {
                Status = LoginStatus.UnauthorizedRole
            };
        }

        var roleName = roles[0];

        if (roleName == RoleNames.PoliticalLeader &&
            !user.PoliticalPartyId.HasValue)
        {
            return new LoginResultDto
            {
                Status = LoginStatus.PartyNotAssigned
            };
        }

        if (roleName != RoleNames.Administrator &&
            roleName != RoleNames.PoliticalLeader)
        {
            return new LoginResultDto
            {
                Status = LoginStatus.UnauthorizedRole
            };
        }

        await _signInManager.SignInAsync(
            user,
            isPersistent: false);

        return new LoginResultDto
        {
            Status = LoginStatus.Success,
            RoleName = roleName
        };
    }

    public Task LogoutAsync()
    {
        return _signInManager.SignOutAsync();
    }
}