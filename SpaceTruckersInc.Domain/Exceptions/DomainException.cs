namespace SpaceTruckersInc.Domain.Exceptions;

/// <summary>
/// Base class for domain-layer exceptions. Keeps domain code dependency-free and allows
/// infrastructure to translate domain failures to transport concerns.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string? message) : base(message)
    {
    }

    public DomainException() : base()
    {
    }

    public DomainException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}