using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class UpdatePoliticalPartyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Acronym { get; set; } = string.Empty;

    public string? LogoPath { get; set; }
}