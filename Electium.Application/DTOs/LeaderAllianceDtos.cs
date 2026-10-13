using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Enums;

namespace Electium.Application.DTOs;

public class LeaderAllianceDashboardDto
{
    public IReadOnlyList<IncomingAllianceRequestDto> IncomingRequests
    { get; set; } = Array.Empty<IncomingAllianceRequestDto>();

    public IReadOnlyList<OutgoingAllianceRequestDto> OutgoingRequests
    { get; set; } = Array.Empty<OutgoingAllianceRequestDto>();

    public IReadOnlyList<ActiveAllianceDto> ActiveAlliances
    { get; set; } = Array.Empty<ActiveAllianceDto>();
}

public class IncomingAllianceRequestDto
{
    public int Id { get; set; }

    public string RequestingPartyName { get; set; } = string.Empty;

    public string RequestingPartyAcronym { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }
}

public class OutgoingAllianceRequestDto
{
    public int Id { get; set; }

    public string ReceivingPartyName { get; set; } = string.Empty;

    public string ReceivingPartyAcronym { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    public AllianceRequestStatus Status { get; set; }
}

public class ActiveAllianceDto
{
    public int Id { get; set; }

    public string AlliedPartyName { get; set; } = string.Empty;

    public string AlliedPartyAcronym { get; set; } = string.Empty;

    public DateTime AcceptedAtUtc { get; set; }
}

public class AvailableAlliancePartyDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Acronym { get; set; } = string.Empty;
}