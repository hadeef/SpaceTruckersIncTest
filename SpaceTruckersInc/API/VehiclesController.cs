using Microsoft.AspNetCore.Mvc;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Application.Services.Interfaces;

namespace SpaceTruckersInc.API;

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class VehiclesController : ControllerBase
{
    private readonly ILogger<VehiclesController> _logger;
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<bool> serviceResponse = await _vehicleService.DeleteAndSaveAsync(id, "Vehicle {VehicleId} deleted.", id);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ServiceResponse<IEnumerable<VehicleDto>?> serviceResponse = await _vehicleService.GetAllAsync(cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<VehicleDto?> serviceResponse = await _vehicleService.GetByIdAsync(id, cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse<VehicleDto> serviceResponse = await _vehicleService.RegisterAsync(request, cancellationToken);
        return serviceResponse.ToActionResult(this, data => CreatedAtAction(nameof(GetById), new { id = data.Id }, data));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { errors = new[] { "Id in route must match id in body." } });
        }

        // NOTE: VehicleDto mapping intentionally ignores `Model` and `CargoCapacity`. Updates
        // performed through this endpoint will NOT change a vehicle's model or cargo capacity.

        ServiceResponse<VehicleDto> serviceResponse = await _vehicleService.UpdateAndSaveAsync(dto, "Vehicle {VehicleId} updated.", dto.Id);
        return serviceResponse.ToActionResult(this);
    }
}