using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a Trip passes a route checkpoint.
/// </summary>
public sealed record CheckpointReachedEvent(
    Guid TripId,
    string CheckpointName,
    DateTime OccurredOn
) : IDomainEvent, INotification
{
    public TripEventType EventType => TripEventType.CheckpointReached;
}