namespace SpaceTruckersInc.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to assign a vehicle that is already assigned to another trip.
/// </summary>
public class VehicleAlreadyOnTripException : DomainException
{
    public VehicleAlreadyOnTripException(Guid vehicleId, string? message = null)
        : base(message ?? $"Vehicle {vehicleId} is already assigned to an active trip.")
    {
        VehicleId = vehicleId;
    }

    public VehicleAlreadyOnTripException(string? message) : base(message)
    {
    }

    public VehicleAlreadyOnTripException() : base()
    {
    }

    public VehicleAlreadyOnTripException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public Guid VehicleId { get; }
}