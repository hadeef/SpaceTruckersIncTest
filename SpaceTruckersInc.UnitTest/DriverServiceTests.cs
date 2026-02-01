using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SpaceTruckersInc.Application.Common;
using SpaceTruckersInc.Application.Common.Interfaces;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Application.Mappings;
using SpaceTruckersInc.Application.Services;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Enums;
using SpaceTruckersInc.Domain.Interfaces;

namespace SpaceTruckersInc.UnitTest;

[TestClass]
public class DriverServiceTests
{
    private Mock<ICachingService> _cacheMock = null!;
    private Mock<IDriverRepository> _driverRepositoryMock = null!;
    private DriverService _driverService = null!;
    private ILoggerFactory _loggerFactory = null!;
    private Mock<ILogger<DriverService>> _loggerMock = null!;
    private IMapper _mapper = null!;

    [TestMethod]
    public async Task GetAllCachedAsync_WhenCacheMissFetchesAndReturnsDrivers()
    {
        // Arrange
        List<Driver> drivers =
        [
            new("John", LicenseLevel.Rookie),
            new("Leia", LicenseLevel.Veteran)
        ];

        _ = _driverRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(drivers);

        _ = _cacheMock
            .Setup(c => c.GetOrAddCacheAsync(
                It.IsAny<Func<Task<IEnumerable<DriverDto>?>>>(),
                It.IsAny<bool>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<string?>()))
            .Returns((Func<Task<IEnumerable<DriverDto>?>> fetch, bool _, TimeSpan? __, string? ___) => fetch());

        // Act
        ServiceResponse<IEnumerable<DriverDto>?> result = await _driverService.GetAllCachedAsync();

        // Assert
        Assert.AreEqual(ServiceResponseStatus.Success.Value, result.StatusCode);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(2, result.Data!.Count());
        _driverRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _cacheMock.Verify(c => c.GetOrAddCacheAsync(
            It.IsAny<Func<Task<IEnumerable<DriverDto>?>>>(),
            It.IsAny<bool>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<string?>()), Times.Once);
    }

    [TestMethod]
    public async Task RegisterAsync_WithInvalidLicenseLevel_ReturnsBadRequest()
    {
        // Arrange
        RegisterDriverRequest request = new() { Name = "Jane Doe", LicenseLevel = "InvalidLevel" };

        // Act
        ServiceResponse<DriverDto> result = await _driverService.RegisterAsync(request);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.BadRequest.Value, result.StatusCode);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid license level")));
        _driverRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Driver>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterAsync_WithMissingName_ReturnsBadRequest()
    {
        // Arrange
        RegisterDriverRequest request = new() { Name = "  ", LicenseLevel = LicenseLevel.Rookie.Name };

        // Act
        ServiceResponse<DriverDto> result = await _driverService.RegisterAsync(request);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.BadRequest.Value, result.StatusCode);
        CollectionAssert.Contains(result.Errors.ToList(), "Name is required.");
        _driverRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Driver>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterAsync_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        ServiceResponse<DriverDto> result = await _driverService.RegisterAsync(null!);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.BadRequest.Value, result.StatusCode);
        CollectionAssert.Contains(result.Errors.ToList(), "Request body is required.");
        _driverRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Driver>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterAsync_WithValidRequest_ReturnsCreatedAndPersistsDriver()
    {
        // Arrange
        RegisterDriverRequest request = new() { Name = "Han Solo", LicenseLevel = LicenseLevel.Veteran.Name };
        _ = _driverRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Driver>()))
            .ReturnsAsync((Driver d) => d);

        // Act
        ServiceResponse<DriverDto> result = await _driverService.RegisterAsync(request);

        // Assert
        Assert.AreEqual(ServiceResponseStatus.Created.Value, result.StatusCode);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(request.Name, result.Data!.Name);
        Assert.AreEqual(LicenseLevel.Veteran.Name, result.Data.LicenseLevel);
        _driverRepositoryMock.Verify(r => r.AddAsync(It.Is<Driver>(d => d.Name == request.Name)), Times.Once);
    }

    [TestInitialize]
    public void Setup()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>(MockBehavior.Strict);
        _cacheMock = new Mock<ICachingService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<DriverService>>();
        _loggerFactory = LoggerFactory.Create(builder => { });

        MapperConfiguration mapperConfig = new(cfg =>
        {
            cfg.AddProfile<DomainToDtoProfile>();
            cfg.AddProfile<DtoToDomainProfile>();
        }, _loggerFactory);

        _mapper = mapperConfig.CreateMapper();

        _driverService = new DriverService(_driverRepositoryMock.Object, _mapper, _cacheMock.Object, _loggerMock.Object);
    }
}