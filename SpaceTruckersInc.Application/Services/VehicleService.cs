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

public class VehicleService : EntityService<Vehicle, VehicleDto, IVehicleRepository>, IVehicleService
{
    public VehicleService(IVehicleRepository vehicleRepository, IMapper mapper, ICachingService cache, ILogger<VehicleService> logger)
        : base(vehicleRepository, mapper, cache, logger)
    {
    }

    public override async Task<ServiceResponse<IEnumerable<VehicleDto>?>> GetAllCachedAsync(string cacheKey = "vehicles:all"
        , CancellationToken cancellationToken = default)
    {
        ServiceResponse<IEnumerable<VehicleDto>?> response = new();
        try
        {
            ServiceResponse<IEnumerable<VehicleDto>?> baseRes = await base.GetAllCachedAsync(cacheKey, cancellationToken);
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
            _logger.LogWarning(ex, "Concurrency conflict when fetching cached vehicles with cacheKey {CacheKey}.", cacheKey);
            const string friendlyErroMessage = "Concurrency conflict occurred while fetching cached vehicles. Please retry.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            response.Data = null;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch all vehicles (cached).");
            const string friendlyErroMessage = "An unexpected error occurred while fetching cached vehicles.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = null;
            return response;
        }
    }

    public async Task<ServiceResponse<VehicleDto>> RegisterAsync(CreateVehicleRequest request, CancellationToken cancellationToken = default)
    {
        ServiceResponse<VehicleDto> response = new();
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

            if (string.IsNullOrWhiteSpace(request.Model))
            {
                const string friendlyErroMessage = "Vehicle model is required.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            // Resolve SmartEnum safely
            VehicleModel? model;
            try
            {
                model = VehicleModel.FromName(request.Model, true);
            }
            catch
            {
                model = null;
            }

            if (model is null)
            {
                const string friendlyErroMessage = "Invalid vehicle model.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            Vehicle vehicle = new(model, request.CargoCapacity);

            Vehicle saved = await AddEntityAndSaveAsync(vehicle, "Vehicle {VehicleId} registered.", vehicle.Id);
            response.Data = _mapper.Map<VehicleDto>(saved);
            response.StatusCode = ServiceResponseStatus.Created.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while registering vehicle. Model: {Model}, Capacity: {Capacity}", request?.Model, request?.CargoCapacity);
            const string friendlyErroMessage = "Concurrency conflict occurred while registering the vehicle. Please retry.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when registering vehicle. Model: {Model}, Capacity: {Capacity}", request?.Model, request?.CargoCapacity);
            const string friendlyErroMessage = "Invalid vehicle data provided.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed when registering vehicle. Model: {Model}, Capacity: {Capacity}", request?.Model, request?.CargoCapacity);
            const string friendlyErroMessage = "Vehicle registration failed validation rules.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register vehicle. Model: {Model}, Capacity: {Capacity}", request?.Model, request?.CargoCapacity);
            const string friendlyErroMessage = "An unexpected error occurred while registering the vehicle.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }
}