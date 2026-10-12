using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class CandidateDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public string? ElectivePositionName { get; set; }
    public bool IsActive { get; set; }
}