using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;

namespace SpaceTruckersInc.Application.Services.Interfaces;

public interface IDriverService
{
    Task<ServiceResponse<DriverDto>> AddAndSaveAsync(DriverDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<DriverDto>>> AddRangeAndSaveAsync(IEnumerable<DriverDto> dtos, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<bool>> DeleteAndSaveAsync(Guid id, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<bool>> DeleteRangeAndSaveAsync(IEnumerable<Guid> ids, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<DriverDto>?>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceResponse<IEnumerable<DriverDto>?>> GetAllCachedAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task<ServiceResponse<DriverDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<DriverDto>> RegisterAsync(RegisterDriverRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<DriverDto>> UpdateAndSaveAsync(DriverDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<DriverDto>>> UpdateRangeAndSaveAsync(IEnumerable<DriverDto> dtos, string logMessageTemplate, params object[] logArgs);
}