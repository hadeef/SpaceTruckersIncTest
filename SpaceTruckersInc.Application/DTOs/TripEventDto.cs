namespace SpaceTruckersInc.Application.DTOs;

public sealed record TripEventDto
{
    public Guid Id { get; init; }
    public DateTime OccurredOn { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? Details { get; init; }
}