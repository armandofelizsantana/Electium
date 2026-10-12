namespace Electium.Web.ViewModels;

public class PoliticalLeaderListViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PoliticalPartyName { get; set; }
    public bool IsActive { get; set; }
}