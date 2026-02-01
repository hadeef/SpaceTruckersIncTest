using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a checkpoint is removed from a route.
/// </summary>
public sealed record RouteCheckpointRemovedEvent(
    Guid RouteId,
    string CheckpointName,
    DateTime OccurredOn
) : IDomainEvent, INotification;