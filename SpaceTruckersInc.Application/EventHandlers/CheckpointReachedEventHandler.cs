using MediatR;
using Microsoft.Extensions.Logging;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.Common.Interfaces;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Events;

namespace SpaceTruckersInc.Application.EventHandlers;

public sealed class CheckpointReachedEventHandler : INotificationHandler<CheckpointReachedEvent>
{
    private readonly ICachingService _cache;
    private readonly ILogger<CheckpointReachedEventHandler> _logger;
    private readonly IRouteService _routeService;
    private readonly ITripService _tripService;

    public CheckpointReachedEventHandler(
        ILogger<CheckpointReachedEventHandler> logger,
        ITripService tripService,
        IRouteService routeService,
        ICachingService cache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
        _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task Handle(CheckpointReachedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trip {TripId} reached checkpoint {Checkpoint} at {OccurredOn}",
            notification.TripId, notification.CheckpointName, notification.OccurredOn);

        try
        {
            ServiceResponse<TripSummaryDto?> tripRes = await _tripService.GetByIdAsync(notification.TripId, cancellationToken);
            if (!tripRes.IsSuccess || tripRes.Data is null)
            {
                _logger.LogWarning("Trip {TripId} not found when handling CheckpointReachedEvent.", notification.TripId);
                return;
            }

            TripSummaryDto tripDto = tripRes.Data;

            ServiceResponse<RouteDto?> routeRes = await _routeService.GetByIdAsync(tripDto.RouteId, cancellationToken);
            if (!routeRes.IsSuccess || routeRes.Data is null)
            {
                _logger.LogWarning("Route {RouteId} not found for trip {TripId} while handling CheckpointReachedEvent.", tripDto.RouteId, notification.TripId);
                return;
            }

            RouteDto routeDto = routeRes.Data;

            DateTime eta = tripDto.StartedOn is not null
                ? tripDto.StartedOn.Value + routeDto.EstimatedDuration
                : DateTime.UtcNow + routeDto.EstimatedDuration;

            if (routeDto.Checkpoints.Count > 0 && !string.IsNullOrWhiteSpace(notification.CheckpointName))
            {
                int idx = -1;
                for (int i = 0; i < routeDto.Checkpoints.Count; i++)
                {
                    if (string.Equals(routeDto.Checkpoints[i], notification.CheckpointName, StringComparison.OrdinalIgnoreCase))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                {
                    int remaining = Math.Max(0, routeDto.Checkpoints.Count - 1 - idx);
                    double fractionRemaining = (double)remaining / Math.Max(1, routeDto.Checkpoints.Count);
                    TimeSpan remainingSpan = TimeSpan.FromSeconds(routeDto.EstimatedDuration.TotalSeconds * fractionRemaining);
                    eta = DateTime.UtcNow + remainingSpan;
                }
            }

            _ = await _cache.GetOrAddCacheAsync(async () => (await _tripService.GetByIdAsync(tripDto.Id, cancellationToken)).Data, refreshCache: true, uniqueIdentity: $"trip:{tripDto.Id}");
            _ = await _cache.GetOrAddCacheAsync(async () => (await _routeService.GetByIdAsync(routeDto.Id, cancellationToken)).Data, refreshCache: true, uniqueIdentity: $"route:{routeDto.Id}");

            _logger.LogInformation("Updated projections for trip {TripId} (checkpoint {Checkpoint}). ETA approx {Eta:u}.", tripDto.Id, notification.CheckpointName, eta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle CheckpointReachedEvent for trip {TripId}.", notification.TripId);
            throw;
        }
    }
}