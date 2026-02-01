using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.Common.Interfaces;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Application.Mappings;
using SpaceTruckersInc.Application.Services;
using SpaceTruckersInc.Application.Services.Interfaces;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.UnitTest;

[TestClass]
public sealed class TripServiceTests
{
    private Mock<ICachingService> _cacheMock = null!;
    private Mock<IDriverService> _driverServiceMock = null!;
    private ILoggerFactory _loggerFactory = null!;
    private Mock<ILogger<TripService>> _loggerMock = null!;
    private IMapper _mapper = null!;
    private Mock<ITripRepository> _tripRepositoryMock = null!;
    private TripService _tripService = null!;
    private Mock<IVehicleService> _vehicleServiceMock = null!;

    [TestMethod]
    public async Task CancelTripAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        Guid tripId = Guid.NewGuid();

        _ = _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync((Trip?)null);

        // Act
        ServiceResponse<bool> result = await _tripService.CancelTripAsync(tripId, "reason");

        // Assert
        Assert.AreEqual(ServiceResponseStatus.NotFound.Value, result.StatusCode);
        Assert.IsFalse(result.Data);
        _tripRepositoryMock.Verify(r => r.GetByIdAsync(tripId), Times.Once);
        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Never);
    }

    [TestMethod]
    public async Task CancelTripAsync_WithExistingTrip_ReturnsSuccess()
    {
        // Arrange
        Guid tripId = Guid.NewGuid();
        Guid driverId = Guid.NewGuid();
        Guid vehicleId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();

        Trip trip = Trip.Create(driverId, vehicleId, routeId);
        trip.StartTrip(); // make it cancellable

        _ = _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync((Trip?)null); // first ensure not found behavior will be covered in another test

        // Now setup for success path (GetByIdAsync should be called with actual Id)
        _tripRepositoryMock.Reset();
        _ = _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(trip.Id))
            .ReturnsAsync(trip);

        _ = _tripRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Trip>()))
            .ReturnsAsync((Trip t) => t);

        // Act
        ServiceResponse<bool> result = await _tripService.CancelTripAsync(trip.Id, "no longer needed");

        // Assert
        Assert.AreEqual(ServiceResponseStatus.Success.Value, result.StatusCode);
        Assert.IsTrue(result.Data);
        _tripRepositoryMock.Verify(r => r.GetByIdAsync(trip.Id), Times.Once);
        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Once);
    }

    [TestMethod]
    public async Task CompleteTripAsync_WithNotInProgress_ReturnsBadRequest()
    {
        // Arrange
        Guid driverId = Guid.NewGuid();
        Guid vehicleId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();
        Trip trip = Trip.Create(driverId, vehicleId, routeId); // still Pending

        _ = _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(trip.Id))
            .ReturnsAsync(trip);

        // Act
        ServiceResponse<bool> result = await _tripService.CompleteTripAsync(trip.Id);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.BadRequest.Value, result.StatusCode);
        Assert.IsFalse(result.Data);
        _tripRepositoryMock.Verify(r => r.GetByIdAsync(trip.Id), Times.Once);
        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Never);
    }

    [TestMethod]
    public async Task RecordEventAsync_Checkpoint_OnInProgressTrip_Succeeds()
    {
        // Arrange
        Guid driverId = Guid.NewGuid();
        Guid vehicleId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();

        Trip trip = Trip.Create(driverId, vehicleId, routeId);
        trip.StartTrip(); // InProgress

        _ = _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(trip.Id))
            .ReturnsAsync(trip);

        _ = _tripRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Trip>()))
            .ReturnsAsync((Trip t) => t);

        RecordEventRequest req = new() { TripId = trip.Id, EventType = TripEventType.CheckpointReached.Name, Details = "CP-1" };

        // Act
        ServiceResponse<bool> result = await _tripService.RecordEventAsync(req);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.Success.Value, result.StatusCode);
        Assert.IsTrue(result.Data);
        _tripRepositoryMock.Verify(r => r.GetByIdAsync(trip.Id), Times.Once);
        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Once);
    }

    [TestMethod]
    public async Task RecordEventAsync_InvalidEventType_ReturnsBadRequest()
    {
        // Arrange
        Guid tripId = Guid.NewGuid();
        RecordEventRequest req = new() { TripId = tripId, EventType = "NonExistingEvent", Details = "x" };

        Trip existing = Trip.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _ = _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId))
            .ReturnsAsync(existing);

        // Act
        ServiceResponse<bool> result = await _tripService.RecordEventAsync(req);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.BadRequest.Value, result.StatusCode);
        Assert.IsFalse(result.Data);
        _tripRepositoryMock.Verify(r => r.GetByIdAsync(tripId), Times.Once);
        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>()), Times.Never);
    }

    [TestInitialize]
    public void Setup()
    {
        _tripRepositoryMock = new Mock<ITripRepository>(MockBehavior.Strict);
        _driverServiceMock = new Mock<IDriverService>(MockBehavior.Strict);
        _vehicle_service_init();
        _cacheMock = new Mock<ICachingService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<TripService>>();
        _loggerFactory = LoggerFactory.Create(builder => { });

        // Configure AutoMapper the same way application does (profiles)
        MapperConfiguration mapperConfig = new(cfg =>
        {
            cfg.AddProfile<DomainToDtoProfile>();
            cfg.AddProfile<DtoToDomainProfile>();
        }, _loggerFactory);

        _mapper = mapperConfig.CreateMapper();

        _tripService = new TripService(
            _tripRepositoryMock.Object,
            _driverServiceMock.Object,
            _vehicleServiceMock.Object,
            _mapper,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task StartTripAsync_WhenDriverMissing_ReturnsNotFound()
    {
        // Arrange
        CreateTripRequest req = new() { DriverId = Guid.NewGuid(), VehicleId = Guid.NewGuid(), RouteId = Guid.NewGuid() };

        _ = _driverServiceMock
            .Setup(s => s.GetByIdAsync(req.DriverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceResponse<DriverDto?> { Data = null, StatusCode = ServiceResponseStatus.NotFound.Value });

        // Act
        ServiceResponse<TripSummaryDto> result = await _tripService.StartTripAsync(req);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.NotFound.Value, result.StatusCode);
        _driverServiceMock.Verify(s => s.GetByIdAsync(req.DriverId, It.IsAny<CancellationToken>()), Times.Once);
        _vehicleServiceMock.VerifyNoOtherCalls();
        _tripRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Trip>()), Times.Never);
    }

    [TestMethod]
    public async Task StartTripAsync_WithValidRequest_CreatesTrip()
    {
        // Arrange
        Guid driverId = Guid.NewGuid();
        Guid vehicleId = Guid.NewGuid();
        Guid routeId = Guid.NewGuid();

        CreateTripRequest createReq = new()
        {
            DriverId = driverId,
            VehicleId = vehicleId,
            RouteId = routeId
        };

        // driver and vehicle must exist
        DriverDto driverDto = new()
        {
            Id = driverId,
            Name = "Driver",
            LicenseLevel = LicenseLevel.Rookie.Name,
            Status = DriverStatus.Available.Name
        };
        VehicleDto vehicleDto = new()
        {
            Id = vehicleId,
            Model = VehicleModel.HoverTruck.Name,
            CargoCapacity = 100,
            Status = VehicleStatus.Available.Name,
            Condition = VehicleCondition.Functional.Name
        };

        _ = _driverServiceMock
            .Setup(s => s.GetByIdAsync(driverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceResponse<DriverDto?> { Data = driverDto, StatusCode = ServiceResponseStatus.Success.Value });

        _ = _vehicleServiceMock
            .Setup(s => s.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceResponse<VehicleDto?> { Data = vehicleDto, StatusCode = ServiceResponseStatus.Success.Value });

        _ = _tripRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Trip>()))
            .ReturnsAsync((Trip t) => t);

        // Act
        ServiceResponse<TripSummaryDto> result = await _tripService.StartTripAsync(createReq);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.Created.Value, result.StatusCode);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(driverId, result.Data!.DriverId);
        Assert.AreEqual(vehicleId, result.Data.VehicleId);
        _driverServiceMock.Verify(s => s.GetByIdAsync(driverId, It.IsAny<CancellationToken>()), Times.Once);
        _vehicleServiceMock.Verify(s => s.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()), Times.Once);
        _tripRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Trip>()), Times.Once);
    }

    // small helper to keep variable naming consistent in Setup
    private void _vehicle_service_init()
    {
        _vehicleServiceMock = new Mock<IVehicleService>(MockBehavior.Strict);
    }
}