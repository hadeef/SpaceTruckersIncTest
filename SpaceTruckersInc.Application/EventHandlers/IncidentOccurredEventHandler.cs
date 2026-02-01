using MediatR;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Events;

namespace SpaceTruckersInc.Application.EventHandlers;

public sealed class IncidentOccurredEventHandler : INotificationHandler<IncidentOccurredEvent>
{
    private readonly IDriverService _driverService;
    private readonly ILogger<IncidentOccurredEventHandler> _logger;
    private readonly ITripService _tripService;
    private readonly IVehicleService _vehicleService;

    public IncidentOccurredEventHandler(
        IDriverService driverService,
        IVehicleService vehicleService,
        ITripService tripService,
        ILogger<IncidentOccurredEventHandler> logger)
    {
        _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(IncidentOccurredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            ServiceResponse<TripSummaryDto?> tripRes = await _tripService.GetByIdAsync(notification.TripId, cancellationToken);
            if (!tripRes.IsSuccess || tripRes.Data is null)
            {
                _logger.LogWarning("Trip {TripId} not found when handling IncidentOccurredEvent.", notification.TripId);
                return;
            }

            TripSummaryDto tripDto = tripRes.Data;

            bool isDamagingIncident =
                notification.IncidentType == TripEventType.EmergencyMaintenanceRequired ||
                notification.IncidentType == TripEventType.AsteroidFieldEncountered ||
                notification.IncidentType == TripEventType.CosmicStormHit;

            if (isDamagingIncident)
            {
                ServiceResponse<VehicleDto?> vehicleRes = await _vehicleService.GetByIdAsync(tripDto.VehicleId, cancellationToken);
                if (!vehicleRes.IsSuccess || vehicleRes.Data is null)
                {
                    _logger.LogWarning("Vehicle {VehicleId} not found when handling IncidentOccurredEvent for trip {TripId}.", tripDto.VehicleId, tripDto.Id);
                }
                else
                {
                    VehicleDto updatedVehicle = vehicleRes.Data with
                    {
                        Condition = VehicleCondition.Damaged.Name,
                        Status = VehicleStatus.Maintenance.Name
                    };
                    ServiceResponse<VehicleDto> vUpd = await _vehicleService.UpdateAndSaveAsync(updatedVehicle, "Vehicle {VehicleId} marked damaged.", updatedVehicle.Id);
                    if (!vUpd.IsSuccess)
                    {
                        _logger.LogWarning("Failed to update vehicle {VehicleId} after incident for trip {TripId}. {Errors}", updatedVehicle.Id, tripDto.Id, vUpd.ErrorsMessage);
                    }
                }

                ServiceResponse<DriverDto?> driverRes = await _driverService.GetByIdAsync(tripDto.DriverId, cancellationToken);
                if (!driverRes.IsSuccess || driverRes.Data is null)
                {
                    _logger.LogWarning("Driver {DriverId} not found when handling IncidentOccurredEvent for trip {TripId}.", tripDto.DriverId, tripDto.Id);
                }
                else
                {
                    DriverDto updatedDriver = driverRes.Data with { Status = DriverStatus.Available.Name };
                    ServiceResponse<DriverDto> dUpd = await _driverService.UpdateAndSaveAsync(updatedDriver, "Driver {DriverId} marked available due to incident.", updatedDriver.Id);
                    if (!dUpd.IsSuccess)
                    {
                        _logger.LogWarning("Failed to update driver {DriverId} after incident for trip {TripId}. {Errors}", updatedDriver.Id, tripDto.Id, dUpd.ErrorsMessage);
                    }
                }
            }
            else
            {
                _logger.LogInformation("Non-damaging incident for trip {TripId}: {Details}", notification.TripId, notification.Details);
            }

            _logger.LogInformation("Handled IncidentOccurredEvent for trip {TripId} (type {IncidentType}).", notification.TripId, notification.IncidentType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle IncidentOccurredEvent for trip {TripId}.", notification.TripId);
            throw;
        }
    }
}