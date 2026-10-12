namespace Electium.Web.ViewModels;

public class ConfirmCandidateStatusViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}