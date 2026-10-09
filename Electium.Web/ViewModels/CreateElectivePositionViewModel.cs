using System.ComponentModel.DataAnnotations;

namespace Electium.Web.ViewModels;

public class CreateElectivePositionViewModel
{
    [Required(ErrorMessage = "El nombre del puesto es obligatorio.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}