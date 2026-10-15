using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class UpdateSystemUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;

    // En una edición, la contraseña es opcional.
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}