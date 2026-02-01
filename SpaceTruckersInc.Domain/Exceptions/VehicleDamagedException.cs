namespace SpaceTruckersInc.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to use a vehicle that is currently damaged / under maintenance.
/// </summary>
public class VehicleDamagedException : DomainException
{
    public VehicleDamagedException(Guid vehicleId, string? message = null)
        : base(message ?? $"Vehicle {vehicleId} is damaged and cannot be assigned.")
    {
        VehicleId = vehicleId;
    }

    public VehicleDamagedException(string? message) : base(message)
    {
    }

    public VehicleDamagedException() : base()
    {
    }

    public VehicleDamagedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public Guid VehicleId { get; }
}