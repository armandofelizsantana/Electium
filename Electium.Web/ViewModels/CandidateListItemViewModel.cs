namespace Electium.Web.ViewModels;

public class CandidateListItemViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public string? ElectivePositionName { get; set; }
    public bool IsActive { get; set; }
}