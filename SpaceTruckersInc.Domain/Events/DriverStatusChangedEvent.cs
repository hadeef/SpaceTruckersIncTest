using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a driver's status changes (e.g., Available , OnTrip).
/// </summary>
public sealed record DriverStatusChangedEvent(
    Guid DriverId,
    DriverStatus PreviousStatus,
    DriverStatus NewStatus,
    DateTime OccurredOn
) : IDomainEvent, INotification;