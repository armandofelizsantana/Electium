using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Enums;

namespace Electium.Application.DTOs;

public class ElectionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public DateTime? EndsAtUtc { get; set; }

    public ElectionStatus Status { get; set; }

    public bool IsActive { get; set; }
}