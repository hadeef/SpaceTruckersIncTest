using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a Trip is started (driver leaves origin).
/// </summary>
public sealed record TripStartedEvent(
    Guid TripId,
    Guid DriverId,
    Guid VehicleId,
    Guid RouteId,
    DateTime OccurredOn
) : IDomainEvent, INotification
{
    public TripEventType EventType => TripEventType.TripStarted;
}