using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Electium.Web.ViewModels;

public class UploadIdentityViewModel
{
    [Required(ErrorMessage = "Selecciona una imagen de la parte frontal de tu cédula.")]
    public IFormFile? FrontImage { get; set; }

    public string? Message { get; set; }
}