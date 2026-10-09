using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResultDto> LoginAsync(
        string userName,
        string password);

    Task LogoutAsync();
}