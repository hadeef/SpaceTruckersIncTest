namespace SpaceTruckersInc.Application.DTOs.Requests;

public sealed record CreateVehicleRequest
{
    public string Model { get; init; } = string.Empty;
    public decimal CargoCapacity { get; init; }
}