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

public class DriverService : EntityService<Driver, DriverDto, IDriverRepository>, IDriverService
{
    public DriverService(IDriverRepository driverRepository, IMapper mapper, ICachingService cache, ILogger<DriverService> logger)
        : base(driverRepository, mapper, cache, logger)
    {
    }

    public override async Task<ServiceResponse<IEnumerable<DriverDto>?>> GetAllCachedAsync(string cacheKey = "drivers:all"
        , CancellationToken cancellationToken = default)
    {
        ServiceResponse<IEnumerable<DriverDto>?> response = new();
        try
        {
            ServiceResponse<IEnumerable<DriverDto>?> baseRes = await base.GetAllCachedAsync(cacheKey, cancellationToken);
            // Propagate base response (includes StatusCode/Errors/Data)
            response.Data = baseRes.Data;
            response.StatusCode = baseRes.StatusCode;
            response.Message = baseRes.Message;
            foreach (string e in baseRes.Errors)
            {
                response.Errors.Add(e);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch all drivers (cached) with cacheKey {CacheKey}.", cacheKey);

            const string friendlyErroMessage = "Failed to fetch drivers from cache.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            response.Data = null;
            return response;
        }
    }

    public async Task<ServiceResponse<DriverDto>> RegisterAsync(RegisterDriverRequest request, CancellationToken cancellationToken = default)
    {
        ServiceResponse<DriverDto> response = new();
        try
        {
            if (request is null)
            {
                response.Errors.Add("Request body is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                response.Errors.Add("Name is required.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            LicenseLevel? license = null;
            try
            {
                license = LicenseLevel.FromName(request.LicenseLevel ?? string.Empty, false);
            }
            catch
            {
                license = null;
            }

            if (license is null)
            {
                response.Errors.Add($"Invalid license level: '{request.LicenseLevel}'.");
                response.StatusCode = ServiceResponseStatus.BadRequest.Value;
                return response;
            }

            Driver driver = new(request.Name, license);

            Driver saved = await AddEntityAndSaveAsync(driver, "Driver {DriverId} registered.", driver.Id);
            response.Data = _mapper.Map<DriverDto>(saved);
            response.StatusCode = ServiceResponseStatus.Created.Value;
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid input when registering driver {Name}.", request?.Name);

            const string friendlyErroMessage = "Invalid input provided for driver registration.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation failed when registering driver {Name}.", request?.Name);

            const string friendlyErroMessage = "Driver registration failed validation rules.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.BadRequest.Value;
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register driver {Name} with license {LicenseLevel}.", request?.Name, request?.LicenseLevel);

            const string friendlyErroMessage = "An unexpected error occurred while registering the driver.";
            response.Errors.Add(friendlyErroMessage);
            response.Message = friendlyErroMessage;
            response.StatusCode = ServiceResponseStatus.InternalServerError.Value;
            return response;
        }
    }
}