using AutoMapper;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Application.DTOs.Requests;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.Mappings;

public sealed class DtoToDomainProfile : Profile
{
    public DtoToDomainProfile()
    {
        #region Requests

        _ = CreateMap<CreateVehicleRequest, Vehicle>()
            .ConstructUsing(src => new Vehicle(VehicleModel.FromName(src.Model, false), src.CargoCapacity));

        _ = CreateMap<CreateDriverRequest, Driver>()
            .ConstructUsing(src => new Driver(src.Name, LicenseLevel.FromName(src.LicenseLevel, false)));

        _ = CreateMap<CreateRouteRequest, Route>()
            .ConstructUsing(src => new Route(src.Origin, src.Destination, src.EstimatedDuration, src.Checkpoints));

        #endregion Requests

        #region Entities

        _ = CreateMap<DriverDto, Driver>()
            .ConstructUsing(src => new Driver(src.Name, LicenseLevel.FromName(src.LicenseLevel, false)))
            .ForMember(d => d.LicenseLevel, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Status) ? DriverStatus.Available : DriverStatus.FromName(src.Status, false)))
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion));

        _ = CreateMap<VehicleDto, Vehicle>()
            .ConstructUsing(src => new Vehicle(VehicleModel.FromName(src.Model, false), src.CargoCapacity))
            .ForMember(d => d.Model, o => o.Ignore())
            .ForMember(d => d.CargoCapacity, o => o.Ignore())
            .ForMember(d => d.Condition, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion));

        _ = CreateMap<RouteDto, Route>()
            .ConstructUsing(src => new Route(src.Origin, src.Destination, src.EstimatedDuration, src.Checkpoints))
            .ForMember(d => d.Origin, o => o.Ignore())
            .ForMember(d => d.Destination, o => o.Ignore())
            .ForMember(d => d.EstimatedDuration, o => o.Ignore())
            .ForMember(d => d.Checkpoints, o => o.Ignore())
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => s.RowVersion));

        _ = CreateMap<TripEventDto, TripEvent>()
            .ConstructUsing(src => new TripEvent(
                src.Id == Guid.Empty ? Guid.NewGuid() : src.Id,
                src.OccurredOn == default ? DateTime.UtcNow : src.OccurredOn,
                string.IsNullOrWhiteSpace(src.EventType) ? TripEventType.Other : TripEventType.FromName(src.EventType, false),
                src.Details));

        #endregion Entities
    }
}