using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class CitizenDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string IdentityNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}