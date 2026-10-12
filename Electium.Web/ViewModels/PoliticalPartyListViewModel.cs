namespace Electium.Web.ViewModels;

public class PoliticalPartyListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Acronym { get; set; } = string.Empty;
    public string LogoPath { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}