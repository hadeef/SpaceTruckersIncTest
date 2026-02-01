using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class DriverStatus : SmartEnum<DriverStatus, int>
{
    public static readonly DriverStatus Available = new(nameof(Available), 1);
    public static readonly DriverStatus OnTrip = new(nameof(OnTrip), 2);

    private DriverStatus(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        return List.Select(l => l.Name).ToArray();
    }
}