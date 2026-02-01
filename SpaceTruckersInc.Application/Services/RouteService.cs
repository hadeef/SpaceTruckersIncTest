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
}