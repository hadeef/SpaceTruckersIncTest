using AutoMapper;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.Common.Interfaces;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Exceptions;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.Application.Services;

public class RouteService : EntityService<Route, RouteDto, IRouteRepository>, IRouteService
{
    public RouteService(IRouteRepository routeRepository, IMapper mapper, ICachingService cache, ILogger<RouteService> logger)
        : base(routeRepository, mapper, cache, logger)
    {
    }

    public async Task<ServiceResponse<RouteDto>> CreateAsync(CreateRouteRequest request, CancellationToken cancellationToken = default)
    {
        ServiceResponse<RouteDto> response = new();
        try
        {
            if (request is null)
            {
                const string friendlyErroMessage = "Request body is required.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
            {
                const string friendlyErroMessage = "Origin and destination are required.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            Route route = new(request.Origin, request.Destination, request.EstimatedDuration, request.Checkpoints);
            Route saved = await AddEntityAndSaveAsync(route, "Route {RouteId} created.", route.Id);
            response.Data = _mapper.Map<RouteDto>(saved);
            response.StatusCode = ServiceResponseStatus.Created.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while creating route from {Origin} to {Destination}", request?.Origin, request?.Destination);
            const string friendlyErroMessage = "Concurrency conflict occurred while creating the route. Please retry.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when creating route from {Origin} to {Destination}", request?.Origin, request?.Destination);
            const string friendlyErroMessage = "Invalid route data provided.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed when creating route from {Origin} to {Destination}", request?.Origin, request?.Destination);
            const string friendlyErroMessage = "Route creation failed validation rules.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create route from {Origin} to {Destination}", request?.Origin, request?.Destination);
            const string friendlyErroMessage = "An unexpected error occurred while creating the route.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }

    public override async Task<ServiceResponse<IEnumerable<RouteDto>?>> GetAllCachedAsync(string cacheKey = "routes:all"
        , CancellationToken cancellationToken = default)
    {
        ServiceResponse<IEnumerable<RouteDto>?> response = new();
        try
        {
            ServiceResponse<IEnumerable<RouteDto>?> baseRes = await base.GetAllCachedAsync(cacheKey, cancellationToken);
            response.Data = baseRes.Data;
            response.StatusCode = baseRes.StatusCode;
            foreach (string e in baseRes.Errors)
            {
                response.Errors.Add(e);
            }

            response.Message = baseRes.Message;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when fetching cached routes with cacheKey {CacheKey}.", cacheKey);
            const string friendlyErroMessage = "Concurrency conflict occurred while fetching cached routes.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            response.Data = null;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch all routes (cached).");
            const string friendlyErroMessage = "An unexpected error occurred while fetching cached routes.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = null;
            return response;
        }
    }

    public override async Task<ServiceResponse<RouteDto>> UpdateAndSaveAsync(RouteDto dto, string logMessageTemplate, params object[] logArgs)
    {
        ServiceResponse<RouteDto> response = new();
        try
        {
            if (dto is null)
            {
                response.Errors.Add("Request body is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            if (dto.Id == Guid.Empty)
            {
                response.Errors.Add("Id is required for update operations.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            Route? existing = await _repository.GetByIdAsync(dto.Id);
            if (existing is null)
            {
                response.Errors.Add("Entity not found.");
                response.StatusCode = ServiceResponseStatus.NotFound.Value;
                return response;
            }

            ApplyRouteDtoToEntity(dto, existing);

            Route saved = await UpdateEntityAndSaveAsync(existing, logMessageTemplate, logArgs);

            _logger.LogInformation(logMessageTemplate, logArgs);
            response.Data = _mapper.Map<RouteDto>(saved);
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while updating route {RouteId}.", dto?.Id);
            ServiceResponse<RouteDto> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while updating the entity.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            return conflictResponse;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when updating route {RouteId}.", dto?.Id);
            response.Errors.Add("Invalid route data provided.");
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed when updating route {RouteId}.", dto?.Id);
            response.Errors.Add("Route update failed validation rules.");
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAndSaveAsync failed for Route id {RouteId}.", dto?.Id);
            const string friendlyErroMessage = "An unexpected error occurred while updating the route.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }

    private void ApplyRouteDtoToEntity(RouteDto src, Route dest)
    {
        try
        {
            if (src.EstimatedDuration.TotalSeconds >= 0 && src.EstimatedDuration != dest.EstimatedDuration)
            {
                dest.UpdateEstimatedDuration(src.EstimatedDuration);
            }
        }
        catch
        {
            _logger.LogWarning("Failed to update EstimatedDuration for Route {RouteId}.", dest.Id);
        }

        try
        {
            List<string> existingCheckpoints = dest.Checkpoints.ToList();
            IReadOnlyList<string> incomingCheckpoints = src.Checkpoints ?? Array.Empty<string>();

            foreach (string cp in incomingCheckpoints)
            {
                if (!existingCheckpoints.Contains(cp, StringComparer.Ordinal))
                {
                    dest.AddCheckpoint(cp);
                }
            }

            foreach (string cp in existingCheckpoints)
            {
                if (!incomingCheckpoints.Contains(cp, StringComparer.Ordinal))
                {
                    _ = dest.RemoveCheckpoint(cp);
                }
            }
        }
        catch
        {
            _logger.LogWarning("Failed to update Checkpoints for Route {RouteId}.", dest.Id);
        }
    }
}