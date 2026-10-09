using System.ComponentModel.DataAnnotations;

namespace Electium.Web.ViewModels;

public class EditElectivePositionViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del puesto es obligatorio.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripcion es obligatorio")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
}