using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.DTOs;

public class LeaderPartyAssignmentDto
{
    public int LeaderUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int PartyId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string PartyAcronym { get; set; } = string.Empty;
}

public class AssignablePoliticalLeaderDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}

public class AssignablePoliticalPartyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Acronym { get; set; } = string.Empty;
}

public class CreateLeaderPartyAssignmentDto
{
    public int LeaderUserId { get; set; }
    public int PartyId { get; set; }
}