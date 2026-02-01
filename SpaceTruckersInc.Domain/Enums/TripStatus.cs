using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class TripStatus : SmartEnum<TripStatus, int>
{
    public static readonly TripStatus Cancelled = new(nameof(Cancelled), 4);
    public static readonly TripStatus Completed = new(nameof(Completed), 3);
    public static readonly TripStatus InProgress = new(nameof(InProgress), 2);
    public static readonly TripStatus Pending = new(nameof(Pending), 1);

    private TripStatus(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        return List.Select(l => l.Name).ToArray();
    }
}