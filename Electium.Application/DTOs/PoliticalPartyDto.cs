using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Entities;

namespace Electium.Application.DTOs;

public class PoliticalPartyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Acronym { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}