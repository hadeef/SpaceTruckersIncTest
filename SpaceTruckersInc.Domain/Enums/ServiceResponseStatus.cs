using Ardalis.SmartEnum;

namespace SpaceTruckersInc.Domain.Enums;

public sealed class ServiceResponseStatus : SmartEnum<ServiceResponseStatus, int>
{
    public static readonly ServiceResponseStatus BadRequest = new(nameof(BadRequest), 400);
    public static readonly ServiceResponseStatus Conflict = new(nameof(Conflict), 409);
    public static readonly ServiceResponseStatus Created = new(nameof(Created), 201);
    public static readonly ServiceResponseStatus Forbidden = new(nameof(Forbidden), 403);
    public static readonly ServiceResponseStatus InternalServerError = new(nameof(InternalServerError), 500);
    public static readonly ServiceResponseStatus NoContent = new(nameof(NoContent), 204);
    public static readonly ServiceResponseStatus NotFound = new(nameof(NotFound), 404);
    public static readonly ServiceResponseStatus Success = new(nameof(Success), 200);
    public static readonly ServiceResponseStatus TooManyRequest = new(nameof(TooManyRequest), 429);
    public static readonly ServiceResponseStatus Unauthorized = new(nameof(Unauthorized), 401);

    private ServiceResponseStatus(string name, int value) : base(name, value)
    {
    }

    public static IReadOnlyCollection<string> GetNames()
    {
        return List.Select(l => l.Name).ToArray();
    }
}