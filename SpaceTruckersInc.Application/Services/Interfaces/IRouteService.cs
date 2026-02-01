using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;

namespace SpaceTruckersInc.Application.Services.Interfaces;

public interface IRouteService
{
    Task<ServiceResponse<RouteDto>> AddAndSaveAsync(RouteDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<RouteDto>>> AddRangeAndSaveAsync(IEnumerable<RouteDto> dtos, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<RouteDto>> CreateAsync(CreateRouteRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<bool>> DeleteAndSaveAsync(Guid id, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<bool>> DeleteRangeAndSaveAsync(IEnumerable<Guid> ids, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<RouteDto>?>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceResponse<IEnumerable<RouteDto>?>> GetAllCachedAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task<ServiceResponse<RouteDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<RouteDto>> UpdateAndSaveAsync(RouteDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<RouteDto>>> UpdateRangeAndSaveAsync(IEnumerable<RouteDto> dtos, string logMessageTemplate, params object[] logArgs);
}