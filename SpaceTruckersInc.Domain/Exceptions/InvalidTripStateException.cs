namespace SpaceTruckersInc.Domain.Exceptions;

public class InvalidTripStateException : DomainException
{
    public InvalidTripStateException(Guid tripId, string? message = null)
        : base(message ?? $"Operation not allowed for Trip {tripId}.")
    {
        TripId = tripId;
    }

    public InvalidTripStateException(string? message) : base(message)
    {
    }

    public InvalidTripStateException() : base()
    {
    }

    public InvalidTripStateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public Guid TripId { get; }
}