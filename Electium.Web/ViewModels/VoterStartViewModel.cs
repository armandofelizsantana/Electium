using System.ComponentModel.DataAnnotations;

namespace Electium.Web.ViewModels;

public class VoterStartViewModel
{
    [Required(ErrorMessage = "Ingresa tu número de documento.")]
    [StringLength(30)]
    public string IdentityNumber { get; set; } = string.Empty;

    public string? Message { get; set; }

    public bool IsError { get; set; }
}