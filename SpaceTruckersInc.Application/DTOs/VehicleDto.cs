namespace SpaceTruckersInc.Application.DTOs;

public sealed record VehicleDto
{
    public Guid Id { get; init; }
    public string Model { get; init; } = string.Empty;
    public decimal CargoCapacity { get; init; }
    public string Condition { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public byte[]? RowVersion { get; init; }
}