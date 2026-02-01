using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a checkpoint is added to a route.
/// </summary>
public sealed record RouteCheckpointAddedEvent(
    Guid RouteId,
    string CheckpointName,
    DateTime OccurredOn
) : IDomainEvent, INotification;