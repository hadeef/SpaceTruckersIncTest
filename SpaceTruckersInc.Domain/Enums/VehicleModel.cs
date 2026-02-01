using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class VehicleModel : SmartEnum<VehicleModel, int>
{
    public static readonly VehicleModel HoverTruck = new(nameof(HoverTruck), 1);
    public static readonly VehicleModel RocketVan = new(nameof(RocketVan), 2);

    private VehicleModel(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        return List.Select(l => l.Name).ToArray();
    }
}