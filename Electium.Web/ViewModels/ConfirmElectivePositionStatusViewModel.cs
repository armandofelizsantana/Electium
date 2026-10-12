namespace Electium.Web.ViewModels;

public class ConfirmElectivePositionStatusViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string ActionText => IsActive ? "activar" : "desactivar";

    public string ButtonText => IsActive
        ? "Sí, activar puesto"
        : "Sí, desactivar puesto";
}