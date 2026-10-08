namespace Electium.Web.ViewModels;

public class CitizenListViewModel
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string IdentityNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}