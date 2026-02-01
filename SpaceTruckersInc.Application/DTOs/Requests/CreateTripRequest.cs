namespace SpaceTruckersInc.Application.DTOs.Requests;

public sealed record CreateTripRequest
{
    public Guid DriverId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid RouteId { get; init; }
}