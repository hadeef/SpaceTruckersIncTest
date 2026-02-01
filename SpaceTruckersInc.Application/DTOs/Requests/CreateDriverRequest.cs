namespace SpaceTruckersInc.Application.DTOs.Requests;

public sealed record CreateDriverRequest
{
    public string Name { get; init; } = string.Empty;
    public string LicenseLevel { get; init; } = string.Empty;
}