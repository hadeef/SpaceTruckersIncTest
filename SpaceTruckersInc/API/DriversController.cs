using Microsoft.AspNetCore.Mvc;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Application.Services.Interfaces;

namespace SpaceTruckersInc.API;

[ApiController]
[Route("api/[controller]/[action]")]
public sealed class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;
    private readonly ILogger<DriversController> _logger;

    public DriversController(IDriverService driverService, ILogger<DriversController> logger)
    {
        _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<bool> serviceResponse = await _driverService.DeleteAndSaveAsync(id, "Driver {DriverId} deleted.", id);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        ServiceResponse<IEnumerable<DriverDto>?> serviceResponse = await _driverService.GetAllAsync(cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        ServiceResponse<DriverDto?> serviceResponse = await _driverService.GetByIdAsync(id, cancellationToken);
        return serviceResponse.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDriverRequest request, CancellationToken cancellationToken)
    {
        ServiceResponse<DriverDto> serviceResponse = await _driverService.RegisterAsync(request, cancellationToken);
        return serviceResponse.ToActionResult(this, data => CreatedAtAction(nameof(GetById), new { id = data.Id }, data));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] DriverDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest(new { errors = new[] { "Id in route must match id in body." } });
        }

        ServiceResponse<DriverDto> serviceResponse = await _driverService.UpdateAndSaveAsync(dto, "Driver {DriverId} updated.", dto.Id);
        return serviceResponse.ToActionResult(this);
    }
}