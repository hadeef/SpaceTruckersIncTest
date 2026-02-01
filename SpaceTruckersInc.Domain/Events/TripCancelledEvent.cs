using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a trip is cancelled.
/// </summary>
public sealed record TripCancelledEvent(
    Guid TripId,
    Guid DriverId,
    Guid VehicleId,
    string Reason,
    DateTime OccurredOn
) : IDomainEvent, INotification
{
    public TripEventType EventType => TripEventType.Other;
}