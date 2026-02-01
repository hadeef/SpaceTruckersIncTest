namespace SpaceTruckersInc.Application.DTOs;

public sealed record DriverDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string LicenseLevel { get; init; } = string.Empty; // SmartEnum
    public string Status { get; init; } = string.Empty;
    public byte[]? RowVersion { get; init; }
}