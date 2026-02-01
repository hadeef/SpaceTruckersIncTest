using SpaceTruckersInc.Domain.Common.Interfaces;

namespace SpaceTruckersInc.Domain.Common;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity()
    {
        Id = Guid.NewGuid();
        CreateTime = DateTime.UtcNow;
    }

    public DateTime CreateTime { get; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Primary identifier for the entity. Using <see cref="Guid"/> gives a globally-unique,
    /// client-side generated id (no DB round-trip), which is convenient for distributed systems and
    /// for creating entities before persisting. If you rely on clustered DB indexes, consider
    /// sequential/sortable GUIDs to reduce index fragmentation.
    /// </summary>
    public Guid Id { get; protected init; }

    public byte[]? RowVersion { get; set; }
    public DateTime? UpdateTime { get; set; }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}