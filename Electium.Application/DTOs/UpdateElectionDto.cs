using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class UpdateElectionDto
{
    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public DateTime? EndsAtUtc { get; set; }
}