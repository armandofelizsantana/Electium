using Electium.Domain.Enums;

namespace Electium.Web.ViewModels;

public class ElectionListViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartsAtLocal { get; set; }

    public DateTime? EndsAtLocal { get; set; }

    public ElectionStatus Status { get; set; }

    public bool IsActive { get; set; }
}