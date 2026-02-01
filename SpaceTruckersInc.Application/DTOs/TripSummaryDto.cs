namespace SpaceTruckersInc.Application.DTOs;

public sealed record TripSummaryDto
{
    public Guid Id { get; init; }
    public Guid DriverId { get; init; }
    public Guid VehicleId { get; init; }
    public Guid RouteId { get; init; }

    public string CurrentStatus { get; init; } = string.Empty; //SmartEnum

    public IReadOnlyList<TripEventDto> Timeline { get; init; } = Array.Empty<TripEventDto>();
    public DateTime? StartedOn { get; init; }
    public DateTime? CompletedOn { get; init; }
    public byte[]? RowVersion { get; init; }
}