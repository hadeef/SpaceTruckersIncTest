using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a delivery (trip) completes successfully.
/// </summary>
public sealed record DeliveryCompletedEvent(
    Guid TripId,
    Guid DriverId,
    Guid VehicleId,
    DateTime OccurredOn
) : IDomainEvent, INotification
{
    public TripEventType EventType => TripEventType.DeliveryCompleted;
}