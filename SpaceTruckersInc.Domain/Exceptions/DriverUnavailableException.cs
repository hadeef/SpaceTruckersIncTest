namespace SpaceTruckersInc.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to assign a driver that is not available.
/// </summary>
public class DriverUnavailableException : DomainException
{
    public DriverUnavailableException(Guid driverId, string? message = null)
        : base(message ?? $"Driver {driverId} is not available for assignment.")
    {
        DriverId = driverId;
    }

    public DriverUnavailableException(string? message) : base(message)
    {
    }

    public DriverUnavailableException() : base()
    {
    }

    public DriverUnavailableException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public Guid DriverId { get; }
}