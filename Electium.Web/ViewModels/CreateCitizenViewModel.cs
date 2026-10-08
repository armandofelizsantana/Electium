using System.ComponentModel.DataAnnotations;

namespace Electium.Web.ViewModels;

public class CreateCitizenViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(30)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Escribe un correo electrónico válido.")]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
    [StringLength(20)]
    public string IdentityNumber { get; set; } = string.Empty;
}