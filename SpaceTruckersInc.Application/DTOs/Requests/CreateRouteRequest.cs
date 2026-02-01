namespace SpaceTruckersInc.Application.DTOs.Requests;

public sealed record CreateRouteRequest
{
    public string Origin { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public TimeSpan EstimatedDuration { get; init; }
    public IReadOnlyList<string> Checkpoints { get; init; } = Array.Empty<string>();
}