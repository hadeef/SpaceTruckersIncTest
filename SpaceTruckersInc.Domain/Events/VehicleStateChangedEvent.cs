using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Fired when a vehicle's status or condition changes.
/// </summary>
public sealed record VehicleStateChangedEvent(
    Guid VehicleId,
    VehicleStatus PreviousStatus,
    VehicleStatus NewStatus,
    VehicleCondition PreviousCondition,
    VehicleCondition NewCondition,
    DateTime OccurredOn
) : IDomainEvent, INotification;