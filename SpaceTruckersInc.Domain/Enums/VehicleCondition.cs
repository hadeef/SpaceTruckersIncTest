using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class VehicleCondition : SmartEnum<VehicleCondition, int>
{
    public static readonly VehicleCondition Damaged = new(nameof(Damaged), 2);
    public static readonly VehicleCondition Functional = new(nameof(Functional), 1);

    private VehicleCondition(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        return List.Select(l => l.Name).ToArray();
    }
}