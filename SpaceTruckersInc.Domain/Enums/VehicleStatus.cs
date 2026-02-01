using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class VehicleStatus : SmartEnum<VehicleStatus, int>
{
    public static readonly VehicleStatus Available = new(nameof(Available), 1);
    public static readonly VehicleStatus Maintenance = new(nameof(Maintenance), 3);
    public static readonly VehicleStatus OnTrip = new(nameof(OnTrip), 2);

    private VehicleStatus(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        return List.Select(l => l.Name).ToArray();
    }
}