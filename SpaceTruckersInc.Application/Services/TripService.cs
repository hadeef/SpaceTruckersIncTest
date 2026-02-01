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

public class TripService : EntityService<Trip, TripSummaryDto, ITripRepository>, ITripService
{
    private readonly IDriverService _driverService;
    private readonly IVehicleService _vehicleService;

    public TripService(
        ITripRepository tripRepository,
        IDriverService driverService,
        IVehicleService vehicleService,
        IMapper mapper,
        ICachingService cache,
        ILogger<TripService> logger)
        : base(tripRepository, mapper, cache, logger)
    {
        _driverService = driverService;
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
    }

    public async Task<ServiceResponse<bool>> CancelTripAsync(Guid tripId, string reason, CancellationToken cancellationToken = default)
    {
        ServiceResponse<bool> response = new();
        try
        {
            Trip trip = await FindEntityByIdAsync(tripId) ?? throw new EntityNotFoundException(typeof(Trip), tripId);

            trip.CancelTrip(reason);
            _ = await UpdateEntityAndSaveAsync(trip, "Trip {TripId} cancelled. Reason: {Reason}", trip.Id, reason);

            response.Data = true;
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (EntityNotFoundException)
        {
            const string friendlyErroMessage = "Trip not found.";
            _logger.LogWarning("CancelTripAsync: {Message} TripId:{TripId}", friendlyErroMessage, tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.NotFound.Value;
            response.Data = false;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            const string friendlyErroMessage = "Concurrency conflict occurred while cancelling the trip. Please retry.";
            _logger.LogWarning(ex, "Concurrency conflict while cancelling trip {TripId}", tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            response.Data = false;
            return response;
        }
        catch (DomainException ex)
        {
            const string friendlyErroMessage = "Cannot cancel trip in its current state.";
            _logger.LogWarning(ex, "Domain error cancelling trip {TripId}", tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            response.Data = false;
            return response;
        }
        catch (Exception ex)
        {
            const string friendlyErroMessage = "An unexpected error occurred while cancelling the trip.";
            _logger.LogError(ex, "Failed to cancel trip {TripId}.", tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = false;
            return response;
        }
    }

    public async Task<ServiceResponse<bool>> CompleteTripAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        ServiceResponse<bool> response = new();
        try
        {
            Trip trip = await FindEntityByIdAsync(tripId) ?? throw new EntityNotFoundException(typeof(Trip), tripId);

            trip.CompleteTrip();
            _ = await UpdateEntityAndSaveAsync(trip, "Trip {TripId} completed.", trip.Id);

            response.Data = true;
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (EntityNotFoundException)
        {
            const string friendlyErroMessage = "Trip not found.";
            _logger.LogWarning("CompleteTripAsync: {Message} TripId:{TripId}", friendlyErroMessage, tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.NotFound.Value;
            response.Data = false;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            const string friendlyErroMessage = "Concurrency conflict occurred while completing the trip. Please retry.";
            _logger.LogWarning(ex, "Concurrency conflict while completing trip {TripId}", tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            response.Data = false;
            return response;
        }
        catch (DomainException ex)
        {
            const string friendlyErroMessage = "Cannot complete trip in its current state.";
            _logger.LogWarning(ex, "Domain error completing trip {TripId}", tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            response.Data = false;
            return response;
        }
        catch (Exception ex)
        {
            const string friendlyErroMessage = "An unexpected error occurred while completing the trip.";
            _logger.LogError(ex, "Failed to complete trip {TripId}.", tripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = false;
            return response;
        }
    }

    public async Task<ServiceResponse<bool>> RecordEventAsync(RecordEventRequest request, CancellationToken cancellationToken = default)
    {
        ServiceResponse<bool> response = new();
        try
        {
            if (request is null)
            {
                const string friendlyErroMessage = "Request body is required.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                response.Data = false;
                return response;
            }

            Trip trip = await FindEntityByIdAsync(request.TripId) ?? throw new EntityNotFoundException(typeof(Trip), request.TripId);

            TripEventType eventType;
            try
            {
                eventType = TripEventType.FromName(request.EventType, true);
            }
            catch
            {
                const string friendlyErroMessage = "Invalid event type.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                response.Data = false;
                return response;
            }

            if (eventType == TripEventType.CheckpointReached)
            {
                trip.RecordCheckpoint(request.Details ?? "Checkpoint");
            }
            else if (eventType == TripEventType.DeliveryCompleted)
            {
                trip.CompleteTrip();
            }
            else
            {
                trip.RecordIncident(eventType, request.Details ?? "Incident");
            }

            _ = await UpdateEntityAndSaveAsync(trip, "Trip {TripId} recorded event {EventType}.", trip.Id, eventType.Name);

            response.Data = true;
            response.StatusCode = ServiceResponseStatus.Success.Value;
            return response;
        }
        catch (EntityNotFoundException)
        {
            const string friendlyErroMessage = "Trip not found.";
            _logger.LogWarning("RecordEventAsync: {Message} TripId:{TripId}", friendlyErroMessage, request?.TripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.NotFound.Value;
            response.Data = false;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            const string friendlyErroMessage = "Concurrency conflict occurred while recording the event. Please retry.";
            _logger.LogWarning(ex, "Concurrency conflict recording event for trip {TripId}", request?.TripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            response.Data = false;
            return response;
        }
        catch (DomainException ex)
        {
            const string friendlyErroMessage = "Invalid operation for the trip's current state.";
            _logger.LogWarning(ex, "Domain error recording event for trip {TripId}", request?.TripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            response.Data = false;
            return response;
        }
        catch (Exception ex)
        {
            const string friendlyErroMessage = "An unexpected error occurred while recording the event.";
            _logger.LogError(ex, "Failed to record event for trip {TripId}.", request?.TripId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = false;
            return response;
        }
    }

    public async Task<ServiceResponse<TripSummaryDto>> StartTripAsync(CreateTripRequest request
        , CancellationToken cancellationToken = default)
    {
        ServiceResponse<TripSummaryDto> response = new();
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

            _logger.LogInformation("Starting trip Driver:{DriverId} Vehicle:{VehicleId} Route:{RouteId}",
                request.DriverId,
                request.VehicleId,
                request.RouteId);

            // Use services to validate existence (services return DTOs)
            ServiceResponse<DriverDto?> driverDtoServiceResponse = await _driverService.GetByIdAsync(request.DriverId, cancellationToken);
            DriverDto? driverDto = driverDtoServiceResponse.Data;
            if (driverDto is null)
            {
                const string friendlyErroMessage = "Driver not found.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.NotFound.Value;
                return response;
            }

            ServiceResponse<VehicleDto?> vehicleDtoServiceResponse = await _vehicleService.GetByIdAsync(request.VehicleId, cancellationToken);
            VehicleDto? vehicleDto = vehicleDtoServiceResponse.Data;
            if (vehicleDto is null)
            {
                const string friendlyErroMessage = "Vehicle not found.";
                response.Errors.Add(friendlyErroMessage);
                response.Message = friendlyErroMessage;
                response.StatusCode = ServiceResponseStatus.NotFound.Value;
                return response;
            }

            Trip trip = Trip.Create(request.DriverId, request.VehicleId, request.RouteId);
            trip.StartTrip();

            Trip saved = await AddEntityAndSaveAsync(trip, "Trip {TripId} started.", trip.Id);
            response.Data = _mapper.Map<TripSummaryDto>(saved);
            response.StatusCode = ServiceResponseStatus.Created.Value;
            return response;
        }
        catch (ConcurrencyConflictException ex)
        {
            const string friendlyErroMessage = "Concurrency conflict occurred while starting the trip. Please retry.";
            _logger.LogWarning(ex, "Concurrency conflict starting trip Driver:{DriverId} Vehicle:{VehicleId} Route:{RouteId}",
                request?.DriverId, request?.VehicleId, request?.RouteId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.Conflict.Value;
            return response;
        }
        catch (DomainException ex)
        {
            const string friendlyErroMessage = "Trip cannot be started due to domain validation.";
            _logger.LogWarning(ex, "Domain validation failed starting trip Driver:{DriverId} Vehicle:{VehicleId} Route:{RouteId}",
                request?.DriverId, request?.VehicleId, request?.RouteId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (ArgumentException ex)
        {
            const string friendlyErroMessage = "Invalid trip data provided.";
            _logger.LogWarning(ex, "Invalid input starting trip Driver:{DriverId} Vehicle:{VehicleId} Route:{RouteId}",
                request?.DriverId, request?.VehicleId, request?.RouteId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (Exception ex)
        {
            const string friendlyErroMessage = "An unexpected error occurred while starting the trip.";
            _logger.LogError(ex, "Failed to start trip Driver:{DriverId} Vehicle:{VehicleId} Route:{RouteId}",
                request?.DriverId, request?.VehicleId, request?.RouteId);
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }
}