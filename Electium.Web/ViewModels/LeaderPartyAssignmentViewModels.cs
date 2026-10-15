using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Electium.Web.ViewModels;

public class LeaderPartyAssignmentListViewModel
{
    public int LeaderUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string PartyAcronym { get; set; } = string.Empty;
}

public class CreateLeaderPartyAssignmentViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un dirigente.")]
    public int LeaderUserId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un partido.")]
    public int PartyId { get; set; }

    public IEnumerable<SelectListItem> AvailableLeaders { get; set; }
        = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> ActiveParties { get; set; }
        = Enumerable.Empty<SelectListItem>();
}

public class ConfirmRemoveLeaderPartyAssignmentViewModel
{
    public int LeaderUserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string PartyAcronym { get; set; } = string.Empty;
}