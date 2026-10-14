using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class VoteReceiptDto
{
    public int ElectionId { get; set; }

    public string ElectionName { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public IReadOnlyList<string> SelectionSummary { get; set; }
        = Array.Empty<string>();

    public bool EmailSent { get; set; }
}