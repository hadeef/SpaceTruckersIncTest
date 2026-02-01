using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.Common;

public class ServiceResponse<T>
{
    public T? Data { get; set; }
    public IList<string> Errors { get; set; } = [];
    public string ErrorsMessage => string.Join(";", Errors);
    public bool IsSuccess => Errors.Count == 0;
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; } = ServiceResponseStatus.Success.Value;
}