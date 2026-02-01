using SpaceTruckersInc.Domain.Common;
using SpaceTruckersInc.Domain.Events;
using System.Collections.ObjectModel;

namespace SpaceTruckersInc.Domain.Entities;

public class Route : Entity
{
    private readonly List<RouteCheckpoint> _checkpoints = [];

    public Route(string origin, string destination, TimeSpan estimatedDuration, IEnumerable<string>? checkpoints = null)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException("Origin is required.", nameof(origin));
        }
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("Destination is required.", nameof(destination));
        }
        Origin = origin.Trim();
        Destination = destination.Trim();

        EstimatedDuration = estimatedDuration.TotalSeconds < 0
                            ? throw new ArgumentException("estimatedDuration.TotalSeconds must be non-negative.", nameof(estimatedDuration))
                            : estimatedDuration;

        if (checkpoints != null)
        {
            foreach (string cp in checkpoints)
            {
                if (!string.IsNullOrWhiteSpace(cp))
                {
                    _checkpoints.Add(new RouteCheckpoint(cp.Trim(), Id));
                }
            }
        }
    }

    // Parameterless ctor for EF Core
    protected Route()
    {
    }

    // Expose checkpoint locations (read-only)
    public IReadOnlyCollection<string> Checkpoints => new ReadOnlyCollection<string>(_checkpoints.Select(c => c.Location).ToList());

    public string Destination { get; private set; } = string.Empty;
    public TimeSpan EstimatedDuration { get; private set; }
    public string Origin { get; private set; } = string.Empty;

    public void AddCheckpoint(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("Location is required.", nameof(location));
        }

        string trimmed = location.Trim();
        // prevent duplicates (domain decision)
        if (_checkpoints.Any(c => string.Equals(c.Location, trimmed, StringComparison.Ordinal)))
        {
            return;
        }

        var checkpoint = new RouteCheckpoint(trimmed, Id);
        _checkpoints.Add(checkpoint);

        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new RouteCheckpointAddedEvent(Id, trimmed, occurredOn));
    }

    public bool RemoveCheckpoint(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return false;
        }

        string trimmed = location.Trim();
        RouteCheckpoint? toRemove = _checkpoints.FirstOrDefault(c => string.Equals(c.Location, trimmed, StringComparison.Ordinal));
        if (toRemove is null)
        {
            return false;
        }

        bool removed = _checkpoints.Remove(toRemove);
        if (removed)
        {
            DateTime occurredOn = DateTime.UtcNow;
            UpdateTime = occurredOn;
            RaiseDomainEvent(new RouteCheckpointRemovedEvent(Id, trimmed, occurredOn));
        }

        return removed;
    }

    public void UpdateEstimatedDuration(TimeSpan newDuration)
    {
        if (newDuration.TotalSeconds < 0)
        {
            throw new ArgumentException("newDuration.TotalSeconds must be non-negative.", nameof(newDuration));
        }

        TimeSpan previous = EstimatedDuration;
        if (newDuration == previous)
        {
            return;
        }

        EstimatedDuration = newDuration;
        DateTime occurredOn = DateTime.UtcNow;
        UpdateTime = occurredOn;
        RaiseDomainEvent(new RouteEstimatedDurationUpdatedEvent(Id, previous, newDuration, occurredOn));
    }
}