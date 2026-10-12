using Electium.Domain.Enums;

namespace Electium.Web.ViewModels;

public class ElectionListViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime ElectionDateLocal { get; set; }

    public ElectionStatus Status { get; set; }

    public bool IsActive { get; set; }

    public int PoliticalPartyCount { get; set; }

    public int ElectivePositionCount { get; set; }
}