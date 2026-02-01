using AutoMapper;
using SpaceTruckersInc.Application.DTOs;
using SpaceTruckersInc.Domain.Entities;
using SpaceTruckersInc.Domain.Enums;

namespace SpaceTruckersInc.Application.Mappings;

public sealed class DomainToDtoProfile : Profile
{
    public DomainToDtoProfile()
    {
        _ = CreateMap<Driver, DriverDto>()
            .ForMember(d => d.LicenseLevel, o => o.MapFrom(s => s.LicenseLevel.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.Name));

        _ = CreateMap<Vehicle, VehicleDto>()
            .ForMember(d => d.Model, o => o.MapFrom(s => s.Model.Name))
            .ForMember(d => d.Condition, o => o.MapFrom(s => s.Condition.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.Name));

        _ = CreateMap<Route, RouteDto>()
            .ForMember(d => d.Checkpoints, o => o.MapFrom(s => s.Checkpoints.ToList()));

        _ = CreateMap<TripEvent, TripEventDto>()
            .ForMember(d => d.EventType, o => o.MapFrom(s => s.EventType.Name))
            .ForMember(d => d.Details, o => o.MapFrom(s => s.Details));

        _ = CreateMap<Trip, TripSummaryDto>()
            .ForMember(d => d.CurrentStatus, o => o.MapFrom(s => s.CurrentStatus.Name))
            .ForMember(d => d.Timeline, o => o.MapFrom(s => s.TripEvents))
            .ForMember(
                d => d.StartedOn,
                o => o.MapFrom(s =>
                    s.TripEvents.FirstOrDefault(t => t.EventType == TripEventType.TripStarted) != null
                        ? s.TripEvents.FirstOrDefault(t => t.EventType == TripEventType.TripStarted)!.OccurredOn
                        : (DateTime?)null
                )
            )
            .ForMember(
                d => d.CompletedOn,
                o => o.MapFrom(s =>
                    s.TripEvents.FirstOrDefault(t => t.EventType == TripEventType.DeliveryCompleted) != null
                        ? s.TripEvents.FirstOrDefault(t => t.EventType == TripEventType.DeliveryCompleted)!.OccurredOn
                        : (DateTime?)null
                )
            );
    }
}