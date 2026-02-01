using Microsoft.AspNetCore.Mvc;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Application.Services.Interfaces;

namespace SpaceTruckersInc.API;

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class RoutesController : ControllerBase
{
    private readonly ILogger<RoutesController> _logger;
    private readonly IRouteService _routeService;

    public RoutesController(IRouteService routeService, ILogger<RoutesController> logger)
    {
        _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRouteRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse<RouteDto> serviceResponse = await _routeService.CreateAsync(request, cancellationToken);
        return serviceResponse.ToActionResult(this, data => CreatedAtAction(nameof(GetById), new { id = data.Id }, data));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<bool> serviceResponse = await _routeService.DeleteAndSaveAsync(id, "Route {RouteId} deleted.", id);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ServiceResponse<IEnumerable<RouteDto>?> serviceResponse = await _routeService.GetAllAsync(cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<RouteDto?> serviceResponse = await _routeService.GetByIdAsync(id, cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RouteDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { errors = new[] { "Id in route must match id in body." } });
        }

        // NOTE: Origin and Destination are not changed directly by this Update endpoint. The Route
        // mapping intentionally ignores direct assignment of Origin/Destination to preserve domain invariants.

        ServiceResponse<RouteDto> serviceResponse = await _routeService.UpdateAndSaveAsync(dto, "Route {RouteId} updated.", dto.Id);
        return serviceResponse.ToActionResult(this);
    }
}