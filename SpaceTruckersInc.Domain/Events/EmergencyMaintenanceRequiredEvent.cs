using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Specific incident: emergency maintenance required (e.g., vehicle failure).
/// </summary>
public sealed record EmergencyMaintenanceRequiredEvent(
    Guid TripId,
    string Details,
    DateTime OccurredOn
) : IDomainEvent, INotification
{
    public TripEventType IncidentType => TripEventType.EmergencyMaintenanceRequired;
}