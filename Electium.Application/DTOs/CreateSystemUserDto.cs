using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class CreateSystemUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}