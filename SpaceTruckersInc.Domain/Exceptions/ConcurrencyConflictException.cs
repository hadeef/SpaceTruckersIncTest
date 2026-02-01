namespace SpaceTruckersInc.Domain.Exceptions;

/// <summary>
/// Thrown when a concurrency token (RowVersion) mismatch is detected during an update.
/// Infrastructure can catch this and translate to 409 Conflict.
/// </summary>
public class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(Guid entityId, string? message = null)
        : base(message ?? $"Concurrency conflict detected for entity {entityId}.")
    {
        EntityId = entityId;
    }

    public ConcurrencyConflictException(string? message) : base(message)
    {
    }

    public ConcurrencyConflictException() : base()
    {
    }

    public ConcurrencyConflictException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public Guid EntityId { get; }
}