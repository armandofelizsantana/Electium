using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Electium.Web.ViewModels;

public class CreatePoliticalPartyViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La sigla es obligatoria.")]
    [StringLength(20)]
    public string Acronym { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona el logo del partido.")]
    public IFormFile? LogoFile { get; set; }
}