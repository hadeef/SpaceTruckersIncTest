using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a driver is renamed.
/// </summary>
public sealed record DriverRenamedEvent(
    Guid DriverId,
    string OldName,
    string NewName,
    DateTime OccurredOn
) : IDomainEvent, INotification;