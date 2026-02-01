namespace SpaceTruckersInc.Application.DTOs.Requests;

public sealed record RecordEventRequest
{
    public Guid TripId { get; init; }
    public string EventType { get; init; } = string.Empty; // SmartEnum name
    public string? Details { get; init; }
}