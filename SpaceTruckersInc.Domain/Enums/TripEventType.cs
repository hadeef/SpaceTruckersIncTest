using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class TripEventType : SmartEnum<TripEventType, int>
{
    public static readonly TripEventType AsteroidFieldEncountered = new(nameof(AsteroidFieldEncountered), 4);
    public static readonly TripEventType CheckpointReached = new(nameof(CheckpointReached), 2);
    public static readonly TripEventType CosmicStormHit = new(nameof(CosmicStormHit), 5);
    public static readonly TripEventType DeliveryCompleted = new(nameof(DeliveryCompleted), 7);
    public static readonly TripEventType EmergencyMaintenanceRequired = new(nameof(EmergencyMaintenanceRequired), 6);
    public static readonly TripEventType IncidentOccurred = new(nameof(IncidentOccurred), 3);
    public static readonly TripEventType Other = new(nameof(Other), 99);
    public static readonly TripEventType TripCompleted = new(nameof(TripCompleted), 8);
    public static readonly TripEventType TripStarted = new(nameof(TripStarted), 1);

    private TripEventType(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        return List.Select(l => l.Name).ToArray();
    }
}