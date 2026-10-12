using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Electium.Web.ViewModels;

public class EditCandidateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public string CurrentPhotoPath { get; set; } = string.Empty;

    // Es opcional durante la edición.
    public IFormFile? PhotoFile { get; set; }
}