using MediatR;
using SpaceTruckersInc.Domain.Common.Interfaces;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Domain.Events;

/// <summary>
/// Generic incident event recorded against a Trip. Sub-types represent specific incident kinds.
/// </summary>
public sealed record IncidentOccurredEvent(
    Guid TripId,
    TripEventType IncidentType,
    string Details,
    DateTime OccurredOn
) : IDomainEvent, INotification;