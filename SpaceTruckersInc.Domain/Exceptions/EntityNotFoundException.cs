namespace SpaceTruckersInc.Domain.Exceptions;

/// <summary>
/// Thrown when a requested domain entity cannot be found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(Type entityType, object? key = null)
        : base(FormMessage(entityType, key))
    {
        EntityType = entityType;
        Key = key;
    }

    public EntityNotFoundException(string? message) : base(message)
    {
    }

    public EntityNotFoundException() : base()
    {
    }

    public EntityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public Type EntityType { get; }
    public object? Key { get; }

    private static string FormMessage(Type entityType, object? key)
    {
        return key is null
                ? $"Entity of type '{entityType.Name}' was not found."
                : $"Entity of type '{entityType.Name}' with key '{key}' was not found.";
    }
}