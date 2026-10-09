using System;
using System.Collections.Generic;
using System.Text;
using Electium.Application.Security;

namespace Electium.Application.DTOs;

public class LoginResultDto
{
    public LoginStatus Status { get; set; }

    public string? RoleName { get; set; }
}