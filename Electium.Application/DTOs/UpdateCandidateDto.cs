using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class UpdateCandidateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Null significa que se conserva la foto actual.
    public string? PhotoPath { get; set; }
}