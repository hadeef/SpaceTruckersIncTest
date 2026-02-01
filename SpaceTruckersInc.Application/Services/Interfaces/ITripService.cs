using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;

namespace SpaceTruckersInc.Application.Services.Interfaces;

public interface ITripService
{
    Task<ServiceResponse<TripSummaryDto>> AddAndSaveAsync(TripSummaryDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<TripSummaryDto>>> AddRangeAndSaveAsync(IEnumerable<TripSummaryDto> dtos, string logMessageTemplate
        , params object[] logArgs);

    Task<ServiceResponse<bool>> CancelTripAsync(Guid tripId, string reason, CancellationToken cancellationToken = default);

    Task<ServiceResponse<bool>> CompleteTripAsync(Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResponse<bool>> DeleteAndSaveAsync(Guid id, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<bool>> DeleteRangeAndSaveAsync(IEnumerable<Guid> ids, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<TripSummaryDto>?>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceResponse<IEnumerable<TripSummaryDto>?>> GetAllCachedAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task<ServiceResponse<TripSummaryDto?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceResponse<bool>> RecordEventAsync(RecordEventRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<TripSummaryDto>> StartTripAsync(CreateTripRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<TripSummaryDto>> UpdateAndSaveAsync(TripSummaryDto dto, string logMessageTemplate, params object[] logArgs);

    Task<ServiceResponse<IEnumerable<TripSummaryDto>>> UpdateRangeAndSaveAsync(IEnumerable<TripSummaryDto> dtos, string logMessageTemplate
        , params object[] logArgs);
}