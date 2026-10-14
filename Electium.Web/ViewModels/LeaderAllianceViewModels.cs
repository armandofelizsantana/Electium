using System.ComponentModel.DataAnnotations;
using Electium.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Electium.Web.ViewModels;

public class LeaderAllianceDashboardViewModel
{
    public IReadOnlyList<IncomingAllianceRequestViewModel> IncomingRequests
    { get; set; } = Array.Empty<IncomingAllianceRequestViewModel>();

    public IReadOnlyList<OutgoingAllianceRequestViewModel> OutgoingRequests
    { get; set; } = Array.Empty<OutgoingAllianceRequestViewModel>();

    public IReadOnlyList<ActiveAllianceViewModel> ActiveAlliances
    { get; set; } = Array.Empty<ActiveAllianceViewModel>();
}

public class IncomingAllianceRequestViewModel
{
    public int Id { get; set; }

    public string RequestingPartyName { get; set; } = string.Empty;

    public string RequestingPartyAcronym { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }
}

public class OutgoingAllianceRequestViewModel
{
    public int Id { get; set; }

    public string ReceivingPartyName { get; set; } = string.Empty;

    public string ReceivingPartyAcronym { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    public AllianceRequestStatus Status { get; set; }
}

public class ActiveAllianceViewModel
{
    public int Id { get; set; }

    public string AlliedPartyName { get; set; } = string.Empty;

    public string AlliedPartyAcronym { get; set; } = string.Empty;

    public DateTime AcceptedAtUtc { get; set; }
}

public class CreateAllianceRequestViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un partido político.")]
    public int ReceivingPartyId { get; set; }

    public List<SelectListItem> AvailableParties { get; set; } = [];
}

public class ConfirmAllianceResponseViewModel
{
    public int Id { get; set; }

    public bool Accept { get; set; }

    public string OtherPartyName { get; set; } = string.Empty;

    public string OtherPartyAcronym { get; set; } = string.Empty;
}

public class ConfirmDeleteAllianceRequestViewModel
{
    public int Id { get; set; }

    public string ReceivingPartyName { get; set; } = string.Empty;

    public string ReceivingPartyAcronym { get; set; } = string.Empty;
}