using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Electium.Web.ViewModels;

public class NominationIndexViewModel
{
    public bool HasActiveElection { get; set; }

    public List<NominationRowViewModel> Nominations { get; set; } = [];
}

public class NominationRowViewModel
{
    public int Id { get; set; }
    public string CandidateFullName { get; set; } = string.Empty;
    public string CandidatePhotoPath { get; set; } = string.Empty;
    public string CandidatePartyName { get; set; } = string.Empty;
    public string ElectivePositionName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateNominationViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un puesto.")]
    public int ElectivePositionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un candidato.")]
    public int CandidateId { get; set; }

    public List<SelectListItem> ActivePositions { get; set; } = [];
}

public class ConfirmCreateNominationViewModel
{
    public int ElectivePositionId { get; set; }
    public int CandidateId { get; set; }
    public string ElectivePositionName { get; set; } = string.Empty;
    public string CandidateFullName { get; set; } = string.Empty;
    public string CandidatePartyName { get; set; } = string.Empty;
}

public class ConfirmDeactivateNominationViewModel
{
    public int Id { get; set; }
    public string ElectivePositionName { get; set; } = string.Empty;
    public string CandidateFullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}