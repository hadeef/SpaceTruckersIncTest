using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;

namespace SpaceTruckersInc.Application.Services.Interfaces;

public interface IVehicleService
{
    Task<ServiceResponse<VehicleDto>> AddAndSaveAsync(VehicleDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<VehicleDto>>> AddRangeAndSaveAsync(IEnumerable<VehicleDto> dtos, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<bool>> DeleteAndSaveAsync(Guid id, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<bool>> DeleteRangeAndSaveAsync(IEnumerable<Guid> ids, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<VehicleDto>?>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceResponse<IEnumerable<VehicleDto>?>> GetAllCachedAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VehicleDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VehicleDto>> RegisterAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<VehicleDto>> UpdateAndSaveAsync(VehicleDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<VehicleDto>>> UpdateRangeAndSaveAsync(IEnumerable<VehicleDto> dtos, string logMessageTemplate, params object[] logArgs);
}