using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a route's estimated duration changes.
/// </summary>
public sealed record RouteEstimatedDurationUpdatedEvent(
    Guid RouteId,
    TimeSpan PreviousDuration,
    TimeSpan NewDuration,
    DateTime OccurredOn
) : IDomainEvent, INotification;