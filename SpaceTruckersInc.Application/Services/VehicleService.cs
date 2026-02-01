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

    public override async Task<ServiceResponse<VehicleDto>> UpdateAndSaveAsync(VehicleDto dto, string logMessageTemplate
        , params object[] logArgs)
    {
        ServiceResponse<VehicleDto> response = new();
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

            Vehicle? existingVehicle = await _repository.GetByIdAsync(dto.Id);
            if (existingVehicle is null)
            {
                response.Errors.Add("Entity not found.");
                response.StatusCode = ServiceResponseStatus.NotFound.Value;
                return response;
            }

            ApplyVehicleDtoToEntity(dto, existingVehicle);

            Vehicle saved = await UpdateEntityAndSaveAsync(existingVehicle, logMessageTemplate, logArgs);

            _logger.LogInformation(logMessageTemplate, logArgs);
            response.Data = _mapper.Map<VehicleDto>(saved);
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict while updating vehicle {VehicleId}.", dto?.Id);
            ServiceResponse<VehicleDto> conflictResponse = new();
            conflictResponse.Errors.Add("Concurrency conflict occurred while updating the entity.");
            conflictResponse.StatusCode = ServiceResponseStatus.Conflict.Value;
            return conflictResponse;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when updating vehicle {VehicleId}.", dto?.Id);
            response.Errors.Add("Invalid vehicle data provided.");
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed when updating vehicle {VehicleId}.", dto?.Id);
            response.Errors.Add("Vehicle update failed validation rules.");
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAndSaveAsync failed for Vehicle id {VehicleId}.", dto?.Id);
            const string friendlyErroMessage = "An unexpected error occurred while updating the vehicle.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }

    private void ApplyVehicleDtoToEntity(VehicleDto src, Vehicle dest)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(src.Condition))
            {
                VehicleCondition condition = VehicleCondition.FromName(src.Condition, false);
                if (condition == VehicleCondition.Damaged && dest.Condition != VehicleCondition.Damaged)
                {
                    dest.MarkDamaged(); // sets Maintenance status
                }
                else if (condition == VehicleCondition.Functional && dest.Condition != VehicleCondition.Functional)
                {
                    dest.Repair(); // sets Available status
                }
            }
        }
        catch
        {
            _logger.LogWarning("Failed to apply Condition from VehicleDto to Vehicle entity for VehicleId {VehicleId}.", dest.Id);
        }

        // Apply status with guard: damaged must not be available
        try
        {
            if (!string.IsNullOrWhiteSpace(src.Status))
            {
                VehicleStatus status = VehicleStatus.FromName(src.Status, false);

                if (status == VehicleStatus.OnTrip && dest.Status != VehicleStatus.OnTrip)
                {
                    dest.AssignToTrip();
                }
                else if (status == VehicleStatus.Available && dest.Condition != VehicleCondition.Damaged && dest.Status != VehicleStatus.Available)
                {
                    dest.ReleaseFromTrip();
                }
                else if (status == VehicleStatus.Maintenance && dest.Status != VehicleStatus.Maintenance)
                {
                    // Keep condition + status aligned to maintenance
                    if (dest.Condition != VehicleCondition.Damaged)
                    {
                        dest.MarkDamaged(); // sets Maintenance
                    }
                    else
                    {
                        dest.MarkDamaged(); // already damaged; keeps Maintenance
                    }
                }
                else if (status == VehicleStatus.Available && dest.Condition == VehicleCondition.Damaged)
                {
                    // Requested Available but condition is Damaged: enforce maintenance
                    dest.MarkDamaged();
                }
            }
        }
        catch
        {
            _logger.LogWarning("Failed to apply Status from VehicleDto to Vehicle entity for VehicleId {VehicleId}.", dest.Id);
        }
    }
}