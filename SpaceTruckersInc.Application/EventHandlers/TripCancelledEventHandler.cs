using MediatR;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Events;

namespace SpaceTruckersInc.Application.EventHandlers;

public sealed class TripCancelledEventHandler : INotificationHandler<TripCancelledEvent>
{
    private readonly IDriverService _driverService;
    private readonly ILogger<TripCancelledEventHandler> _logger;
    private readonly IVehicleService _vehicleService;

    public TripCancelledEventHandler(
        IDriverService driverService,
        IVehicleService vehicleService,
        ILogger<TripCancelledEventHandler> logger)
    {
        _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(TripCancelledEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            ServiceResponse<DriverDto?> driverRes = await _driverService.GetByIdAsync(notification.DriverId, cancellationToken);
            if (!driverRes.IsSuccess || driverRes.Data is null)
            {
                _logger.LogWarning("Driver {DriverId} not found when handling TripCancelledEvent for trip {TripId}.", notification.DriverId, notification.TripId);
            }
            else
            {
                DriverDto updatedDriver = driverRes.Data with { Status = DriverStatus.Available.Name };
                ServiceResponse<DriverDto> upd = await _driverService.UpdateAndSaveAsync(updatedDriver, "Driver {DriverId} marked available (cancel).", updatedDriver.Id);
                if (!upd.IsSuccess)
                {
                    _logger.LogWarning("Failed to update driver {DriverId} after cancel for trip {TripId}. {Errors}", updatedDriver.Id, notification.TripId, upd.ErrorsMessage);
                }
            }

            ServiceResponse<VehicleDto?> vehicleRes = await _vehicleService.GetByIdAsync(notification.VehicleId, cancellationToken);
            if (!vehicleRes.IsSuccess || vehicleRes.Data is null)
            {
                _logger.LogWarning("Vehicle {VehicleId} not found when handling TripCancelledEvent for trip {TripId}.", notification.VehicleId, notification.TripId);
            }
            else
            {
                VehicleDto updatedVehicle = vehicleRes.Data with { Status = VehicleStatus.Available.Name };
                ServiceResponse<VehicleDto> updV = await _vehicleService.UpdateAndSaveAsync(updatedVehicle, "Vehicle {VehicleId} released (cancel).", updatedVehicle.Id);
                if (!updV.IsSuccess)
                {
                    _logger.LogWarning("Failed to update vehicle {VehicleId} after cancel for trip {TripId}. {Errors}", updatedVehicle.Id, notification.TripId, updV.ErrorsMessage);
                }
            }

            _logger.LogInformation("Handled TripCancelledEvent for trip {TripId}: driver {DriverId} available, vehicle {VehicleId} released. Reason: {Reason}",
                notification.TripId,
                notification.DriverId,
                notification.VehicleId,
                notification.Reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle TripCancelledEvent for trip {TripId}.", notification.TripId);
            throw;
        }
    }
}