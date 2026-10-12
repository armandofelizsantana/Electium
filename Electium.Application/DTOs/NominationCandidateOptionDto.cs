using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class NominationCandidateOptionDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string OriginPartyName { get; set; } = string.Empty;
}