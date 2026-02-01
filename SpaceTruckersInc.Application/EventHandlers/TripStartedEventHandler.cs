using MediatR;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Events;

namespace SpaceTruckersInc.Application.EventHandlers;

public sealed class TripStartedEventHandler : INotificationHandler<TripStartedEvent>
{
    private readonly IDriverService _driverService;
    private readonly ILogger<TripStartedEventHandler> _logger;
    private readonly IVehicleService _vehicleService;

    public TripStartedEventHandler(
        IDriverService driverService,
        IVehicleService vehicleService,
        ILogger<TripStartedEventHandler> logger)
    {
        _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(TripStartedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            ServiceResponse<DriverDto?> driverRes = await _driverService.GetByIdAsync(notification.DriverId, cancellationToken);
            if (!driverRes.IsSuccess || driverRes.Data is null)
            {
                _logger.LogWarning(
                    "Driver {DriverId} not found when handling TripStartedEvent for trip {TripId}.",
                    notification.DriverId,
                    notification.TripId);
            }
            else
            {
                DriverDto updatedDriver = driverRes.Data with { Status = DriverStatus.OnTrip.Name };
                ServiceResponse<DriverDto> upd = await _driverService.UpdateAndSaveAsync(updatedDriver, "Driver {DriverId} marked on trip.", updatedDriver.Id);
                if (!upd.IsSuccess)
                {
                    _logger.LogWarning("Failed to update driver {DriverId} on trip {TripId}. {Errors}", updatedDriver.Id, notification.TripId, upd.ErrorsMessage);
                }
            }

            ServiceResponse<VehicleDto?> vehicleRes = await _vehicleService.GetByIdAsync(notification.VehicleId, cancellationToken);
            if (!vehicleRes.IsSuccess || vehicleRes.Data is null)
            {
                _logger.LogWarning(
                    "Vehicle {VehicleId} not found when handling TripStartedEvent for trip {TripId}.",
                    notification.VehicleId,
                    notification.TripId);
            }
            else
            {
                VehicleDto updatedVehicle = vehicleRes.Data with { Status = VehicleStatus.OnTrip.Name };
                ServiceResponse<VehicleDto> updV = await _vehicleService.UpdateAndSaveAsync(updatedVehicle, "Vehicle {VehicleId} assigned to trip.", updatedVehicle.Id);
                if (!updV.IsSuccess)
                {
                    _logger.LogWarning("Failed to update vehicle {VehicleId} on trip {TripId}. {Errors}", updatedVehicle.Id, notification.TripId, updV.ErrorsMessage);
                }
            }

            _logger.LogInformation(
                "Handled TripStartedEvent for trip {TripId}: driver {DriverId} on trip, vehicle {VehicleId} on trip.",
                notification.TripId,
                notification.DriverId,
                notification.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle TripStartedEvent for trip {TripId}.", notification.TripId);
            throw;
        }
    }
}