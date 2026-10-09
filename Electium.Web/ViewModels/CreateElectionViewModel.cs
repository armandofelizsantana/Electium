using System.ComponentModel.DataAnnotations;

namespace Electium.Web.ViewModels;

public class CreateElectionViewModel
{
    [Required(ErrorMessage = "El nombre de la elección es obligatorio.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(1900, 9999, ErrorMessage = "Ingresa un año válido.")]
    public int Year { get; set; }

    [Required(ErrorMessage = "La fecha y hora de inicio son obligatorias.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Inicio (hora local)")]
    public DateTime? StartsAtLocal { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Finalización (hora local)")]
    public DateTime? EndsAtLocal { get; set; }
}