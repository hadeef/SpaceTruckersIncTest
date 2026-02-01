namespace SpaceTruckersInc.Application.DTOs;

public sealed record RouteDto
{
    public Guid Id { get; init; }
    public string Origin { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public TimeSpan EstimatedDuration { get; init; }
    public IReadOnlyList<string> Checkpoints { get; init; } = Array.Empty<string>();
    public byte[]? RowVersion { get; init; }
}