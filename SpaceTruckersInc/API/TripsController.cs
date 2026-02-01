using Microsoft.AspNetCore.Mvc;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Application.Services.Interfaces;

namespace SpaceTruckersInc.API;

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class TripsController : ControllerBase
{
    private readonly ILogger<TripsController> _logger;
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService, ILogger<TripsController> logger)
    {
        _tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<IActionResult> Start([FromBody] CreateTripRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse<TripSummaryDto> serviceResponse = await _tripService.StartTripAsync(request, cancellationToken);
        return serviceResponse.ToActionResult(this, data => CreatedAtAction(nameof(GetById), new { id = data.Id }, data));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        ServiceResponse<bool> serviceResponse = await _tripService.CancelTripAsync(id, reason, cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<bool> serviceResponse = await _tripService.CompleteTripAsync(id, cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpPost("record")]
    public async Task<IActionResult> RecordEvent([FromBody] RecordEventRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse<bool> serviceResponse = await _tripService.RecordEventAsync(request, cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<bool> serviceResponse = await _tripService.DeleteAndSaveAsync(id, "Trip {TripId} deleted.", id);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ServiceResponse<IEnumerable<TripSummaryDto>?> serviceResponse = await _tripService.GetAllAsync(cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<TripSummaryDto?> serviceResponse = await _tripService.GetByIdAsync(id, cancellationToken);
        return serviceResponse.ToActionResult(this);
    }
}