using SpaceTruckersInc.Domain.Common;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Events;
using SpaceTruckersInc.Domain.Exceptions;

namespace SpaceTruckersInc.Domain.Entities;

public sealed record TripEvent(Guid Id, DateTime OccurredOn, TripEventType EventType, string? Details);

public class Trip : Entity
{
    private readonly List<TripEvent> _tripEvents = [];

    private Trip(Guid driverId, Guid vehicleId, Guid routeId)
    {
        DriverId = driverId;
        VehicleId = vehicleId;
        RouteId = routeId;
        CurrentStatus = TripStatus.Pending;
    }

    public TripStatus CurrentStatus { get; private set; }
    public Guid DriverId { get; private set; }
    public Guid RouteId { get; private set; }
    public IReadOnlyCollection<TripEvent> TripEvents => _tripEvents.AsReadOnly();
    public Guid VehicleId { get; private set; }

    public static Trip Create(Guid driverId, Guid vehicleId, Guid routeId)
    {
        return driverId == Guid.Empty
            ? throw new DomainException("DriverId is required.")
            : vehicleId == Guid.Empty
            ? throw new DomainException("VehicleId is required.")
            : routeId == Guid.Empty ? throw new DomainException("RouteId is required.") : new Trip(driverId, vehicleId, routeId);
    }

    public void CancelTrip(string reason)
    {
        if (CurrentStatus.Equals(TripStatus.Completed))
        {
            throw new InvalidTripStateException(Id, "Cannot cancel a completed trip.");
        }

        CurrentStatus = TripStatus.Cancelled;
        DateTime occurredOn = DateTime.UtcNow;
        TripEvent entry = new(Guid.NewGuid(), occurredOn, TripEventType.Other, $"Cancelled: {reason}");
        _tripEvents.Add(entry);
        RaiseDomainEvent(new TripCancelledEvent(Id, DriverId, VehicleId, reason, occurredOn));
        UpdateTime = occurredOn;
    }

    public void CompleteTrip()
    {
        if (!CurrentStatus.Equals(TripStatus.InProgress))
        {
            throw new InvalidTripStateException(Id, "Only an InProgress trip may be completed.");
        }

        CurrentStatus = TripStatus.Completed;
        DateTime occurredOn = DateTime.UtcNow;
        TripEvent entry = new(Guid.NewGuid(), occurredOn, TripEventType.DeliveryCompleted, "Delivery completed");
        _tripEvents.Add(entry);
        RaiseDomainEvent(new DeliveryCompletedEvent(Id, DriverId, VehicleId, occurredOn));
        UpdateTime = occurredOn;
    }

    public void RecordCheckpoint(string checkpointName)
    {
        if (!CurrentStatus.Equals(TripStatus.InProgress))
        {
            throw new InvalidTripStateException(Id, "Can only record checkpoints while trip is InProgress.");
        }

        DateTime occurredOn = DateTime.UtcNow;
        TripEvent entry = new(Guid.NewGuid(), occurredOn, TripEventType.CheckpointReached, checkpointName);
        _tripEvents.Add(entry);
        RaiseDomainEvent(new CheckpointReachedEvent(Id, checkpointName, occurredOn));
        UpdateTime = occurredOn;
    }

    public void RecordIncident(TripEventType incidentType, string details)
    {
        if (!CurrentStatus.Equals(TripStatus.InProgress))
        {
            throw new InvalidTripStateException(Id, "Can only record incidents while trip is InProgress.");
        }

        DateTime occurredOn = DateTime.UtcNow;
        TripEvent entry = new(Guid.NewGuid(), occurredOn, incidentType, details);
        _tripEvents.Add(entry);
        RaiseDomainEvent(new IncidentOccurredEvent(Id, incidentType, details, occurredOn));
        UpdateTime = occurredOn;
    }

    public void StartTrip()
    {
        if (!CurrentStatus.Equals(TripStatus.Pending))
        {
            throw new InvalidTripStateException(Id, "Trip can only be started from Pending state.");
        }

        CurrentStatus = TripStatus.InProgress;
        DateTime occurredOn = DateTime.UtcNow;
        TripEvent entry = new(Guid.NewGuid(), occurredOn, TripEventType.TripStarted, "Trip started");
        _tripEvents.Add(entry);
        RaiseDomainEvent(new TripStartedEvent(Id, DriverId, VehicleId, RouteId, occurredOn));
        UpdateTime = occurredOn;
    }
}