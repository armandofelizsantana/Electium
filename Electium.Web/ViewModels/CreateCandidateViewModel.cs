using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Electium.Web.ViewModels;

public class CreateCandidateViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La foto del candidato es obligatoria.")]
    public IFormFile? PhotoFile { get; set; }
}