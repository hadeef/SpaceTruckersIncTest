using MediatR;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Events;

namespace SpaceTruckersInc.Application.EventHandlers;

public sealed class DeliveryCompletedEventHandler : INotificationHandler<DeliveryCompletedEvent>
{
    private readonly IDriverService _driverService;
    private readonly ILogger<DeliveryCompletedEventHandler> _logger;
    private readonly IVehicleService _vehicleService;

    public DeliveryCompletedEventHandler(
        IDriverService driverService,
        IVehicleService vehicleService,
        ILogger<DeliveryCompletedEventHandler> logger)
    {
        _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeliveryCompletedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            ServiceResponse<DriverDto?> driverRes = await _driverService.GetByIdAsync(notification.DriverId, cancellationToken);
            if (!driverRes.IsSuccess || driverRes.Data is null)
            {
                _logger.LogWarning(
                    "Driver {DriverId} not found when handling DeliveryCompletedEvent for trip {TripId}.",
                    notification.DriverId,
                    notification.TripId);
            }
            else
            {
                DriverDto updatedDriver = driverRes.Data with { Status = DriverStatus.Available.Name };
                ServiceResponse<DriverDto> updateRes = await _driverService.UpdateAndSaveAsync(updatedDriver, "Driver {DriverId} marked available.", updatedDriver.Id);
                if (!updateRes.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to update driver {DriverId} after DeliveryCompletedEvent for trip {TripId}. Errors: {Errors}",
                        updatedDriver.Id,
                        notification.TripId,
                        updateRes.ErrorsMessage);
                }
            }

            ServiceResponse<VehicleDto?> vehicleRes = await _vehicleService.GetByIdAsync(notification.VehicleId, cancellationToken);
            if (!vehicleRes.IsSuccess || vehicleRes.Data is null)
            {
                _logger.LogWarning(
                    "Vehicle {VehicleId} not found when handling DeliveryCompletedEvent for trip {TripId}.",
                    notification.VehicleId,
                    notification.TripId);
            }
            else
            {
                VehicleDto updatedVehicle = vehicleRes.Data with { Status = VehicleStatus.Available.Name };
                ServiceResponse<VehicleDto> updateVRes = await _vehicleService.UpdateAndSaveAsync(updatedVehicle, "Vehicle {VehicleId} released from trip.", updatedVehicle.Id);
                if (!updateVRes.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to update vehicle {VehicleId} after DeliveryCompletedEvent for trip {TripId}. Errors: {Errors}",
                        updatedVehicle.Id,
                        notification.TripId,
                        updateVRes.ErrorsMessage);
                }
            }

            _logger.LogInformation(
                "Handled DeliveryCompletedEvent for trip {TripId}: driver {DriverId} available, vehicle {VehicleId} released.",
                notification.TripId,
                notification.DriverId,
                notification.VehicleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle DeliveryCompletedEvent for trip {TripId}.", notification.TripId);
            throw;
        }
    }
}