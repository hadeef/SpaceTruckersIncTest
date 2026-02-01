using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class LicenseLevel : SmartEnum<LicenseLevel, int>
{
    public static readonly LicenseLevel Rookie = new(nameof(Rookie), 1);
    public static readonly LicenseLevel Veteran = new(nameof(Veteran), 2);

    private LicenseLevel(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        // Use SmartEnum's built-in list of instances (LicenseLevel.List) and return their names.
        return List.Select(l => l.Name).ToArray();
    }
}