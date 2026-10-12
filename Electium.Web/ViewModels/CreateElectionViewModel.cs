using System.ComponentModel.DataAnnotations;

namespace Electium.Web.ViewModels;

public class CreateElectionViewModel
{
    [Required(ErrorMessage = "El nombre de la elección es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Nombre de la elección")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de realización es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de realización")]
    public DateTime? ElectionDateLocal { get; set; }
}