using SpaceTruckersInc.Domain.Common;

namespace SpaceTruckersInc.Domain.Entities;

public class RouteCheckpoint : Entity
{
    // Parameterless ctor for EF Core
    protected RouteCheckpoint() { }

    public RouteCheckpoint(string location, Guid routeId)
    {
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location is required.", nameof(location));
        Location = location.Trim();
        RouteId = routeId;
    }

    public Guid RouteId { get; private set; }
    public string Location { get; private set; } = string.Empty;
}